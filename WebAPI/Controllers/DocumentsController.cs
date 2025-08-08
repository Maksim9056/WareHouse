using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.Date;
using ClassLibrary.Models;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;
    public DocumentsController(AppDbContext context) => _context = context;

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
        return NoContent();
    }
}
