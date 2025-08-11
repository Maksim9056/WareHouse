using ClassLibrary.Date;
using ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using WebAPI.Service;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BalanceReportService _balances;

    public IMemoryCache cache;

    public DocumentsController(AppDbContext context, IMemoryCache memoryCache, BalanceReportService balances)
    {
        //_context = context;
        cache = memoryCache;
        cache.Set("_context", context);


        cache.TryGetValue("_context", out AppDbContext? appDbContext);

        _context = appDbContext;
        _balances = balances;

    }




    // ----------------- helpers -----------------
    private async Task ValidateDocumentFksAsync(Document d)
    {
        if (!await _context.TypeDoc.AnyAsync(x => x.Id == d.TypeDocId))
            ModelState.AddModelError(nameof(d.TypeDocId), "Тип документа не найден.");

        if (!await _context.Client.AnyAsync(x => x.Id == d.ClientId))
            ModelState.AddModelError(nameof(d.ClientId), "Клиент не найден.");

        if (!await _context.Condition.AnyAsync(x => x.Id == d.ConditionId))
            ModelState.AddModelError(nameof(d.ConditionId), "Состояние не найдено.");
    }

    private async Task ValidateRowFksAsync(Document_resource r)
    {
        if (!await _context.Resource.AnyAsync(x => x.Id == r.ResourceId))
            ModelState.AddModelError(nameof(r.ResourceId), $"Ресурс {r.ResourceId} не найден.");

        if (!await _context.Unit.AnyAsync(x => x.Id == r.UnitId))
            ModelState.AddModelError(nameof(r.UnitId), $"Ед. изм. {r.UnitId} не найдена.");

        if (r.Count < 1)
            ModelState.AddModelError(nameof(r.Count), "Количество должно быть >= 1.");
    }

    // ----------------- READ -----------------

    // GET: api/Documents
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
    {
        var list = await _context.Document
            .Include(d => d.TypeDoc)
            .Include(d => d.Client)
            .Include(d => d.Condition)
            .OrderByDescending(d => d.Date)
            .ToListAsync();

        return list;
    }

    // GET: api/Documents/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Document>> GetDocument(int id)
    {
        var doc = await _context.Document
            .Include(d => d.TypeDoc)
            .Include(d => d.Client)
            .Include(d => d.Condition)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null) return NotFound();
        return doc;
    }

    // GET: api/Documents/5/items
    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<IEnumerable<Document_resource>>> GetItems(int id)
    {
        var exists = await _context.Document.AnyAsync(d => d.Id == id);
        if (!exists) return NotFound();

        var items = await _context.Document_resource
            .Where(r => r.DocumentId == id)
            .Include(r => r.Resource)
            .Include(r => r.Unit)
            .ToListAsync();

        return items;
    }

    // ----------------- WRITE -----------------

    // POST: api/Documents  -> returns just new Id (int)
    [HttpPost]
    public async Task<ActionResult<int>> PostDocument([FromBody] Document document)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await ValidateDocumentFksAsync(document);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        _context.Document.Add(document);
        await _context.SaveChangesAsync();
        _balances.InvalidateBalancesCache(); // сброс сразу после записи

        return Ok(document.Id);
    }

    // PUT: api/Documents/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDocument(int id, [FromBody] Document incoming)
    {
        if (id != incoming.Id) return BadRequest("Id в маршруте и теле не совпадают.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var doc = await _context.Document.FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null) return NotFound();

        await ValidateDocumentFksAsync(incoming);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // обновляем скаляры и FK
        doc.Number = incoming.Number;
        doc.Date = incoming.Date;
        doc.TypeDocId = incoming.TypeDocId;
        doc.ClientId = incoming.ClientId;
        doc.ConditionId = incoming.ConditionId;

        await _context.SaveChangesAsync();
        _balances.InvalidateBalancesCache(); // сброс сразу после записи

        return NoContent();
    }

    // DELETE: api/Documents/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.Document.FindAsync(id);
        if (document == null) return NotFound();

        _context.Document.Remove(document);
        await _context.SaveChangesAsync();
        _balances.InvalidateBalancesCache(); 
        return NoContent();
    }

    // ----------------- with-items (Document + строки) -----------------

    public class DocumentWithItems
    {
        public Document Document { get; set; } = default!;
        public List<Document_resource> Items { get; set; } = new();
    }

    // POST: api/Documents/with-items  -> создать шапку и строки в одной транзакции
    [HttpPost("with-items")]
    public async Task<ActionResult<int>> CreateWithItems([FromBody] DocumentWithItems payload)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await ValidateDocumentFksAsync(payload.Document);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        foreach (var r in payload.Items)
        {
            // FK на документ будет выставлен после сохранения шапки
            if (r.DocumentId != 0)
                ModelState.AddModelError(nameof(r.DocumentId), "DocumentId устанавливается на сервере.");

            await ValidateRowFksAsync(r);
        }

        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await using var tx = await _context.Database.BeginTransactionAsync();

        _context.Document.Add(payload.Document);
        await _context.SaveChangesAsync(); // появился Document.Id

        int docId = payload.Document.Id;

        foreach (var r in payload.Items)
        {
            r.DocumentId = docId;
            _context.Document_resource.Add(r);
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        _balances.InvalidateBalancesCache(); // сброс сразу после записи

        return Ok(docId);
    }

    // PUT: api/Documents/with-items/5  -> обновить шапку и синхронизировать строки
    [HttpPut("with-items/{id:int}")]
    public async Task<IActionResult> UpdateWithItems(int id, [FromBody] DocumentWithItems payload)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var doc = await _context.Document.FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null) return NotFound();

        // валидируем шапку
        payload.Document.Id = id; // на всякий случай
        await ValidateDocumentFksAsync(payload.Document);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // валидируем строки (FK на документ проверять не нужно — он = id)
        foreach (var r in payload.Items)
        {
            if (r.Id < 0) ModelState.AddModelError(nameof(r.Id), "Некорректный Id строки.");
            await ValidateRowFksAsync(r);
        }
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await using var tx = await _context.Database.BeginTransactionAsync();

        // обновляем шапку
        doc.Number = payload.Document.Number;
        doc.Date = payload.Document.Date;
        doc.TypeDocId = payload.Document.TypeDocId;
        doc.ClientId = payload.Document.ClientId;
        doc.ConditionId = payload.Document.ConditionId;

        // текущие строки
        var existing = await _context.Document_resource
            .Where(r => r.DocumentId == id)
            .ToListAsync();

        var incomingById = payload.Items.Where(x => x.Id != 0)
            .ToDictionary(x => x.Id);

        // удаляем отсутствующие в payload
        var toDelete = existing.Where(x => !incomingById.ContainsKey(x.Id)).ToList();
        if (toDelete.Count > 0) _context.Document_resource.RemoveRange(toDelete);

        // upsert для присланных строк
        foreach (var r in payload.Items)
        {
            if (r.Id == 0)
            {
                var newRow = new Document_resource
                {
                    DocumentId = id,
                    ResourceId = r.ResourceId,
                    UnitId = r.UnitId,
                    DateTime = r.DateTime,
                    Count = r.Count
                };
                _context.Document_resource.Add(newRow);
            }
            else
            {
                var ex = existing.FirstOrDefault(x => x.Id == r.Id);
                if (ex == null)
                {
                    // строка с таким Id не принадлежит документу
                    ModelState.AddModelError(nameof(r.Id), $"Строка {r.Id} не принадлежит документу {id}.");
                    return ValidationProblem(ModelState);
                }

                ex.ResourceId = r.ResourceId;
                ex.UnitId = r.UnitId;
                ex.DateTime = r.DateTime;
                ex.Count = r.Count;
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        _balances.InvalidateBalancesCache(); // сброс сразу после записи

        return NoContent();
    }
    public class IdName { public int Id { get; set; } public string Name { get; set; } = ""; }
    public class FilterOptions
    {
        public List<string> Numbers { get; set; } = new();
        public List<IdName> Resources { get; set; } = new();
        public List<IdName> Units { get; set; } = new();
    }

    [HttpGet("filter-options")]
    public async Task<ActionResult<FilterOptions>> GetFilterOptions([FromQuery] int typeDocId)
    {
        if (typeDocId <= 0) return BadRequest("typeDocId required");

        var numbers = await _context.Document
            .Where(d => d.TypeDocId == typeDocId)
            .Select(d => d.Number)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();

        var docIds = _context.Document
            .Where(d => d.TypeDocId == typeDocId)
            .Select(d => d.Id);

        var resourceIds = await _context.Document_resource
            .Where(r => docIds.Contains(r.DocumentId))
            .Select(r => r.ResourceId)
            .Distinct()
            .ToListAsync();

        var unitIds = await _context.Document_resource
            .Where(r => docIds.Contains(r.DocumentId))
            .Select(r => r.UnitId)
            .Distinct()
            .ToListAsync();

        var resources = await _context.Resource
            .Where(x => resourceIds.Contains(x.Id))
            .Select(x => new IdName { Id = x.Id, Name = x.Name })
            .OrderBy(x => x.Name)
            .ToListAsync();

        var units = await _context.Unit
            .Where(x => unitIds.Contains(x.Id))
            .Select(x => new IdName { Id = x.Id, Name = x.Name })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return new FilterOptions { Numbers = numbers, Resources = resources, Units = units };
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Document>>> Search(
    [FromQuery] int typeDocId,
    [FromQuery] DateOnly? dateFrom,
    [FromQuery] DateOnly? dateTo,
    [FromQuery] string? numbers,
    [FromQuery] string? resourceIds,
    [FromQuery] string? unitIds)
    {
        if (typeDocId <= 0) return BadRequest("typeDocId required");

        var numberSet = (numbers ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        static List<int> ParseIntCsv(string? s) => (s ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var v) ? v : 0)
            .Where(v => v > 0).Distinct().ToList();

        var resIds = ParseIntCsv(resourceIds);
        var uniIds = ParseIntCsv(unitIds);

        var q = _context.Document
            .Where(d => d.TypeDocId == typeDocId);

        if (dateFrom is not null) q = q.Where(d => d.Date >= dateFrom);
        if (dateTo is not null) q = q.Where(d => d.Date <= dateTo);
        if (numberSet.Count > 0) q = q.Where(d => numberSet.Contains(d.Number));

        // Если заданы ресурсы/ед.изм — документ должен иметь хотя бы одну строку, попадающую под фильтр
        if (resIds.Count > 0 || uniIds.Count > 0)
        {
            q = q.Where(d => _context.Document_resource.Any(r =>
                r.DocumentId == d.Id &&
                (resIds.Count == 0 || resIds.Contains(r.ResourceId)) &&
                (uniIds.Count == 0 || uniIds.Contains(r.UnitId))));
        }

        var list = await q
            .Include(d => d.TypeDoc)
            .Include(d => d.Client)
            .Include(d => d.Condition)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.Id)
            .ToListAsync();

        return list;
    }
}
