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

        public Document_resourceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Document_resource
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document_resource>>> GetDocument_resource()
        {
            return await _context.Document_resource.ToListAsync();
        }

        // GET: api/Document_resource/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document_resource>> GetDocument_resource(int id)
        {
            var document_resource = await _context.Document_resource.FindAsync(id);

            if (document_resource == null)
            {
                return NotFound();
            }

            return document_resource;
        }

        // PUT: api/Document_resource/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument_resource(int id, Document_resource document_resource)
        {
            if (id != document_resource.Id)
            {
                return BadRequest();
            }

            _context.Entry(document_resource).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Document_resourceExists(id))
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

        // POST: api/Document_resource
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Document_resource>> PostDocument_resource(Document_resource document_resource)
        {
            _context.Document_resource.Add(document_resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocument_resource", new { id = document_resource.Id }, document_resource);
        }

        // DELETE: api/Document_resource/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument_resource(int id)
        {
            var document_resource = await _context.Document_resource.FindAsync(id);
            if (document_resource == null)
            {
                return NotFound();
            }

            _context.Document_resource.Remove(document_resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Document_resourceExists(int id)
        {
            return _context.Document_resource.Any(e => e.Id == id);
        }
    }
}
