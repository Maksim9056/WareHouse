using ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Service;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly BalanceReportService _svc;
        public ReportsController(BalanceReportService svc) => _svc = svc;

        // GET /api/reports/balances?resourceIds=1&resourceIds=5&unitIds=2
        [HttpGet("removecache")]
        public async Task<ActionResult> Remove_Cache([FromQuery] int[]? resourceIds, [FromQuery] int[]? unitIds, CancellationToken ct)
        {
            _svc.InvalidateBalancesCache(resourceIds, unitIds);
            return Ok();
        }

        // GET /api/reports/balances?resourceIds=1&resourceIds=5&unitIds=2
        [HttpGet("balances")]
        public async Task<ActionResult<List<BalanceRowDto>>> Get(
            [FromQuery] int[]? resourceIds,
            [FromQuery] int[]? unitIds,
            CancellationToken ct)
        {
            var list = await _svc.GetBalancesAsync(resourceIds, unitIds, ct);
            return Ok(list);
        }
    }
}
