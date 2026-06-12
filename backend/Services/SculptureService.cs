using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;
using SculptureMonitor.Repositories;

namespace SculptureMonitor.Services
{
    public class SculptureService : ISculptureService
    {
        private readonly ISculptureRepository _repository;
        private readonly AppDbContext _context;

        public SculptureService(ISculptureRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<Sculpture>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Sculpture> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Sculpture> CreateAsync(Sculpture sculpture)
        {
            sculpture.CreatedAt = DateTime.Now;
            sculpture.UpdatedAt = DateTime.Now;
            return await _repository.CreateAsync(sculpture);
        }

        public async Task<Sculpture> UpdateAsync(Sculpture sculpture)
        {
            sculpture.UpdatedAt = DateTime.Now;
            return await _repository.UpdateAsync(sculpture);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Sculpture>> GetByStatusAsync(string status)
        {
            return await _repository.GetByStatusAsync(status);
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            var sculptures = await _repository.GetAllAsync();
            var sensors = await _context.Sensors.ToListAsync();
            var alerts = await _context.Alerts.Where(a => a.Status == "Pending").ToListAsync();

            var now = DateTime.Now;
            var activeSensors = sensors.Count(s => s.Status == "Active" && 
                (s.LastCalibration == null || (now - s.LastCalibration.Value).TotalDays < 30));

            return new
            {
                TotalSculptures = sculptures.Count(),
                NormalCount = sculptures.Count(s => s.Status == "normal"),
                WarningCount = sculptures.Count(s => s.Status == "warning"),
                AlertCount = sculptures.Count(s => s.Status == "alert"),
                ActiveSensors = activeSensors,
                OfflineSensors = sensors.Count - activeSensors,
                RecentAlerts = alerts.Count
            };
        }

        public async Task<IEnumerable<object>> GetTimeSeriesDataAsync(int sculptureId, int days = 7)
        {
            var startDate = DateTime.Now.AddDays(-days);
            
            var data = await _context.SensorData
                .Include(sd => sd.Sensor)
                .Where(sd => sd.Sensor.SculptureId == sculptureId && sd.Timestamp >= startDate)
                .OrderBy(sd => sd.Timestamp)
                .Select(sd => new
                {
                    sd.Timestamp,
                    sd.SaltConcentration,
                    sd.SodiumIon,
                    sd.Temperature,
                    sd.Humidity,
                    sd.CrystalCoverage,
                    SensorType = sd.Sensor.SensorType
                })
                .ToListAsync();

            return data.GroupBy(d => d.Timestamp.Date)
                .Select(g => new
                {
                    Timestamp = g.Key.ToString("yyyy-MM-dd HH:mm:ss"),
                    SaltLevel = Math.Round(g.Average(d => d.SaltConcentration ?? 0) / 1000.0, 4),
                    Temperature = Math.Round(g.Average(d => d.Temperature ?? 20), 1),
                    Humidity = Math.Round(g.Average(d => d.Humidity ?? 50), 1),
                    CrystalCoverage = Math.Round(g.Average(d => d.CrystalCoverage ?? 0), 2)
                });
        }

        public async Task<IEnumerable<object>> GetHeatmapDataAsync(int sculptureId)
        {
            var hotspots = new List<object>();
            var random = new Random(sculptureId);

            var sculpture = await _repository.GetByIdAsync(sculptureId);
            var baseValue = sculpture?.Status == "alert" ? 0.7 : 
                           sculpture?.Status == "warning" ? 0.4 : 0.15;

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    var centerX = 10;
                    var centerY = 15;
                    var dist = Math.Sqrt((x - centerX) ** 2 + (y - centerY) ** 2);
                    var value = Math.Max(0, 1 - dist / 15) * baseValue * (0.8 + random.NextDouble() * 0.4);
                    
                    hotspots.Add(new
                    {
                        x,
                        y,
                        value = Math.Round(value, 2)
                    });
                }
            }

            return hotspots;
        }

        public async Task UpdateSculptureStatusAsync(int sculptureId)
        {
            var sculpture = await _repository.GetByIdAsync(sculptureId);
            if (sculpture == null) return;

            var thresholds = await _context.AlertThresholds.ToListAsync();
            var coverageThreshold = thresholds.FirstOrDefault(t => t.ParameterName == "CrystalCoverage")?.CriticalThreshold ?? 30;
            var sodiumThreshold = thresholds.FirstOrDefault(t => t.ParameterName == "SodiumIon")?.CriticalThreshold ?? 500;

            var latestData = await _context.SensorData
                .Include(sd => sd.Sensor)
                .Where(sd => sd.Sensor.SculptureId == sculptureId)
                .OrderByDescending(sd => sd.Timestamp)
                .FirstOrDefaultAsync();

            if (latestData != null)
            {
                if (latestData.CrystalCoverage > coverageThreshold || latestData.SodiumIon > sodiumThreshold)
                {
                    sculpture.Status = "alert";
                }
                else if (latestData.CrystalCoverage > coverageThreshold * 0.6 || latestData.SodiumIon > sodiumThreshold * 0.6)
                {
                    sculpture.Status = "warning";
                }
                else
                {
                    sculpture.Status = "normal";
                }
                
                sculpture.UpdatedAt = DateTime.Now;
                await _repository.UpdateAsync(sculpture);
            }
        }
    }
}
