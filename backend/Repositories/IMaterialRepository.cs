using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public interface IMaterialRepository
    {
        Task<IEnumerable<Material>> GetAllAsync();
        Task<Material> GetByIdAsync(string id);
        Task<Material> GetByNameAsync(string name);
        Task<Material> CreateAsync(Material material);
        Task<Material> UpdateAsync(Material material);
        Task<bool> DeleteAsync(string id);
        Task<MaterialScore> CreateScoreAsync(MaterialScore score);
        Task<IEnumerable<MaterialScore>> GetScoresBySculptureIdAsync(int sculptureId, int limit = 10);
        Task<MaterialScore> GetLatestScoreAsync(int sculptureId, string materialId);
        Task<int> GetCountAsync();
    }
}
