using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Material>> GetAllAsync()
        {
            return await _context.Materials.ToListAsync();
        }

        public async Task<Material> GetByIdAsync(string id)
        {
            return await _context.Materials.FindAsync(id);
        }

        public async Task<Material> GetByNameAsync(string name)
        {
            return await _context.Materials.FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<Material> CreateAsync(Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<Material> UpdateAsync(Material material)
        {
            _context.Entry(material).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return false;
            
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MaterialScore> CreateScoreAsync(MaterialScore score)
        {
            _context.MaterialScores.Add(score);
            await _context.SaveChangesAsync();
            return score;
        }

        public async Task<IEnumerable<MaterialScore>> GetScoresBySculptureIdAsync(int sculptureId, int limit = 10)
        {
            return await _context.MaterialScores
                .Where(ms => ms.SculptureId == sculptureId)
                .OrderByDescending(ms => ms.CalculatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<MaterialScore> GetLatestScoreAsync(int sculptureId, string materialId)
        {
            return await _context.MaterialScores
                .Where(ms => ms.SculptureId == sculptureId && ms.MaterialId == materialId)
                .OrderByDescending(ms => ms.CalculatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Materials.CountAsync();
        }
    }
}
