using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public class SensorDataRepository : ISensorDataRepository
    {
        private readonly AppDbContext _context;

        public SensorDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SensorData>> GetAllAsync()
        {
            return await _context.SensorData
                .Include(sd => sd.Sensor)
                .OrderByDescending(sd => sd.Timestamp)
                .Take(1000)
                .ToListAsync();
        }

        public async Task<SensorData> GetByIdAsync(long id)
        {
            return await _context.SensorData
                .Include(sd => sd.Sensor)
                .FirstOrDefaultAsync(sd => sd.Id == id);
        }

        public async Task<IEnumerable<SensorData>> GetBySensorIdAsync(int sensorId, int limit = 100)
        {
            return await _context.SensorData
                .Where(sd => sd.SensorId == sensorId)
                .OrderByDescending(sd => sd.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorData>> GetBySculptureIdAsync(int sculptureId, int limit = 100)
        {
            return await _context.SensorData
                .Include(sd => sd.Sensor)
                .Where(sd => sd.Sensor.SculptureId == sculptureId)
                .OrderByDescending(sd => sd.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.SensorData
                .Where(sd => sd.Timestamp >= startTime && sd.Timestamp <= endTime)
                .OrderBy(sd => sd.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorData>> GetBySensorIdAndTimeRangeAsync(int sensorId, DateTime startTime, DateTime endTime)
        {
            return await _context.SensorData
                .Where(sd => sd.SensorId == sensorId && sd.Timestamp >= startTime && sd.Timestamp <= endTime)
                .OrderBy(sd => sd.Timestamp)
                .ToListAsync();
        }

        public async Task<SensorData> CreateAsync(SensorData sensorData)
        {
            _context.SensorData.Add(sensorData);
            await _context.SaveChangesAsync();
            return sensorData;
        }

        public async Task<IEnumerable<SensorData>> CreateBatchAsync(IEnumerable<SensorData> sensorDataList)
        {
            _context.SensorData.AddRange(sensorDataList);
            await _context.SaveChangesAsync();
            return sensorDataList;
        }

        public async Task<SensorData> GetLatestBySensorIdAsync(int sensorId)
        {
            return await _context.SensorData
                .Where(sd => sd.SensorId == sensorId)
                .OrderByDescending(sd => sd.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.SensorData.CountAsync();
        }

        public async Task<int> GetCountBySensorIdAsync(int sensorId)
        {
            return await _context.SensorData.CountAsync(sd => sd.SensorId == sensorId);
        }
    }
}
