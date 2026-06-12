using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _service;
        private readonly AppDbContext _context;

        public AlertsController(IAlertService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts(
            int? sculptureId = null,
            string severity = null,
            DateTime? startDate = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Alerts.AsQueryable();

            if (sculptureId.HasValue)
                query = query.Where(a => a.SculptureId == sculptureId.Value);
            
            if (!string.IsNullOrEmpty(severity))
                query = query.Where(a => a.Severity == severity);
            
            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            query = query.OrderByDescending(a => a.CreatedAt);
            
            var alerts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await query.CountAsync();

            return Ok(new { alerts, total, page, pageSize });
        }

        [HttpGet("pending")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Alert>>> GetPendingAlerts(int limit = 10)
        {
            var alerts = await _context.Alerts
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
            return Ok(alerts);
        }

        [HttpPost("{id}/acknowledge")]
        [Authorize]
        public async Task<IActionResult> AcknowledgeAlert(string id)
        {
            var result = await _service.AcknowledgeAlertAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [HttpGet("config")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<AlertThreshold>>> GetAlertConfig()
        {
            var configs = await _context.AlertThresholds.ToListAsync();
            return Ok(configs);
        }

        [HttpPut("config")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateAlertConfig([FromBody] IEnumerable<AlertThreshold> configs)
        {
            foreach (var config in configs)
            {
                var existing = await _context.AlertThresholds.FindAsync(config.Id);
                if (existing != null)
                {
                    existing.WarningThreshold = config.WarningThreshold;
                    existing.CriticalThreshold = config.CriticalThreshold;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("test-dingtalk")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestDingTalkNotification()
        {
            var testAlert = new Alert
            {
                Id = Guid.NewGuid().ToString(),
                SculptureId = 1,
                Title = "测试告警",
                Message = "这是一条测试告警消息，用于验证钉钉推送功能是否正常工作。",
                Severity = "Medium",
                AlertType = "Test",
                ActualValue = 45.5,
                ThresholdValue = 30.0,
                CreatedAt = DateTime.Now
            };
            
            var result = await _service.SendDingTalkNotificationAsync(testAlert);
            return Ok(new { success = result });
        }
    }
}
