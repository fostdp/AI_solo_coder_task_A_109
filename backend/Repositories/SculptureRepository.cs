using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public class SculptureRepository : ISculptureRepository
    {
        private readonly AppDbContext _context;

        public SculptureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sculpture>> GetAllAsync()
        {
            return await _context.Sculptures
                .Include(s => s.Sensors)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Sculpture> GetByIdAsync(int id)
        {
            return await _context.Sculptures
                .Include(s => s.Sensors)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sculpture> CreateAsync(Sculpture sculpture)
        {
            _context.Sculptures.Add(sculpture);
            await _context.SaveChangesAsync();
            return sculpture;
        }

        public async Task<Sculpture> UpdateAsync(Sculpture sculpture)
        {
            _context.Entry(sculpture).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return sculpture;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sculpture = await _context.Sculptures.FindAsync(id);
            if (sculpture == null) return false;
            
            _context.Sculptures.Remove(sculpture);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Sculpture>> GetByStatusAsync(string status)
        {
            return await _context.Sculptures
                .Where(s => s.Status == status)
                .Include(s => s.Sensors)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Sculptures.CountAsync();
        }
    }
}
