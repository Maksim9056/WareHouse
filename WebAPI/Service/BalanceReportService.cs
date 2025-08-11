using ClassLibrary.Date;
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace WebAPI.Service
{
    public enum TypeDocKind
    {
        Receipt = 1, // Поступление
        Shipment = 2  // Отгрузка
    }

    public class BalanceReportService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        // если когда-нибудь потребуется учитывать и «Отозван» — поставь true
        private const bool IncludeRevokedInBalance = false;
        private const string SignedName = "Подписан";
        private const string RevokedName = "Отозван";

        public BalanceReportService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        private static string CacheKey(int[]? resourceIds, int[]? unitIds)
        {
            static string Part(int[]? a) =>
                (a is { Length: > 0 }) ? string.Join(',', a.Distinct().OrderBy(x => x)) : "all";
            return $"balances:r={Part(resourceIds)}:u={Part(unitIds)}";
        }

        // --- кэшируем Id нужных состояний ---
        private async Task<int[]> GetCountableConditionIdsAsync(CancellationToken ct)
        {
            const string key = "balances:cond-ids";
            if (_cache.TryGetValue(key, out int[]? cached) && cached is { Length: > 0 })
                return cached;

            var q = _db.Condition.AsNoTracking().Where(c => c.Name == SignedName);
            if (IncludeRevokedInBalance)
                q = q.Where(c => c.Name == SignedName || c.Name == RevokedName);

            // EF не переварит условие выше как есть, поэтому соберём явно:
            var ids = await _db.Condition.AsNoTracking()
                .Where(c => IncludeRevokedInBalance
                        ? (c.Name == SignedName || c.Name == RevokedName)
                        : (c.Name == SignedName))
                .Select(c => c.Id)
                .ToArrayAsync(ct);

            _cache.Set(key, ids, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });

            return ids;
        }

        public async Task<List<BalanceRowDto>> GetBalancesAsync(
            int[]? resourceIds,
            int[]? unitIds,
            CancellationToken ct = default)
        {
            var key = CacheKey(resourceIds, unitIds);
            if (_cache.TryGetValue(key, out List<BalanceRowDto>? cached))
                return cached;

            // берём допустимые состояния
            var allowedCondIds = await GetCountableConditionIdsAsync(ct);

            // строки документов + фильтруем по типу и состоянию
            var q = from dr in _db.Document_resource.AsNoTracking()
                    join d in _db.Document.AsNoTracking() on dr.DocumentId equals d.Id
                    where (d.TypeDocId == (int)TypeDocKind.Receipt || d.TypeDocId == (int)TypeDocKind.Shipment)
                          && allowedCondIds.Contains(d.ConditionId)
                    select new { dr.ResourceId, dr.UnitId, dr.Count, d.TypeDocId };

            if (resourceIds is { Length: > 0 })
                q = q.Where(x => resourceIds!.Contains(x.ResourceId));
            if (unitIds is { Length: > 0 })
                q = q.Where(x => unitIds!.Contains(x.UnitId));

            var grouped = from x in q
                          group x by new { x.ResourceId, x.UnitId } into g
                          select new
                          {
                              g.Key.ResourceId,
                              g.Key.UnitId,
                              Quantity = g.Sum(z =>
                                  (long)z.Count *
                                  (z.TypeDocId == (int)TypeDocKind.Receipt ? 1 : -1))
                          };

            var data = await (from g in grouped
                              join r in _db.Resource.AsNoTracking() on g.ResourceId equals r.Id
                              join u in _db.Unit.AsNoTracking() on g.UnitId equals u.Id
                              orderby r.Name, u.Name
                              select new BalanceRowDto
                              {
                                  ResourceId = g.ResourceId,
                                  ResourceName = r.Name,
                                  UnitId = g.UnitId,
                                  UnitName = u.Name,
                                  Quantity = g.Quantity
                              }).ToListAsync(ct);

            _cache.Set(key, data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            });

            return data;
        }

        // вызывать после успешного сохранения документов/строк ИЛИ смены состояния документа
        public void InvalidateBalancesCache(int[]? resourceIds = null, int[]? unitIds = null)
        {
            _cache.Remove("balances:cond-ids"); // на случай переименования статусов
            _cache.Remove(CacheKey(null, null));
            _cache.Remove(CacheKey(resourceIds, null));
            _cache.Remove(CacheKey(null, unitIds));
            _cache.Remove(CacheKey(resourceIds, unitIds));
        }
    }
}
