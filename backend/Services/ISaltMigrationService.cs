using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface ISaltMigrationService
    {
        Task<MigrationPrediction> PredictMigrationAsync(int sculptureId, int predictionHours, double initialConcentration);
        MigrationPrediction RunRichardsSimulation(double initialConcentration, int totalHours, double dt = 1.0, double dz = 0.5, double maxDepth = 50.0);
        double CalculateDiffusionCoefficient(double theta);
        double CalculateDispersionCoefficient(double velocity);
        double[] ApplyBoundaryConditions(double[] concentration, double surfaceConcentration, double bottomConcentration);
        double[] SolveFDM(double[] concentration, double D, double q, double dt, double dz, int n);
    }
}
