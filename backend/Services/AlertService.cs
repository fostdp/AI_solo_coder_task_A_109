using SculptureMonitor.Data;
using SculptureMonitor.Models;
using SculptureMonitor.Repositories;

namespace SculptureMonitor.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _repository;
        private readonly IDingTalkNotificationService _dingTalkService;
        private readonly ISensorRepository _sensorRepository;
        private readonly AppDbContext _context;

        public AlertService(IAlertRepository repository, IDingTalkNotificationService dingTalkService, 
            ISensorRepository sensorRepository, AppDbContext context)
        {
            _repository = repository;
            _dingTalkService = dingTalkService;
            _sensorRepository = sensorRepository;
            _context = context;
        }

        public async Task<IEnumerable<Alert>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Alert> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Alert>> GetBySculptureIdAsync(int sculptureId)
        {
            return await _repository.GetBySculptureIdAsync(sculptureId);
        }

        public async Task<IEnumerable<Alert>> GetByStatusAsync(string status)
        {
            return await _repository.GetByStatusAsync(status);
        }

        public async Task<IEnumerable<Alert>> GetBySeverityAsync(string severity)
        {
            return await _repository.GetBySeverityAsync(severity);
        }

        public async Task<Alert> CreateAsync(Alert alert)
        {
            var result = await _repository.CreateAsync(alert);
            if (result != null)
            {
                await _dingTalkService.SendAlertNotificationAsync(result);
                result.DingTalkNotified = true;
                result.DingTalkNotifiedAt = DateTime.Now;
                await _repository.UpdateAsync(result);
            }
            return result;
        }

        public async Task<Alert> UpdateAsync(Alert alert)
        {
            if (!await _repository.ExistsAsync(alert.Id))
                throw new KeyNotFoundException($"Alert with id {alert.Id} not found");
            return await _repository.UpdateAsync(alert);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException($"Alert with id {id} not found");
            return await _repository.DeleteAsync(id);
        }

        public async Task<Alert> ResolveAlertAsync(int id, string resolvedNote)
        {
            var alert = await _repository.GetByIdAsync(id);
            if (alert == null)
                throw new KeyNotFoundException($"Alert with id {id} not found");

            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.Now;
            alert.ResolvedNote = resolvedNote;
            return await _repository.UpdateAsync(alert);
        }

        public async Task ProcessSensorDataForAlertsAsync(SensorData sensorData)
        {
            var sensor = await _sensorRepository.GetByIdAsync(sensorData.SensorId);
            if (sensor == null) return;

            var sculptureId = sensor.SculptureId;
            var thresholds = await _context.AlertThresholds.ToListAsync();

            if (sensorData.CrystalCoverage.HasValue)
            {
                var crystalThreshold = thresholds.FirstOrDefault(t => t.ParameterName == "CrystalCoverage");
                if (crystalThreshold != null)
                {
                    if (sensorData.CrystalCoverage > crystalThreshold.CriticalThreshold)
                    {
                        await CreateAlertAsync(sculptureId, "Critical", "盐结晶覆盖率过高", 
                            $"盐结晶覆盖率达到 {sensorData.CrystalCoverage:F1}%，超过阈值 {crystalThreshold.CriticalThreshold}%",
                            "CrystalCoverage", crystalThreshold.CriticalThreshold, sensorData.CrystalCoverage.Value);
                    }
                    else if (sensorData.CrystalCoverage > crystalThreshold.WarningThreshold)
                    {
                        await CreateAlertAsync(sculptureId, "Warning", "盐结晶覆盖率警告",
                            $"盐结晶覆盖率达到 {sensorData.CrystalCoverage:F1}%，超过警告阈值 {crystalThreshold.WarningThreshold}%",
                            "CrystalCoverage", crystalThreshold.WarningThreshold, sensorData.CrystalCoverage.Value);
                    }
                }
            }

            if (sensorData.SodiumIon.HasValue)
            {
                var sodiumThreshold = thresholds.FirstOrDefault(t => t.ParameterName == "SodiumIon");
                if (sodiumThreshold != null)
                {
                    if (sensorData.SodiumIon > sodiumThreshold.CriticalThreshold)
                    {
                        await CreateAlertAsync(sculptureId, "Critical", "钠离子浓度过高",
                            $"Na⁺浓度达到 {sensorData.SodiumIon:F0}ppm，超过阈值 {sodiumThreshold.CriticalThreshold}ppm",
                            "SodiumIon", sodiumThreshold.CriticalThreshold, sensorData.SodiumIon.Value);
                    }
                    else if (sensorData.SodiumIon > sodiumThreshold.WarningThreshold)
                    {
                        await CreateAlertAsync(sculptureId, "Warning", "钠离子浓度警告",
                            $"Na⁺浓度达到 {sensorData.SodiumIon:F0}ppm，超过警告阈值 {sodiumThreshold.WarningThreshold}ppm",
                            "SodiumIon", sodiumThreshold.WarningThreshold, sensorData.SodiumIon.Value);
                    }
                }
            }
        }

        private async Task CreateAlertAsync(int sculptureId, string severity, string title, string message,
            string alertType, double thresholdValue, double actualValue)
        {
            var recentAlerts = await _repository.GetBySculptureIdAsync(sculptureId);
            var existingAlert = recentAlerts.FirstOrDefault(a => 
                a.AlertType == alertType && 
                a.Status == "Pending" &&
                a.Severity == severity &&
                (DateTime.Now - a.CreatedAt).TotalHours < 1);

            if (existingAlert != null) return;

            var alert = new Alert
            {
                SculptureId = sculptureId,
                Severity = severity,
                Title = title,
                Message = message,
                AlertType = alertType,
                ThresholdValue = thresholdValue,
                ActualValue = actualValue,
                Status = "Pending"
            };

            await CreateAsync(alert);
        }
    }
}
