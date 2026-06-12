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
            double[] theta = new double[nz];
            for (int i = 0; i < nz; i++)
            {
                c[i] = initialConcentration * Math.Exp(-z[i] / 10.0);
                theta[i] = 0.35;
            }

            var env = new EvaporationEnvironment
            {
                Temperature = 25.0,
                RelativeHumidity = 0.55,
                WindSpeed = 2.0,
                SolarRadiation = 300.0,
                AtmosphericPressure = 101.325
            };

            double E0 = CalculatePenmanEvaporation(env);
            double qBase = 0.01;

            var prediction = new MigrationPrediction
            {
                PredictionType = "SaltMigration",
                PredictionHours = totalHours,
                PredictionTime = DateTime.Now,
                MaxConcentration = 0,
                AverageConcentration = 0,
                SurfaceEvaporationRate = Math.Round(E0, 6)
            };

            double[] D = new double[nz];
            double[] q = new double[nz];

            for (int t = 0; t < nt; t++)
            {
                double currentTimeHours = (t + 1) * dt;
                double diurnalFactor = 0.4 + 0.6 * Math.Max(0, Math.Sin((currentTimeHours % 24 - 6) * Math.PI / 12));
                double Et = E0 * diurnalFactor;

                for (int i = 0; i < nz; i++)
                {
                    D[i] = CalculateDiffusionCoefficient(theta[i]);
                    double depthFactor = Math.Exp(-z[i] / 15.0);
                    q[i] = qBase - Et * depthFactor;
                }

                c = ApplyBoundaryConditionsWithEvaporation(c, theta, Et, dz, initialConcentration, 0);
                theta = UpdateMoistureWithEvaporation(theta, Et, dt, dz, nz);
                c = SolveFDMWithEvaporation(c, theta, D, q, Et, dt, dz, nz);

                for (int i = 0; i < nz; i++)
                {
                    if (c[i] < 0) c[i] = 0;
                    if (double.IsNaN(c[i]) || double.IsInfinity(c[i])) c[i] = initialConcentration;
                    theta[i] = Math.Clamp(theta[i], 0.02, 0.55);
                }
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
                    MoistureContent = Math.Round(theta[i], 4),
                    TimeStep = nt * dt,
                    Timestamp = DateTime.Now.AddHours(nt * dt)
                });
            }

            prediction.MaxConcentration = Math.Round(maxC, 4);
            prediction.AverageConcentration = Math.Round(sumC / nz, 4);
            prediction.SurfaceEnrichmentRatio = Math.Round(c[0] / (sumC / nz + 1e-9), 4);

            return prediction;
        }

        public double CalculatePenmanEvaporation(EvaporationEnvironment env)
        {
            double T = env.Temperature;
            double RH = env.RelativeHumidity;
            double u2 = env.WindSpeed;
            double Rn = env.SolarRadiation;
            double P = env.AtmosphericPressure;

            double es = 0.6108 * Math.Exp(17.27 * T / (T + 237.3));
            double ea = es * RH;
            double delta = 4098 * es / Math.Pow(T + 237.3, 2);
            double gamma = 0.000665 * P;
            double G = 0.1 * Rn;

            double radiativeTerm = delta * (Rn - G) / (delta + gamma);
            double aerodynamicTerm = gamma * (900.0 / (T + 273.0)) * u2 * (es - ea) / (delta + gamma);

            double LE_mmPerHour = (radiativeTerm + aerodynamicTerm) / 2450.0 / 3600.0 * 1e6;
            double E_cmPerHour = LE_mmPerHour * 0.1;

            return Math.Max(0, E_cmPerHour);
        }

        public double[] ApplyBoundaryConditionsWithEvaporation(double[] concentration, double[] theta, 
            double E, double dz, double bottomConcentration, double bottomGradient)
        {
            int n = concentration.Length;
            if (n < 2) return concentration;

            double surfaceWaterLoss = E;
            if (theta[0] > 0.05)
            {
                double enrichmentFactor = 1.0 + surfaceWaterLoss * 2.5;
                concentration[0] = concentration[1] * enrichmentFactor;
            }
            else
            {
                concentration[0] = concentration[1];
            }

            if (concentration[0] < 0) concentration[0] = 0;
            concentration[n - 1] = bottomConcentration;

            return concentration;
        }

        public double[] UpdateMoistureWithEvaporation(double[] theta, double E, double dt, double dz, int n)
        {
            double[] thetaNew = new double[n];
            Array.Copy(theta, thetaNew, n);

            double surfaceLoss = Math.Min(E * dt / dz, thetaNew[0] - 0.03);
            thetaNew[0] -= Math.Max(0, surfaceLoss);

            for (int i = 1; i < Math.Min(5, n); i++)
            {
                double capillaryFactor = Math.Exp(-i * 0.8);
                double upwardFlow = 0.005 * capillaryFactor * dt / dz;
                thetaNew[i] -= upwardFlow;
                thetaNew[i - 1] += upwardFlow * 0.5;
            }

            for (int i = 0; i < n; i++)
            {
                thetaNew[i] = Math.Clamp(thetaNew[i], 0.02, 0.55);
            }

            return thetaNew;
        }

        public double[] SolveFDMWithEvaporation(double[] concentration, double[] theta, 
            double[] D, double[] q, double E, double dt, double dz, int n)
        {
            double[] cNew = new double[n];
            Array.Copy(concentration, cNew, n);

            for (int i = 1; i < n - 1; i++)
            {
                double thetaMean = 0.5 * (theta[i + 1] + theta[i - 1]);
                if (thetaMean < 1e-6) thetaMean = 0.01;

                double DMean = 0.5 * (D[i] + D[i + 1]);
                double r = DMean * dt / (dz * dz);
                double s = q[i] * dt / (2.0 * dz * thetaMean);

                cNew[i] = concentration[i]
                    + r * (concentration[i + 1] - 2.0 * concentration[i] + concentration[i - 1])
                    - s * (concentration[i + 1] - concentration[i - 1]);
            }

            if (n >= 2)
            {
                double evapConc = E * dt / (dz * Math.Max(theta[0], 0.05));
                cNew[0] = concentration[0] + evapConc * (concentration[1] + 0.001);
                if (cNew[0] < 0) cNew[0] = 0;
                cNew[n - 1] = concentration[n - 1];
            }

            return cNew;
        }

        public double CalculateDiffusionCoefficient(double theta)
        {
            double D0 = 1.5e-9;
            double tau = 0.6;
            return D0 * tau * Math.Max(theta, 0.01) * 3600.0 * 10000.0;
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

    public class EvaporationEnvironment
    {
        public double Temperature { get; set; }
        public double RelativeHumidity { get; set; }
        public double WindSpeed { get; set; }
        public double SolarRadiation { get; set; }
        public double AtmosphericPressure { get; set; }
    }
}
