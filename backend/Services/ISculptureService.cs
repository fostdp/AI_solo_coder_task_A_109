using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface ISculptureService
    {
        Task<IEnumerable<Sculpture>> GetAllAsync();
        Task<Sculpture> GetByIdAsync(int id);
        Task<Sculpture> CreateAsync(Sculpture sculpture);
        Task<Sculpture> UpdateAsync(Sculpture sculpture);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Sculpture>> GetByStatusAsync(string status);
        Task<object> GetDashboardStatsAsync();
        Task<IEnumerable<object>> GetTimeSeriesDataAsync(int sculptureId, int days = 7);
        Task<IEnumerable<object>> GetHeatmapDataAsync(int sculptureId);
        Task UpdateSculptureStatusAsync(int sculptureId);
    }
}
