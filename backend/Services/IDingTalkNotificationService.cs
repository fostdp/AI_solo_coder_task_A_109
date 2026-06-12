namespace SculptureMonitor.Services
{
    public interface IDingTalkNotificationService
    {
        Task<bool> SendTextMessageAsync(string content, string webhookUrl = null, string secret = null);
        Task<bool> SendMarkdownMessageAsync(string title, string content, string webhookUrl = null, string secret = null);
        Task<bool> SendAlertNotificationAsync(Models.Alert alert);
        string GenerateSignature(string secret, long timestamp);
        string BuildWebhookUrl(string baseUrl, string secret, long timestamp, string sign);
    }
}
