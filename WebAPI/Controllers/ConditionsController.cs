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
    public class ConditionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConditionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Conditions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Condition>>> GetCondition()
        {
            return await _context.Condition.ToListAsync();
        }

        // GET: api/Conditions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Condition>> GetCondition(int id)
        {
            var condition = await _context.Condition.FindAsync(id);

            if (condition == null)
            {
                return NotFound();
            }

            return condition;
        }

        // PUT: api/Conditions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCondition(int id, Condition condition)
        {
            if (id != condition.Id)
            {
                return BadRequest();
            }

            _context.Entry(condition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConditionExists(id))
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

        // POST: api/Conditions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Condition>> PostCondition(Condition condition)
        {
            _context.Condition.Add(condition);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCondition", new { id = condition.Id }, condition);
        }

        // DELETE: api/Conditions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCondition(int id)
        {
            var condition = await _context.Condition.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }

            _context.Condition.Remove(condition);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConditionExists(int id)
        {
            return _context.Condition.Any(e => e.Id == id);
        }
    }
}
