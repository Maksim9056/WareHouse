using ClassLibrary.Date;
using ClassLibrary.Models;
using Elfie.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Unit_of_measurement
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnit_of_measurement()
        {
            return await _context.Unit.Include(u =>u.condition).ToListAsync();
        }

        // GET: api/Unit_of_measurement/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetUnit_of_measurement(int id)
        {
            var unit_of_measurement = await _context.Unit.Include(u => u.condition).FirstOrDefaultAsync(u => u.Id == id);

            if (unit_of_measurement == null)
            {
                return NotFound();
            }

            return unit_of_measurement;
        }

        // PUT: api/Unit_of_measurement/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnit_of_measurement(int id, Unit unit_of_measurement)
        {
            if (id != unit_of_measurement.Id)
            {
                return BadRequest();
            }
            
            if (await _context.Unit.AnyAsync(r => r.Id != unit_of_measurement.Id && r.Name == unit_of_measurement.Name))
            {
                return Conflict("Единица измерения  с таким наименованием уже существует");
            }
            unit_of_measurement.condition = await _context.Condition.FirstOrDefaultAsync(u => u.Id == unit_of_measurement.condition.Id);

            _context.Entry(unit_of_measurement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Unit_of_measurementExists(id))
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

        // POST: api/Unit_of_measurement
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Unit>> PostUnit_of_measurement(Unit unit_of_measurement)
        {
            if (await _context.Unit.AnyAsync(r => r.Id != unit_of_measurement.Id && r.Name == unit_of_measurement.Name))
            {
                return Conflict("Единица измерения  с таким наименованием уже существует");
            }
            unit_of_measurement.condition = await _context.Condition.FirstOrDefaultAsync(u => u.Id == unit_of_measurement.condition.Id);

            _context.Unit.Add(unit_of_measurement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnit_of_measurement", new { id = unit_of_measurement.Id }, unit_of_measurement);
        }

        // DELETE: api/Unit_of_measurement/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit_of_measurement(int id)
        {
            var unit_of_measurement = await _context.Unit.FindAsync(id);
            if (unit_of_measurement == null)
            {
                return NotFound();
            }

            _context.Unit.Remove(unit_of_measurement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Unit_of_measurementExists(int id)
        {
            return _context.Unit.Any(e => e.Id == id);
        }
    }
}
