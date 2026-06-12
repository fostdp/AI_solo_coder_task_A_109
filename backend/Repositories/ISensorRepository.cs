using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public interface ISensorRepository
    {
        Task<IEnumerable<Sensor>> GetAllAsync();
        Task<Sensor> GetByIdAsync(int id);
        Task<Sensor> GetByCodeAsync(string sensorCode);
        Task<IEnumerable<Sensor>> GetBySculptureIdAsync(int sculptureId);
        Task<IEnumerable<Sensor>> GetByTypeAsync(string sensorType);
        Task<Sensor> CreateAsync(Sensor sensor);
        Task<Sensor> UpdateAsync(Sensor sensor);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountAsync();
        Task<int> GetCountByTypeAsync(string sensorType);
    }
}
