using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface IAlertService
    {
        Task<IEnumerable<Alert>> GetAllAsync();
        Task<Alert> GetByIdAsync(int id);
        Task<IEnumerable<Alert>> GetBySculptureIdAsync(int sculptureId);
        Task<IEnumerable<Alert>> GetByStatusAsync(string status);
        Task<IEnumerable<Alert>> GetBySeverityAsync(string severity);
        Task<Alert> CreateAsync(Alert alert);
        Task<Alert> UpdateAsync(Alert alert);
        Task<bool> DeleteAsync(int id);
        Task<Alert> ResolveAlertAsync(int id, string resolvedNote);
        Task ProcessSensorDataForAlertsAsync(SensorData sensorData);
    }
}
