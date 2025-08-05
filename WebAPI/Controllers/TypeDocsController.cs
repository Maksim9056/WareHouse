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
    public class TypeDocsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TypeDocsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeDocs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeDoc>>> GetTypeDoc()
        {
            return await _context.TypeDoc.ToListAsync();
        }

        // GET: api/TypeDocs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeDoc>> GetTypeDoc(int id)
        {
            var typeDoc = await _context.TypeDoc.FindAsync(id);

            if (typeDoc == null)
            {
                return NotFound();
            }

            return typeDoc;
        }

        // PUT: api/TypeDocs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeDoc(int id, TypeDoc typeDoc)
        {
            if (id != typeDoc.Id)
            {
                return BadRequest();
            }

            _context.Entry(typeDoc).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeDocExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TypeDocs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TypeDoc>> PostTypeDoc(TypeDoc typeDoc)
        {
            _context.TypeDoc.Add(typeDoc);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeDoc", new { id = typeDoc.Id }, typeDoc);
        }

        // DELETE: api/TypeDocs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeDoc(int id)
        {
            var typeDoc = await _context.TypeDoc.FindAsync(id);
            if (typeDoc == null)
            {
                return NotFound();
            }

            _context.TypeDoc.Remove(typeDoc);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TypeDocExists(int id)
        {
            return _context.TypeDoc.Any(e => e.Id == id);
        }
    }
}
