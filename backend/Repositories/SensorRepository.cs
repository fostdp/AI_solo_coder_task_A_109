using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly AppDbContext _context;

        public SensorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sensor>> GetAllAsync()
        {
            return await _context.Sensors
                .Include(s => s.Sculpture)
                .ToListAsync();
        }

        public async Task<Sensor> GetByIdAsync(int id)
        {
            return await _context.Sensors
                .Include(s => s.Sculpture)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sensor> GetByCodeAsync(string sensorCode)
        {
            return await _context.Sensors
                .Include(s => s.Sculpture)
                .FirstOrDefaultAsync(s => s.SensorCode == sensorCode);
        }

        public async Task<IEnumerable<Sensor>> GetBySculptureIdAsync(int sculptureId)
        {
            return await _context.Sensors
                .Where(s => s.SculptureId == sculptureId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sensor>> GetByTypeAsync(string sensorType)
        {
            return await _context.Sensors
                .Where(s => s.SensorType == sensorType)
                .Include(s => s.Sculpture)
                .ToListAsync();
        }

        public async Task<Sensor> CreateAsync(Sensor sensor)
        {
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
            return sensor;
        }

        public async Task<Sensor> UpdateAsync(Sensor sensor)
        {
            _context.Entry(sensor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return sensor;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null) return false;
            
            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Sensors.CountAsync();
        }

        public async Task<int> GetCountByTypeAsync(string sensorType)
        {
            return await _context.Sensors.CountAsync(s => s.SensorType == sensorType);
        }
    }
}
