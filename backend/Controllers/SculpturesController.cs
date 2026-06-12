using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptureMonitor.Models;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SculpturesController : ControllerBase
    {
        private readonly ISculptureService _service;

        public SculpturesController(ISculptureService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Sculpture>>> GetAll(int page = 1, int pageSize = 30)
        {
            var sculptures = await _service.GetAllAsync();
            return Ok(sculptures.Skip((page - 1) * pageSize).Take(pageSize));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Sculpture>> GetById(int id)
        {
            var sculpture = await _service.GetByIdAsync(id);
            if (sculpture == null) return NotFound();
            return Ok(sculpture);
        }

        [HttpGet("{id}/sensor-data")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensorData(int id, DateTime? startTime, DateTime? endTime, string sensorType = null, int days = 7)
        {
            var data = await _service.GetTimeSeriesDataAsync(id, days);
            return Ok(data);
        }

        [HttpGet("{id}/heatmap")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetHeatmap(int id)
        {
            var data = await _service.GetHeatmapDataAsync(id);
            return Ok(data);
        }

        [HttpGet("dashboard/stats")]
        [Authorize]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            var stats = await _service.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("status/{status}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Sculpture>>> GetByStatus(string status)
        {
            var sculptures = await _service.GetByStatusAsync(status);
            return Ok(sculptures);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Sculpture>> Create(Sculpture sculpture)
        {
            var created = await _service.CreateAsync(sculpture);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, Sculpture sculpture)
        {
            if (id != sculpture.Id) return BadRequest();
            var updated = await _service.UpdateAsync(sculpture);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
