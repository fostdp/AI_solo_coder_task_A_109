using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialAdaptationService _service;
        private readonly AppDbContext _context;

        public MaterialsController(IMaterialAdaptationService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterials()
        {
            var materials = await _context.Materials
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Manufacturer,
                    m.Description,
                    m.DefaultContactAngle,
                    m.DefaultPenetrationDepth,
                    m.DefaultStrength,
                    m.WeatherResistance,
                    m.Reversibility,
                    m.CostPerKg
                })
                .ToListAsync();
            return Ok(materials);
        }

        [HttpGet("adaptation/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterialScores(int sculptureId)
        {
            var scores = await _service.CalculateMaterialScoresAsync(sculptureId);
            return Ok(scores);
        }

        [HttpPost("calculate/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> CalculateScores(int sculptureId, [FromBody] object parameters)
        {
            var scores = await _service.CalculateMaterialScoresAsync(sculptureId, parameters);
            return Ok(scores);
        }

        [HttpGet("{materialId}")]
        [Authorize]
        public async Task<ActionResult<object>> GetMaterial(string materialId)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null) return NotFound();
            return Ok(new
            {
                material.Id,
                material.Name,
                material.Manufacturer,
                material.Description,
                material.DefaultContactAngle,
                material.DefaultPenetrationDepth,
                material.DefaultStrength,
                material.WeatherResistance,
                material.Reversibility,
                material.CostPerKg
            });
        }

        [HttpGet("weights")]
        [Authorize]
        public ActionResult<object> GetScoringWeights()
        {
            var weights = new
            {
                ContactAngle = 0.20,
                PenetrationDepth = 0.25,
                StrengthMatch = 0.20,
                WeatherResistance = 0.15,
                Reversibility = 0.10,
                CostPerformance = 0.10
            };
            return Ok(weights);
        }

        [HttpGet("history/{sculptureId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetCalculationHistory(int sculptureId, int limit = 10)
        {
            var history = await _context.MaterialScores
                .Where(ms => ms.SculptureId == sculptureId)
                .OrderByDescending(ms => ms.CalculatedAt)
                .Take(limit)
                .Select(ms => new
                {
                    ms.Id,
                    ms.MaterialId,
                    ms.TotalScore,
                    ms.Recommendation,
                    ms.CalculatedAt
                })
                .ToListAsync();
            return Ok(history);
        }
    }
}
