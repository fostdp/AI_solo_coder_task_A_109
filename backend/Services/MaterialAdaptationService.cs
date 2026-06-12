using SculptureMonitor.Models;
using SculptureMonitor.Repositories;

namespace SculptureMonitor.Services
{
    public class MaterialAdaptationService : IMaterialAdaptationService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly ISculptureRepository _sculptureRepository;

        private const double W_CONTACT_ANGLE = 0.20;
        private const double W_PENETRATION_DEPTH = 0.25;
        private const double W_STRENGTH_MATCHING = 0.20;
        private const double W_WEATHER_RESISTANCE = 0.15;
        private const double W_REVERSIBILITY = 0.10;
        private const double W_COST_PERFORMANCE = 0.10;

        public MaterialAdaptationService(IMaterialRepository materialRepository, ISculptureRepository sculptureRepository)
        {
            _materialRepository = materialRepository;
            _sculptureRepository = sculptureRepository;
        }

        public async Task<MaterialScore> CalculateScoreAsync(int materialId, int sculptureId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null)
                throw new KeyNotFoundException($"Material with id {materialId} not found");

            var sculpture = await _sculptureRepository.GetByIdAsync(sculptureId);
            if (sculpture == null)
                throw new KeyNotFoundException($"Sculpture with id {sculptureId} not found");

            var scores = new Dictionary<string, double>
            {
                ["ContactAngle"] = CalculateContactAngleScore(material.ContactAngle),
                ["PenetrationDepth"] = CalculatePenetrationDepthScore(material.PenetrationDepth),
                ["StrengthMatching"] = CalculateStrengthMatchingScore(material.StrengthMatching),
                ["WeatherResistance"] = CalculateWeatherResistanceScore(material.WeatherResistance),
                ["Reversibility"] = CalculateReversibilityScore(material.Reversibility),
                ["CostPerformance"] = CalculateCostPerformanceScore(material.CostPerformance)
            };

            var totalScore = CalculateTotalScore(scores);
            var recommendation = GenerateRecommendation(totalScore, material);

            var materialScore = new MaterialScore
            {
                MaterialId = materialId,
                SculptureId = sculptureId,
                ContactAngleScore = scores["ContactAngle"],
                PenetrationDepthScore = scores["PenetrationDepth"],
                StrengthMatchingScore = scores["StrengthMatching"],
                WeatherResistanceScore = scores["WeatherResistance"],
                ReversibilityScore = scores["Reversibility"],
                CostPerformanceScore = scores["CostPerformance"],
                TotalScore = totalScore,
                Recommendation = recommendation
            };

            return await _materialRepository.CreateMaterialScoreAsync(materialScore);
        }

        public async Task<IEnumerable<MaterialScore>> CalculateAllScoresAsync(int sculptureId)
        {
            var materials = await _materialRepository.GetAllAsync();
            var scores = new List<MaterialScore>();

            foreach (var material in materials)
            {
                var score = await CalculateScoreAsync(material.Id, sculptureId);
                scores.Add(score);
            }

            return scores.OrderByDescending(s => s.TotalScore);
        }

        public double CalculateContactAngleScore(double? contactAngle)
        {
            if (!contactAngle.HasValue) return 0;
            double ca = contactAngle.Value;
            if (ca >= 90 && ca <= 120) return 100;
            if (ca >= 80 && ca < 90) return 80 + (ca - 80) * 2;
            if (ca > 120 && ca <= 130) return 80 + (130 - ca) * 2;
            if (ca >= 70 && ca < 80) return 60 + (ca - 70) * 2;
            if (ca > 130 && ca <= 140) return 60 + (140 - ca) * 2;
            if (ca < 70) return Math.Max(0, ca / 70 * 60);
            return Math.Max(0, (180 - ca) / 40 * 60);
        }

        public double CalculatePenetrationDepthScore(double? penetrationDepth)
        {
            if (!penetrationDepth.HasValue) return 0;
            double pd = penetrationDepth.Value;
            if (pd >= 3 && pd <= 5) return 100;
            if (pd >= 2 && pd < 3) return 70 + (pd - 2) * 30;
            if (pd > 5 && pd <= 7) return 70 + (7 - pd) * 15;
            if (pd >= 1 && pd < 2) return 40 + (pd - 1) * 30;
            if (pd > 7 && pd <= 10) return 40 + (10 - pd) * 10;
            if (pd < 1) return Math.Max(0, pd * 40);
            return Math.Max(0, (15 - pd) / 5 * 40);
        }

        public double CalculateStrengthMatchingScore(double? strengthMatching)
        {
            if (!strengthMatching.HasValue) return 0;
            double sm = strengthMatching.Value;
            if (sm >= 80 && sm <= 120) return 100;
            if (sm >= 60 && sm < 80) return 60 + (sm - 60) * 2;
            if (sm > 120 && sm <= 150) return 60 + (150 - sm) * (40.0 / 30);
            if (sm >= 40 && sm < 60) return 30 + (sm - 40) * 1.5;
            if (sm > 150 && sm <= 200) return 30 + (200 - sm) * 0.6;
            if (sm < 40) return Math.Max(0, sm / 40 * 30);
            return Math.Max(0, (250 - sm) / 50 * 30);
        }

        public double CalculateWeatherResistanceScore(double? weatherResistance)
        {
            if (!weatherResistance.HasValue) return 0;
            double wr = weatherResistance.Value;
            if (wr >= 0 && wr <= 100) return wr;
            if (wr > 100) return 100;
            return 0;
        }

        public double CalculateReversibilityScore(double? reversibility)
        {
            if (!reversibility.HasValue) return 0;
            double r = reversibility.Value;
            if (r >= 0 && r <= 100) return r;
            if (r > 100) return 100;
            return 0;
        }

        public double CalculateCostPerformanceScore(double? costPerformance)
        {
            if (!costPerformance.HasValue) return 0;
            double cp = costPerformance.Value;
            if (cp >= 0 && cp <= 100) return cp;
            if (cp > 100) return 100;
            return 0;
        }

        public double CalculateTotalScore(Dictionary<string, double> scores)
        {
            double total = 0;
            total += scores["ContactAngle"] * W_CONTACT_ANGLE;
            total += scores["PenetrationDepth"] * W_PENETRATION_DEPTH;
            total += scores["StrengthMatching"] * W_STRENGTH_MATCHING;
            total += scores["WeatherResistance"] * W_WEATHER_RESISTANCE;
            total += scores["Reversibility"] * W_REVERSIBILITY;
            total += scores["CostPerformance"] * W_COST_PERFORMANCE;
            return Math.Round(total, 2);
        }

        public string GenerateRecommendation(double totalScore, Material material)
        {
            if (totalScore >= 85)
                return $"{material.Name} 是非常优秀的修复材料，强烈推荐使用。总分：{totalScore:F1}";
            if (totalScore >= 70)
                return $"{material.Name} 是良好的修复材料，推荐使用。总分：{totalScore:F1}";
            if (totalScore >= 55)
                return $"{material.Name} 可以考虑使用，但建议结合实际情况评估。总分：{totalScore:F1}";
            if (totalScore >= 40)
                return $"{material.Name} 适用性一般，建议谨慎使用。总分：{totalScore:F1}";
            return $"{material.Name} 不推荐使用，建议选择其他材料。总分：{totalScore:F1}";
        }
    }
}
