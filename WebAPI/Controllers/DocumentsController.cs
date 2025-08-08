using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding; // ← добавь
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

    // ----------------- HELPERS -----------------

    // Убираем авто-ошибки по вложенным навигациям (кроме *.Id), чтобы успеть догрузить сущности
    private static void ClearNavErrors(ModelStateDictionary ms)
    {
        string[] prefixes = { "TypeDoc.", "Client.", "Condition." };
        foreach (var p in prefixes)
            foreach (var k in ms.Keys.Where(k => k.StartsWith(p) && !k.EndsWith(".Id")).ToList())
                ms.Remove(k);
    }

    // Догружаем навигации по Id → подставляем «полные» сущности (с Name и т.п.)
    private async Task<bool> HydrateAsync(Document d)
    {
        if (d.TypeDoc?.Id <= 0 || d.Client?.Id <= 0 || d.Condition?.Id <= 0) return false;

        var td = await _context.TypeDoc.FindAsync(d.TypeDoc.Id);
        var cl = await _context.Client.FindAsync(d.Client.Id);
        var cn = await _context.Condition.FindAsync(d.Condition.Id);

        if (td is null || cl is null || cn is null) return false;

        d.TypeDoc = td;
        d.Client = cl;
        d.Condition = cn;
        return true;
    }

    private (bool ok, IActionResult? err) ValidateDocument(Document d)
    {
        if (string.IsNullOrWhiteSpace(d.Number))
            return (false, ValidationProblem((ValidationProblemDetails)new ValidationProblemDetails { Title = "Номер обязателен" }));

        if (d.TypeDoc?.Id <= 0)
            return (false, ValidationProblem((ValidationProblemDetails)new ValidationProblemDetails { Title = "Не выбран тип документа" }));

        if (d.Client?.Id <= 0)
            return (false, ValidationProblem((ValidationProblemDetails)new ValidationProblemDetails { Title = "Не выбран клиент" }));

        if (d.Condition?.Id <= 0)
            return (false, ValidationProblem((ValidationProblemDetails)new ValidationProblemDetails { Title = "Не выбрано состояние" }));

        return (true, null);
    }

    // ----------------- READ -----------------

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

    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<IEnumerable<Document_resource>>> GetItems(int id)
    {
        var exists = await _context.Document.AnyAsync(d => d.Id == id);
        if (!exists) return NotFound();

        var items = await _context.Document_resource
            .Where(r => EF.Property<int>(r, "DocumentId") == id)
            .Include(r => r.Resource)
            .Include(r => r.Unit)
            .ToListAsync();

        return items;
    }

    // ----------------- WRITE -----------------

    // POST: api/Documents  (создание шапки)
    [HttpPost]
    public async Task<IActionResult> PostDocument([FromBody] Document doc)
    {
        if (doc is null)
            return BadRequest();

        // Подгружаем справочники по Id
        if (doc.Client?.Id > 0)
            doc.Client = await _context.Client.FindAsync(doc.Client.Id);

        if (doc.TypeDoc?.Id > 0)
            doc.TypeDoc = await _context.TypeDoc.FindAsync(doc.TypeDoc.Id);

        if (doc.Condition?.Id > 0)
            doc.Condition = await _context.Condition.FindAsync(doc.Condition.Id);

        // Теперь валидация будет работать с полными объектами
        ModelState.Clear();
        if (!TryValidateModel(doc))
            return ValidationProblem(ModelState);

        _context.Document.Add(doc);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDocument), new { id = doc.Id }, doc);
    }

    // PUT: api/Documents/{id}  (обновление шапки)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDocument(int id, [FromBody] Document incoming)
    {
        if (id != incoming.Id) return BadRequest();

        ClearNavErrors(ModelState);

        var doc = await _context.Document.FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null) return NotFound();

        // простые поля
        doc.Number = incoming.Number;
        doc.Date = incoming.Date;

        // проверка выбранных Id
        var v = ValidateDocument(incoming);
        if (!v.ok) return v.err;

        // гидрируем навигации по Id из incoming
        var td = await _context.TypeDoc.FindAsync(incoming.TypeDoc.Id);
        var cl = await _context.Client.FindAsync(incoming.Client.Id);
        var cn = await _context.Condition.FindAsync(incoming.Condition.Id);
        if (td is null || cl is null || cn is null)
            return ValidationProblem(new ValidationProblemDetails { Title = "Справочник не найден" });

        doc.TypeDoc = td;
        doc.Client = cl;
        doc.Condition = cn;

        // пере-валидация уже обновлённого doc (с полными навигациями)
        ModelState.Clear();
        if (!TryValidateModel(doc)) return ValidationProblem(ModelState);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE без изменений
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.Document.FindAsync(id);
        if (document == null) return NotFound();

        _context.Document.Remove(document);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ------------- транзакционные with-items (как у тебя), но уже гидрируют -------------
    public class DocumentWithItems
    {
        public Document Document { get; set; } = default!;
        public List<Document_resource> Items { get; set; } = new();
    }

    [HttpPost("with-items")]
    public async Task<ActionResult> CreateWithItems([FromBody] DocumentWithItems payload)
    {
        ClearNavErrors(ModelState);

        var v = ValidateDocument(payload.Document);
        //if (!v.ok) return v.err;

        if (!await HydrateAsync(payload.Document))
            return ValidationProblem(new ValidationProblemDetails { Title = "Неверные Id справочников" });

        // повторная валидация полной шапки
        ModelState.Clear();
        if (!TryValidateModel(payload.Document)) return ValidationProblem(ModelState);

        await using var tx = await _context.Database.BeginTransactionAsync();

        _context.Document.Add(payload.Document);
        await _context.SaveChangesAsync(); // нужен Id

        foreach (var row in payload.Items)
        {
            // гидра ресурсов/единиц
            var res = await _context.Resource.FindAsync(row.Resource.Id);
            var uni = await _context.Unit.FindAsync(row.Unit.Id);
            if (res is null || uni is null)
                return ValidationProblem(new ValidationProblemDetails { Title = "Ресурс/Единица не найдены" });

            row.Document = payload.Document; // полная шапка
            row.Resource = res;
            row.Unit = uni;

            // валидируем строку
            ModelState.Clear();
            if (!TryValidateModel(row)) return ValidationProblem(ModelState);

            _context.Document_resource.Add(row);
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = payload.Document.Id }, new { payload.Document.Id });
    }

    [HttpPut("with-items/{id:int}")]
    public async Task<IActionResult> UpdateWithItems(int id, [FromBody] DocumentWithItems payload)
    {
        ClearNavErrors(ModelState);

        var doc = await _context.Document.FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null) return NotFound();

        // обновляем шапку
        doc.Number = payload.Document.Number;
        doc.Date = payload.Document.Date;

        var td = await _context.TypeDoc.FindAsync(payload.Document.TypeDoc.Id);
        var cl = await _context.Client.FindAsync(payload.Document.Client.Id);
        var cn = await _context.Condition.FindAsync(payload.Document.Condition.Id);
        if (td is null || cl is null || cn is null)
            return ValidationProblem(new ValidationProblemDetails { Title = "Справочник не найден" });

        doc.TypeDoc = td; doc.Client = cl; doc.Condition = cn;

        // валидируем шапку уже «полную»
        ModelState.Clear();
        if (!TryValidateModel(doc)) return ValidationProblem(ModelState);

        await using var tx = await _context.Database.BeginTransactionAsync();

        var existing = await _context.Document_resource
            .Where(r => EF.Property<int>(r, "DocumentId") == id)
            .ToListAsync();

        var incomingById = payload.Items.Where(x => x.Id != 0).ToDictionary(x => x.Id);

        // удалить отсутствующие
        foreach (var ex in existing)
            if (!incomingById.ContainsKey(ex.Id))
                _context.Document_resource.Remove(ex);

        // upsert
        foreach (var row in payload.Items)
        {
            var res = await _context.Resource.FindAsync(row.Resource.Id);
            var uni = await _context.Unit.FindAsync(row.Unit.Id);
            if (res is null || uni is null)
                return ValidationProblem(new ValidationProblemDetails { Title = "Ресурс/Единица не найдены" });

            if (row.Id == 0)
            {
                var newRow = new Document_resource
                {
                    DateTime = row.DateTime,
                    Count = row.Count,
                    Document = doc,
                    Resource = res,
                    Unit = uni
                };

                ModelState.Clear();
                if (!TryValidateModel(newRow)) return ValidationProblem(ModelState);

                _context.Document_resource.Add(newRow);
            }
            else
            {
                var ex = existing.First(x => x.Id == row.Id);
                ex.DateTime = row.DateTime;
                ex.Count = row.Count;
                ex.Document = doc;
                ex.Resource = res;
                ex.Unit = uni;

                ModelState.Clear();
                if (!TryValidateModel(ex)) return ValidationProblem(ModelState);
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }
}
