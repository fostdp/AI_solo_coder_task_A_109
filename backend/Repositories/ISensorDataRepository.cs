using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public interface ISensorDataRepository
    {
        Task<IEnumerable<SensorData>> GetAllAsync();
        Task<SensorData> GetByIdAsync(long id);
        Task<IEnumerable<SensorData>> GetBySensorIdAsync(int sensorId, int limit = 100);
        Task<IEnumerable<SensorData>> GetBySculptureIdAsync(int sculptureId, int limit = 100);
        Task<IEnumerable<SensorData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<IEnumerable<SensorData>> GetBySensorIdAndTimeRangeAsync(int sensorId, DateTime startTime, DateTime endTime);
        Task<SensorData> CreateAsync(SensorData sensorData);
        Task<IEnumerable<SensorData>> CreateBatchAsync(IEnumerable<SensorData> sensorDataList);
        Task<SensorData> GetLatestBySensorIdAsync(int sensorId);
        Task<int> GetCountAsync();
        Task<int> GetCountBySensorIdAsync(int sensorId);
    }
}
