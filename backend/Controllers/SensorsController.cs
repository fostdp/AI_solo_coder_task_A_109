using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SensorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensors()
        {
            var sensors = await _context.Sensors
                .Include(s => s.Sculpture)
                .Select(s => new
                {
                    s.Id,
                    s.SensorCode,
                    s.SensorType,
                    s.Model,
                    s.Status,
                    s.InstallLocation,
                    s.InstallDate,
                    s.LastCalibration,
                    SculptureName = s.Sculpture.Name,
                    s.SculptureId
                })
                .ToListAsync();
            return Ok(sensors);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<object>> GetSensor(int id)
        {
            var sensor = await _context.Sensors
                .Include(s => s.Sculpture)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.SensorCode,
                    s.SensorType,
                    s.Model,
                    s.Status,
                    s.InstallLocation,
                    s.InstallDate,
                    s.LastCalibration,
                    s.CalibrationIntervalDays,
                    s.SamplingIntervalMinutes,
                    s.CommunicationProtocol,
                    s.BatteryLevel,
                    s.SignalStrength,
                    s.Latitude,
                    s.Longitude,
                    SculptureName = s.Sculpture.Name,
                    s.SculptureId
                })
                .FirstOrDefaultAsync();
            
            if (sensor == null) return NotFound();
            return Ok(sensor);
        }

        [HttpGet("type/{sensorType}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensorsByType(string sensorType)
        {
            var sensors = await _context.Sensors
                .Where(s => s.SensorType == sensorType)
                .Include(s => s.Sculpture)
                .Select(s => new
                {
                    s.Id,
                    s.SensorCode,
                    s.SensorType,
                    s.Model,
                    s.Status,
                    SculptureName = s.Sculpture.Name,
                    s.SculptureId
                })
                .ToListAsync();
            return Ok(sensors);
        }

        [HttpGet("sculpture/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensorsBySculpture(int sculptureId)
        {
            var sensors = await _context.Sensors
                .Where(s => s.SculptureId == sculptureId)
                .Select(s => new
                {
                    s.Id,
                    s.SensorCode,
                    s.SensorType,
                    s.Model,
                    s.Status,
                    s.InstallLocation
                })
                .ToListAsync();
            return Ok(sensors);
        }

        [HttpGet("layout")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSensorLayout()
        {
            var sensors = await _context.Sensors
                .Include(s => s.Sculpture)
                .Select(s => new
                {
                    s.Id,
                    s.SensorCode,
                    s.SensorType,
                    s.Status,
                    s.Longitude,
                    s.Latitude,
                    SculptureName = s.Sculpture.Name,
                    s.Sculpture.XPosition,
                    s.Sculpture.YPosition
                })
                .ToListAsync();
            return Ok(sensors);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Sensor>> CreateSensor([FromBody] Sensor sensor)
        {
            sensor.CreatedAt = DateTime.Now;
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSensor), new { id = sensor.Id }, sensor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateSensor(int id, [FromBody] Sensor sensor)
        {
            if (id != sensor.Id) return BadRequest();
            _context.Entry(sensor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(sensor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null) return NotFound();
            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{sensorId}/logs")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetLogs(string sensorId, int days = 7)
        {
            var sensor = await _context.Sensors.FirstOrDefaultAsync(s => s.SensorCode == sensorId);
            if (sensor == null) return NotFound();

            var startDate = DateTime.Now.AddDays(-days);
            var logs = await _context.SensorData
                .Where(sd => sd.SensorId == sensor.Id && sd.Timestamp >= startDate)
                .OrderByDescending(sd => sd.Timestamp)
                .Select(sd => new
                {
                    sd.Timestamp,
                    sd.Temperature,
                    sd.Humidity,
                    sd.SodiumIon,
                    sd.PotassiumIon,
                    sd.CalciumIon,
                    sd.SaltConcentration,
                    sd.CrystalCoverage,
                    sd.SignalStrength
                })
                .ToListAsync();

            return Ok(new
            {
                sensor = new
                {
                    sensor.Id,
                    sensor.SensorCode,
                    sensor.SensorType,
                    sensor.Model,
                    sensor.Status
                },
                logs,
                stats = new
                {
                    totalRecords = logs.Count,
                    avgTemperature = logs.Any() ? Math.Round(logs.Average(l => l.Temperature ?? 0), 2) : 0,
                    avgHumidity = logs.Any() ? Math.Round(logs.Average(l => l.Humidity ?? 0), 2) : 0,
                    packetLoss = Math.Round((double)(days * 24 * 60 / 45 - logs.Count) / (days * 24 * 60 / 45) * 100, 2)
                }
            });
        }
    }
}
