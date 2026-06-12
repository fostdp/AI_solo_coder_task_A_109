using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DingTalkController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DingTalkController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("config")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<DingTalkConfig>> GetConfig()
        {
            var config = await _context.DingTalkConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound();
            return Ok(new
            {
                config.Id,
                config.WebhookUrl,
                config.Secret,
                AtMobiles = config.AtMobiles?.Split(',').ToList() ?? new List<string>(),
                config.IsAtAll,
                config.Enabled
            });
        }

        [HttpPut("config")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateConfig([FromBody] DingTalkConfigDto dto)
        {
            var config = await _context.DingTalkConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new DingTalkConfig();
                _context.DingTalkConfigs.Add(config);
            }

            config.WebhookUrl = dto.WebhookUrl;
            config.Secret = dto.Secret;
            config.AtMobiles = string.Join(",", dto.AtMobiles ?? new List<string>());
            config.IsAtAll = dto.IsAtAll;
            config.Enabled = dto.Enabled;
            config.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("test")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestNotification([FromBody] TestNotificationRequest request)
        {
            var config = await _context.DingTalkConfigs.FirstOrDefaultAsync();
            if (config == null || !config.Enabled.GetValueOrDefault() || string.IsNullOrEmpty(config.WebhookUrl))
                return BadRequest(new { success = false, message = "钉钉通知未配置或未启用" });

            var content = !string.IsNullOrEmpty(request.Content) ? request.Content : "这是一条测试通知，用于验证钉钉推送功能是否正常工作。";
            var title = !string.IsNullOrEmpty(request.Title) ? request.Title : "【系统测试】";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sign = GenerateSignature(config.Secret, timestamp);
            var fullUrl = BuildWebhookUrl(config.WebhookUrl, config.Secret, timestamp, sign);

            using var httpClient = new HttpClient();
            var payload = new
            {
                msgtype = "markdown",
                markdown = new
                {
                    title = title,
                    text = $"{title}\n\n{content}\n\n发送时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                },
                at = new
                {
                    atMobiles = config.AtMobiles?.Split(',').Where(m => !string.IsNullOrEmpty(m)).ToList() ?? new List<string>(),
                    isAtAll = config.IsAtAll.GetValueOrDefault()
                }
            };

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(fullUrl, httpContent);
                var result = await response.Content.ReadAsStringAsync();
                return Ok(new { success = response.IsSuccessStatusCode, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private string GenerateSignature(string secret, long timestamp)
        {
            if (string.IsNullOrEmpty(secret)) return string.Empty;

            var stringToSign = $"{timestamp}\n{secret}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hash);
        }

        private string BuildWebhookUrl(string baseUrl, string secret, long timestamp, string sign)
        {
            if (string.IsNullOrEmpty(secret)) return baseUrl;
            return $"{baseUrl}&timestamp={timestamp}&sign={Uri.EscapeDataString(sign)}";
        }
    }

    public class DingTalkConfigDto
    {
        public string WebhookUrl { get; set; }
        public string Secret { get; set; }
        public List<string> AtMobiles { get; set; }
        public bool? IsAtAll { get; set; }
        public bool? Enabled { get; set; }
    }

    public class TestNotificationRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
