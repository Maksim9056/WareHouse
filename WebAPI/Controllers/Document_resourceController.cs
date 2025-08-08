using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.Date;
using ClassLibrary.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Document_resourceController : ControllerBase
    {
        private readonly AppDbContext _context;
        public Document_resourceController(AppDbContext context) => _context = context;

        // ------- базовые методы оставим, но с Attach -------

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document_resource>>> GetDocument_resource()
            => await _context.Document_resource.ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Document_resource>> GetDocument_resource(int id)
        {
            var r = await _context.Document_resource.FindAsync(id);
            return r is null ? NotFound() : r;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutDocument_resource(int id, Document_resource row)
        {
            if (id != row.Id) return BadRequest();

            // привязываем навигации по Id (только ссылки)
            if (row.Document?.Id > 0)
                _context.Attach(row.Document).State = EntityState.Unchanged;
            if (row.Resource?.Id > 0)
                _context.Attach(row.Resource).State = EntityState.Unchanged;
            if (row.Unit?.Id > 0)
                _context.Attach(row.Unit).State = EntityState.Unchanged;

            _context.Entry(row).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Document_resource.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDocument_resource(int id, [FromBody] Document_resource row)
        {
            if (row is null) return BadRequest();
            if (id != row.Id) return BadRequest();

            var doc = (row.Document != null && row.Document.Id > 0)
                ? await _context.Document.FindAsync(row.Document.Id)
                : null;
            var res = (row.Resource != null && row.Resource.Id > 0)
                ? await _context.Resource.FindAsync(row.Resource.Id)
                : null;
            var uni = (row.Unit != null && row.Unit.Id > 0)
                ? await _context.Unit.FindAsync(row.Unit.Id)
                : null;

            if (doc is null || res is null || uni is null)
                return ValidationProblem((ValidationProblemDetails)new ValidationProblemDetails
                {
                    Title = "Документ/Ресурс/Единица не найдены"
                });

            row.Document = doc;
            row.Resource = res;
            row.Unit = uni;

            ModelState.Clear();
            if (!TryValidateModel(row))
                return ValidationProblem(ModelState);

            _context.Entry(row).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<Document_resource>> PostDocument_resource(Document_resource row)
        {
            // гидрируем навигации
            var doc = row.Document?.Id > 0 ? await _context.Document.FindAsync(row.Document.Id) : null;
            var res = row.Resource?.Id > 0 ? await _context.Resource.FindAsync(row.Resource.Id) : null;
            var uni = row.Unit?.Id > 0 ? await _context.Unit.FindAsync(row.Unit.Id) : null;
            if (doc is null || res is null || uni is null)
                return ValidationProblem(new ValidationProblemDetails { Title = "Документ/Ресурс/Единица не найдены" });

            row.Document = doc; row.Resource = res; row.Unit = uni;

            ModelState.Clear();
            if (!TryValidateModel(row)) return ValidationProblem(ModelState);

            _context.Document_resource.Add(row);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocument_resource), new { id = row.Id }, row);
        }
    }
}
