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
    public class BalancesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BalancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Balances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Balance>>> GetBalance()
        {
            return await _context.Balance.ToListAsync();
        }

        // GET: api/Balances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Balance>> GetBalance(int id)
        {
            var balance = await _context.Balance.FindAsync(id);

            if (balance == null)
            {
                return NotFound();
            }

            return balance;
        }

        // PUT: api/Balances/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBalance(int id, Balance balance)
        {
            if (id != balance.Id)
            {
                return BadRequest();
            }

            _context.Entry(balance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BalanceExists(id))
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

        // POST: api/Balances
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Balance>> PostBalance(Balance balance)
        {
            _context.Balance.Add(balance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBalance", new { id = balance.Id }, balance);
        }

        // DELETE: api/Balances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBalance(int id)
        {
            var balance = await _context.Balance.FindAsync(id);
            if (balance == null)
            {
                return NotFound();
            }

            _context.Balance.Remove(balance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BalanceExists(int id)
        {
            return _context.Balance.Any(e => e.Id == id);
        }
    }
}
