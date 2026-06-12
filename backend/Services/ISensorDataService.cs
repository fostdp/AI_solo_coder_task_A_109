using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface ISensorDataService
    {
        Task<IEnumerable<SensorData>> GetAllAsync();
        Task<SensorData> GetByIdAsync(long id);
        Task<IEnumerable<SensorData>> GetBySensorIdAsync(int sensorId, int limit = 100);
        Task<IEnumerable<SensorData>> GetBySculptureIdAsync(int sculptureId, int limit = 100);
        Task<IEnumerable<SensorData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<SensorData> CreateAsync(SensorData sensorData);
        Task<IEnumerable<SensorData>> CreateBatchAsync(IEnumerable<SensorData> sensorDataList);
        Task<object> GetSensorStatusSummaryAsync();
        Task<IEnumerable<object>> GetSensorLogsAsync(string sensorId, int days = 7);
    }
}
