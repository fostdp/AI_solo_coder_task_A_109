using MathNet.Numerics.LinearAlgebra;
using SculptureMonitor.Data;
using SculptureMonitor.Models;
using SculptureMonitor.Repositories;

namespace SculptureMonitor.Services
{
    public class SaltMigrationService : ISaltMigrationService
    {
        private readonly AppDbContext _context;
        private readonly ISculptureRepository _sculptureRepository;

        public SaltMigrationService(AppDbContext context, ISculptureRepository sculptureRepository)
        {
            _context = context;
            _sculptureRepository = sculptureRepository;
        }

        public async Task<MigrationPrediction> PredictMigrationAsync(int sculptureId, int predictionHours, double initialConcentration)
        {
            var sculpture = await _sculptureRepository.GetByIdAsync(sculptureId);
            if (sculpture == null)
                throw new KeyNotFoundException($"Sculpture with id {sculptureId} not found");

            var prediction = RunRichardsSimulation(initialConcentration, predictionHours);
            prediction.SculptureId = sculptureId;

            _context.MigrationPredictions.Add(prediction);
            await _context.SaveChangesAsync();

            return prediction;
        }

        public MigrationPrediction RunRichardsSimulation(double initialConcentration, int totalHours, 
            double dt = 1.0, double dz = 0.5, double maxDepth = 50.0)
        {
            int nz = (int)(maxDepth / dz) + 1;
            int nt = (int)(totalHours / dt);

            double[] z = new double[nz];
            for (int i = 0; i < nz; i++)
            {
                z[i] = i * dz;
            }

            double[] c = new double[nz];
            for (int i = 0; i < nz; i++)
            {
                c[i] = initialConcentration * Math.Exp(-z[i] / 10.0);
            }

            double theta = 0.35;
            double q = 0.01;
            double D = CalculateDiffusionCoefficient(theta);

            var prediction = new MigrationPrediction
            {
                PredictionType = "SaltMigration",
                PredictionHours = totalHours,
                PredictionTime = DateTime.Now,
                MaxConcentration = 0,
                AverageConcentration = 0
            };

            for (int t = 0; t < nt; t++)
            {
                c = ApplyBoundaryConditions(c, initialConcentration, 0);
                c = SolveFDM(c, D, q, dt, dz, nz);
            }

            double maxC = 0;
            double sumC = 0;
            for (int i = 0; i < nz; i++)
            {
                if (c[i] > maxC) maxC = c[i];
                sumC += c[i];

                prediction.PredictionPoints.Add(new MigrationPredictionPoint
                {
                    Depth = z[i],
                    Concentration = Math.Round(c[i], 4),
                    TimeStep = t * dt,
                    Timestamp = DateTime.Now.AddHours(t * dt)
                });
            }

            prediction.MaxConcentration = Math.Round(maxC, 4);
            prediction.AverageConcentration = Math.Round(sumC / nz, 4);

            return prediction;
        }

        public double CalculateDiffusionCoefficient(double theta)
        {
            double D0 = 1.5e-9;
            double tau = 0.6;
            return D0 * tau * theta;
        }

        public double CalculateDispersionCoefficient(double velocity)
        {
            double alphaL = 0.01;
            return alphaL * Math.Abs(velocity);
        }

        public double[] ApplyBoundaryConditions(double[] concentration, double surfaceConcentration, double bottomConcentration)
        {
            concentration[0] = surfaceConcentration;
            concentration[concentration.Length - 1] = bottomConcentration;
            return concentration;
        }

        public double[] SolveFDM(double[] concentration, double D, double q, double dt, double dz, int n)
        {
            double[] cNew = new double[n];
            double r = D * dt / (dz * dz);
            double s = q * dt / (2 * dz);

            for (int i = 1; i < n - 1; i++)
            {
                cNew[i] = concentration[i]
                    + r * (concentration[i + 1] - 2 * concentration[i] + concentration[i - 1])
                    - s * (concentration[i + 1] - concentration[i - 1]);

                if (cNew[i] < 0) cNew[i] = 0;
            }

            cNew[0] = concentration[0];
            cNew[n - 1] = concentration[n - 1];

            return cNew;
        }
    }
}
