using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface IMaterialAdaptationService
    {
        Task<MaterialScore> CalculateScoreAsync(int materialId, int sculptureId);
        Task<IEnumerable<MaterialScore>> CalculateAllScoresAsync(int sculptureId);
        double CalculateContactAngleScore(double? contactAngle);
        double CalculatePenetrationDepthScore(double? penetrationDepth);
        double CalculateStrengthMatchingScore(double? strengthMatching);
        double CalculateWeatherResistanceScore(double? weatherResistance);
        double CalculateReversibilityScore(double? reversibility);
        double CalculateCostPerformanceScore(double? costPerformance);
        double CalculateTotalScore(Dictionary<string, double> scores);
        string GenerateRecommendation(double totalScore, Material material);
    }
}
