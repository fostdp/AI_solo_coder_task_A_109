using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;
using SculptureMonitor.Repositories;

namespace SculptureMonitor.Services
{
    public class SensorDataService : ISensorDataService
    {
        private readonly ISensorDataRepository _repository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IAlertService _alertService;
        private readonly ISculptureService _sculptureService;
        private readonly AppDbContext _context;

        public SensorDataService(ISensorDataRepository repository, ISensorRepository sensorRepository,
            IAlertService alertService, ISculptureService sculptureService, AppDbContext context)
        {
            _repository = repository;
            _sensorRepository = sensorRepository;
            _alertService = alertService;
            _sculptureService = sculptureService;
            _context = context;
        }

        public async Task<IEnumerable<SensorData>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SensorData> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<SensorData>> GetBySensorIdAsync(int sensorId, int limit = 100)
        {
            return await _repository.GetBySensorIdAsync(sensorId, limit);
        }

        public async Task<IEnumerable<SensorData>> GetBySculptureIdAsync(int sculptureId, int limit = 100)
        {
            return await _repository.GetBySculptureIdAsync(sculptureId, limit);
        }

        public async Task<IEnumerable<SensorData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _repository.GetByTimeRangeAsync(startTime, endTime);
        }

        public async Task<SensorData> CreateAsync(SensorData sensorData)
        {
            sensorData.Timestamp = DateTime.Now;
            var result = await _repository.CreateAsync(sensorData);

            await _alertService.ProcessSensorDataForAlertsAsync(sensorData);
            
            var sensor = await _sensorRepository.GetByIdAsync(sensorData.SensorId);
            if (sensor != null)
            {
                sensor.LastCalibration = DateTime.Now;
                await _sensorRepository.UpdateAsync(sensor);
                await _sculptureService.UpdateSculptureStatusAsync(sensor.SculptureId);
            }

            return result;
        }

        public async Task<IEnumerable<SensorData>> CreateBatchAsync(IEnumerable<SensorData> sensorDataList)
        {
            var results = new List<SensorData>();
            foreach (var data in sensorDataList)
            {
                var result = await CreateAsync(data);
                results.Add(result);
            }
            return results;
        }

        public async Task<object> GetSensorStatusSummaryAsync()
        {
            var sensors = await _context.Sensors.ToListAsync();
            var now = DateTime.Now;

            var active = sensors.Count(s => s.Status == "Active");
            var inactive = sensors.Count(s => s.Status != "Active");
            
            var recentData = await _context.SensorData
                .GroupBy(sd => sd.SensorId)
                .Select(g => new { SensorId = g.Key, LatestTime = g.Max(sd => sd.Timestamp) })
                .ToDictionaryAsync(x => x.SensorId, x => x.LatestTime);

            var offline = sensors.Count(s => 
                !recentData.ContainsKey(s.Id) || 
                (now - recentData[s.Id]).TotalMinutes > 90);

            var ionSensors = sensors.Count(s => s.SensorType == "ion");
            var envSensors = sensors.Count(s => s.SensorType == "environment");

            return new
            {
                TotalSensors = sensors.Count,
                IonSensors = ionSensors,
                EnvironmentSensors = envSensors,
                Active = active,
                Inactive = inactive,
                Offline = offline,
                Online = sensors.Count - offline
            };
        }

        public async Task<IEnumerable<object>> GetSensorLogsAsync(string sensorId, int days = 7)
        {
            var startDate = DateTime.Now.AddDays(-days);
            
            var sensor = await _context.Sensors.FirstOrDefaultAsync(s => s.SensorCode == sensorId);
            if (sensor == null) return Enumerable.Empty<object>();

            return await _context.SensorData
                .Where(sd => sd.SensorId == sensor.Id && sd.Timestamp >= startDate)
                .OrderByDescending(sd => sd.Timestamp)
                .Select(sd => new
                {
                    sd.Timestamp,
                    sd.Temperature,
                    sd.Humidity,
                    sd.SodiumIon,
                    sd.ChlorideIon,
                    sd.SaltConcentration,
                    sd.CrystalCoverage,
                    sd.SignalStrength
                })
                .ToListAsync();
        }
    }
}
