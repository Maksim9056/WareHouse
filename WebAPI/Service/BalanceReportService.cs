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

        public async Task<List<BalanceRowDto>> GetBalancesAsync(
            int[]? resourceIds,
            int[]? unitIds,
            CancellationToken ct = default)
        {
            var key = CacheKey(resourceIds, unitIds);
            //if (_cache.TryGetValue(key, out List<BalanceRowDto> cached))
            //    return cached;

            if (_cache.TryGetValue(key, out List<BalanceRowDto>? cached))
            {

                Console.WriteLine($"Кэш их памяти ключа {key} ");
                return cached;
            }


            // Позиции документов + документ (только Поступление/Отгрузка)
            var q = from dr in _db.Document_resource.AsNoTracking()
                    join d in _db.Document.AsNoTracking() on dr.DocumentId equals d.Id
                    where d.TypeDocId == (int)TypeDocKind.Receipt || d.TypeDocId == (int)TypeDocKind.Shipment
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

        // вызывай после успешного SaveChangesAsync в местах, где меняются документы/позиции
        public void InvalidateBalancesCache(int[]? resourceIds = null, int[]? unitIds = null)
        {
            _cache.Remove(CacheKey(null, null));
            _cache.Remove(CacheKey(resourceIds, null));
            _cache.Remove(CacheKey(null, unitIds));
            _cache.Remove(CacheKey(resourceIds, unitIds));
        }
    }
}
