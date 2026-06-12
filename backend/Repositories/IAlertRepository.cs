using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public interface IAlertRepository
    {
        Task<IEnumerable<Alert>> GetAllAsync();
        Task<Alert> GetByIdAsync(string id);
        Task<IEnumerable<Alert>> GetBySculptureIdAsync(int sculptureId, int limit = 50);
        Task<IEnumerable<Alert>> GetBySeverityAsync(string severity, int limit = 50);
        Task<IEnumerable<Alert>> GetByStatusAsync(string status, int limit = 50);
        Task<IEnumerable<Alert>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<Alert> CreateAsync(Alert alert);
        Task<Alert> UpdateAsync(Alert alert);
        Task<bool> DeleteAsync(string id);
        Task<int> GetCountAsync();
        Task<int> GetCountByStatusAsync(string status);
        Task<int> GetCountBySeverityAsync(string severity);
        Task<bool> ExistsRecentAlertAsync(int sculptureId, string alertType, int minutes = 60);
    }
}
