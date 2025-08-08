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

    // GET: api/Documents
    // Возвращаем список с подгруженными справочниками,
    // чтобы на клиенте сразу видеть имена (TypeDoc/Client/Condition)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocument()
    {
        return await _context.Document
            .Include(d => d.TypeDoc)
            .Include(d => d.Client)
            .Include(d => d.Condition)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    // GET: api/Documents/5
    // Одна шапка с навигациями
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Document>> GetDocument(int id)
    {
        var document = await _context.Document
            .Include(d => d.TypeDoc)
            .Include(d => d.Client)
            .Include(d => d.Condition)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null) return NotFound();
        return document;
    }

    // GET: api/Documents/5/items
    // Позиции документа (каждая со своими справочниками)
    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<IEnumerable<Document_resource>>> GetItems(int id)
    {
        var exists = await _context.Document.AnyAsync(d => d.Id == id);
        if (!exists) return NotFound();

        var items = await _context.Document_resource
            .Where(r => EF.Property<int>(r, "DocumentId") == id) // shadow FK
            .Include(r => r.Resource)
            .Include(r => r.Unit)
            .ToListAsync();

        return items;
    }

    // PUT: api/Documents/5
    // Обновление только шапки (без позиций)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDocument(int id, Document document)
    {
        if (id != document.Id) return BadRequest();

        // Перепривяжем справочники по их Id (ожидается, что в body они пришли с заполненным Id)
        document.TypeDoc = await _context.TypeDoc.FindAsync(document.TypeDoc.Id) ?? document.TypeDoc;
        document.Client = await _context.Client.FindAsync(document.Client.Id) ?? document.Client;
        document.Condition = await _context.Condition.FindAsync(document.Condition.Id) ?? document.Condition;

        _context.Entry(document).State = EntityState.Modified;

        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!DocumentExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // POST: api/Documents
    // Создание только шапки (без позиций)
    [HttpPost]
    public async Task<ActionResult<Document>> PostDocument(Document document)
    {
        if (!await _context.TypeDoc.AnyAsync(x => x.Id == document.TypeDoc.Id))
            return BadRequest($"Invalid TypeDocId: {document.TypeDoc.Id}");
        if (!await _context.Client.AnyAsync(x => x.Id == document.Client.Id))
            return BadRequest($"Invalid ClientId: {document.Client.Id}");
        if (!await _context.Condition.AnyAsync(x => x.Id == document.Condition.Id))
            return BadRequest($"Invalid ConditionId: {document.Condition.Id}");

        // прикрепляем существующие записи, чтобы EF не пытался их создать
        _context.Attach(new TypeDoc { Id = document.TypeDoc.Id });
        _context.Attach(new Client { Id = document.Client.Id });
        _context.Attach(new Condition { Id = document.Condition.Id });

        // переназначаем навигации на прикреплённые сущности
        document.TypeDoc = await _context.TypeDoc.FindAsync(document.TypeDoc.Id)!;
        document.Client = await _context.Client.FindAsync(document.Client.Id)!;
        document.Condition = await _context.Condition.FindAsync(document.Condition.Id)!;

        _context.Document.Add(document);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
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

    private bool DocumentExists(int id) => _context.Document.Any(e => e.Id == id);

    // --------------------------
    // ТРАНЗАКЦИОННЫЕ эндпоинты
    // --------------------------

    // Транспортная модель только для запроса/ответа (БД-модель не меняем!)
    public class DocumentWithItems
    {
        public Document Document { get; set; } = default!;
        public List<Document_resource> Items { get; set; } = new();
    }

    // POST: api/Documents/with-items
    // Создание шапки и строк одним вызовом, в ОДНОЙ транзакции
    [HttpPost("with-items")]
    public async Task<ActionResult> CreateWithItems([FromBody] DocumentWithItems payload)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        // Привязываем справочники для шапки
        payload.Document.TypeDoc = await _context.TypeDoc.FindAsync(payload.Document.TypeDoc.Id) ?? payload.Document.TypeDoc;
        payload.Document.Client = await _context.Client.FindAsync(payload.Document.Client.Id) ?? payload.Document.Client;
        payload.Document.Condition = await _context.Condition.FindAsync(payload.Document.Condition.Id) ?? payload.Document.Condition;

        _context.Document.Add(payload.Document);
        await _context.SaveChangesAsync(); // нужен Id документа

        // Добавляем строки, связываем навигацией Document (shadow FK запишется сам)
        foreach (var row in payload.Items)
        {
            row.Document = payload.Document;
            row.Resource = await _context.Resource.FindAsync(row.Resource.Id) ?? row.Resource;
            row.Unit = await _context.Unit.FindAsync(row.Unit.Id) ?? row.Unit;
            _context.Document_resource.Add(row);
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = payload.Document.Id }, new { payload.Document.Id });
    }

    // PUT: api/Documents/with-items/5
    // Обновление шапки и синхронизация строк (add/update/delete) в ОДНОЙ транзакции
    [HttpPut("with-items/{id:int}")]
    public async Task<IActionResult> UpdateWithItems(int id, [FromBody] DocumentWithItems payload)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        var doc = await _context.Document
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc is null) return NotFound();

        // обновляем шапку (перепривязываем справочники)
        doc.Number = payload.Document.Number;
        doc.Date = payload.Document.Date;
        doc.TypeDoc = await _context.TypeDoc.FindAsync(payload.Document.TypeDoc.Id) ?? doc.TypeDoc;
        doc.Client = await _context.Client.FindAsync(payload.Document.Client.Id) ?? doc.Client;
        doc.Condition = await _context.Condition.FindAsync(payload.Document.Condition.Id) ?? doc.Condition;

        // Текущие строки документа
        var existing = await _context.Document_resource
            .Where(r => EF.Property<int>(r, "DocumentId") == id)
            .ToListAsync();

        // Индексируем входящие по Id
        var incomingById = payload.Items.Where(x => x.Id != 0)
            .ToDictionary(x => x.Id);

        // Удаляем отсутствующие
        foreach (var ex in existing)
            if (!incomingById.ContainsKey(ex.Id))
                _context.Document_resource.Remove(ex);

        // Добавляем/обновляем
        foreach (var row in payload.Items)
        {
            if (row.Id == 0)
            {
                // новая строка
                var newRow = new Document_resource
                {
                    DateTime = row.DateTime,
                    Count = row.Count
                };
                newRow.Document = doc;
                newRow.Resource = await _context.Resource.FindAsync(row.Resource.Id) ?? row.Resource;
                newRow.Unit = await _context.Unit.FindAsync(row.Unit.Id) ?? row.Unit;
                _context.Document_resource.Add(newRow);
            }
            else
            {
                var ex = existing.First(x => x.Id == row.Id);
                ex.DateTime = row.DateTime;
                ex.Count = row.Count;
                ex.Resource = await _context.Resource.FindAsync(row.Resource.Id) ?? ex.Resource;
                ex.Unit = await _context.Unit.FindAsync(row.Unit.Id) ?? ex.Unit;
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }
}
