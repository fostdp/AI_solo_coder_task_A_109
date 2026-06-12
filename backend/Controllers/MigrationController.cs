using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptureMonitor.Models;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly ISaltMigrationService _service;

        public MigrationController(ISaltMigrationService service)
        {
            _service = service;
        }

        [HttpPost("predict")]
        [Authorize]
        public async Task<ActionResult<MigrationPrediction>> Predict([FromBody] MigrationPrediction request)
        {
            var prediction = await _service.PredictMigrationAsync(request);
            return Ok(prediction);
        }

        [HttpGet("simulate/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<object>> SimulateMigration(
            int sculptureId,
            double porosity = 0.35,
            double saturation = 0.6,
            double temperature = 20.0,
            double humidity = 65.0,
            int hours = 72)
        {
            var request = new MigrationPrediction
            {
                SculptureId = sculptureId,
                Porosity = porosity,
                Saturation = saturation,
                Temperature = temperature,
                Humidity = humidity,
                PredictionHours = hours
            };

            var prediction = await _service.PredictMigrationAsync(request);
            
            return Ok(new
            {
                prediction.Id,
                prediction.SculptureId,
                prediction.Porosity,
                prediction.Saturation,
                prediction.Temperature,
                prediction.Humidity,
                prediction.PredictionHours,
                prediction.CreatedAt,
                Points = prediction.MigrationPredictionPoints
                    .Select(p => new
                    {
                        p.TimeHour,
                        p.DepthCm,
                        p.Concentration
                    })
                    .OrderBy(p => p.DepthCm)
                    .ThenBy(p => p.TimeHour)
                    .ToList()
            });
        }

        [HttpGet("parameters/default")]
        [Authorize]
        public ActionResult<object> GetDefaultParameters()
        {
            return Ok(new
            {
                Porosity = new { Min = 0.1, Max = 0.6, Default = 0.35, Unit = "%" },
                Saturation = new { Min = 0.1, Max = 1.0, Default = 0.6, Unit = "%" },
                Temperature = new { Min = -10, Max = 50, Default = 20, Unit = "°C" },
                Humidity = new { Min = 10, Max = 100, Default = 65, Unit = "% RH" },
                PredictionHours = new { Min = 1, Max = 720, Default = 72, Unit = "小时" },
                SurfaceConcentration = new { Min = 100, Max = 5000, Default = 1000, Unit = "ppm" }
            });
        }

        [HttpGet("history/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetPredictionHistory(int sculptureId, int limit = 10)
        {
            var history = await _service.GetPredictionHistoryAsync(sculptureId, limit);
            return Ok(history);
        }
    }
}
