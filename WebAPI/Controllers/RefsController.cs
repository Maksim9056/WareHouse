using ClassLibrary.Date;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
    public sealed class IdNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [ApiController]
    [Route("api/refs")]
    public class RefsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RefsController(AppDbContext db) => _db = db;

        [HttpGet("resources")]
        public async Task<ActionResult<List<IdNameDto>>> Resources(CancellationToken ct)
            => Ok(await _db.Resource.AsNoTracking()
                   .OrderBy(x => x.Name)
                   .Select(x => new IdNameDto { Id = x.Id, Name = x.Name })
                   .ToListAsync(ct));

        [HttpGet("units")]
        public async Task<ActionResult<List<IdNameDto>>> Units(CancellationToken ct)
            => Ok(await _db.Unit.AsNoTracking()
                   .OrderBy(x => x.Name)
                   .Select(x => new IdNameDto { Id = x.Id, Name = x.Name })
                   .ToListAsync(ct));
    }
}
