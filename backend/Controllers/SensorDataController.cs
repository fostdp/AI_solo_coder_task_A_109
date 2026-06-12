using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptureMonitor.Models;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController : ControllerBase
    {
        private readonly ISensorDataService _service;

        public SensorDataController(ISensorDataService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetAll(int limit = 100)
        {
            var data = await _service.GetAllAsync();
            return Ok(data.Take(limit));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<SensorData>> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("sensor/{sensorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetBySensorId(int sensorId, int limit = 100)
        {
            var data = await _service.GetBySensorIdAsync(sensorId, limit);
            return Ok(data);
        }

        [HttpGet("sculpture/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetBySculptureId(int sculptureId, int limit = 100)
        {
            var data = await _service.GetBySculptureIdAsync(sculptureId, limit);
            return Ok(data);
        }

        [HttpGet("timerange")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetByTimeRange(DateTime startTime, DateTime endTime)
        {
            var data = await _service.GetByTimeRangeAsync(startTime, endTime);
            return Ok(data);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<SensorData>> Create([FromBody] SensorData sensorData)
        {
            try
            {
                var created = await _service.CreateAsync(sensorData);
                return Ok(new { success = true, id = created.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("batch")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SensorData>>> CreateBatch([FromBody] IEnumerable<SensorData> sensorDataList)
        {
            try
            {
                var created = await _service.CreateBatchAsync(sensorDataList);
                return Ok(new { success = true, count = created.Count() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("status/summary")]
        [Authorize]
        public async Task<ActionResult<object>> GetStatusSummary()
        {
            var summary = await _service.GetSensorStatusSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("{sensorId}/logs")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensorLogs(string sensorId, int days = 7)
        {
            var logs = await _service.GetSensorLogsAsync(sensorId, days);
            return Ok(logs);
        }
    }
}
