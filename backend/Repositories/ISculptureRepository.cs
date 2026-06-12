using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public interface ISculptureRepository
    {
        Task<IEnumerable<Sculpture>> GetAllAsync();
        Task<Sculpture> GetByIdAsync(int id);
        Task<Sculpture> CreateAsync(Sculpture sculpture);
        Task<Sculpture> UpdateAsync(Sculpture sculpture);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Sculpture>> GetByStatusAsync(string status);
        Task<int> GetCountAsync();
    }
}
