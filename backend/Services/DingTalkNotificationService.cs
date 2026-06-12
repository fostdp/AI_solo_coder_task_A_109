using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;

namespace SculptureMonitor.Services
{
    public class DingTalkNotificationService : IDingTalkNotificationService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public DingTalkNotificationService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<bool> SendAlertNotificationAsync(Models.Alert alert)
        {
            var config = await _context.DingTalkConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled.GetValueOrDefault() || string.IsNullOrEmpty(config.WebhookUrl))
                return false;

            var title = alert.Severity == "Critical" ? "【紧急告警】" : "【预警通知】";
            var sculpture = await _context.Sculptures.FindAsync(alert.SculptureId);
            var sculptureName = sculpture?.Name ?? "未知泥塑";

            var content = $@"{title}
**泥塑名称**: {sculptureName}
**告警类型**: {alert.AlertType}
**严重程度**: {alert.Severity}
**告警标题**: {alert.Title}
**告警内容**: {alert.Message}
**当前值**: {alert.ActualValue:F2}
**阈值**: {alert.ThresholdValue:F2}
**告警时间**: {alert.CreatedAt:yyyy-MM-dd HH:mm:ss}

请相关人员及时处理。";

            return await SendMarkdownMessageAsync(alert.Title, content, config.WebhookUrl, config.Secret);
        }

        public async Task<bool> SendTextMessageAsync(string content, string webhookUrl = null, string secret = null)
        {
            var config = await GetConfigAsync();
            var url = webhookUrl ?? config?.WebhookUrl;
            var sec = secret ?? config?.Secret;

            if (string.IsNullOrEmpty(url)) return false;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sign = GenerateSignature(sec, timestamp);
            var fullUrl = BuildWebhookUrl(url, sec, timestamp, sign);

            var payload = new
            {
                msgtype = "text",
                text = new { content = content }
            };

            return await SendRequestAsync(fullUrl, payload);
        }

        public async Task<bool> SendMarkdownMessageAsync(string title, string content, string webhookUrl = null, string secret = null)
        {
            var config = await GetConfigAsync();
            var url = webhookUrl ?? config?.WebhookUrl;
            var sec = secret ?? config?.Secret;

            if (string.IsNullOrEmpty(url)) return false;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sign = GenerateSignature(sec, timestamp);
            var fullUrl = BuildWebhookUrl(url, sec, timestamp, sign);

            var payload = new
            {
                msgtype = "markdown",
                markdown = new { title = title, text = content }
            };

            return await SendRequestAsync(fullUrl, payload);
        }

        public string GenerateSignature(string secret, long timestamp)
        {
            if (string.IsNullOrEmpty(secret)) return string.Empty;

            var stringToSign = $"{timestamp}\n{secret}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hash);
        }

        public string BuildWebhookUrl(string baseUrl, string secret, long timestamp, string sign)
        {
            if (string.IsNullOrEmpty(secret)) return baseUrl;
            return $"{baseUrl}&timestamp={timestamp}&sign={Uri.EscapeDataString(sign)}";
        }

        private async Task<bool> SendRequestAsync(string url, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<Models.DingTalkConfig> GetConfigAsync()
        {
            return await _context.DingTalkConfigs.FirstOrDefaultAsync();
        }
    }
}
