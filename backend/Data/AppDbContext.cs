using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Models;

namespace SculptureMonitor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Sculpture> Sculptures { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialScore> MaterialScores { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AlertThreshold> AlertThresholds { get; set; }
        public DbSet<DingTalkConfig> DingTalkConfigs { get; set; }
        public DbSet<MigrationPrediction> MigrationPredictions { get; set; }
        public DbSet<MigrationPredictionPoint> MigrationPredictionPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sculpture>()
                .HasMany(s => s.Sensors)
                .WithOne(s => s.Sculpture)
                .HasForeignKey(s => s.SculptureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sculpture>()
                .HasMany(s => s.Alerts)
                .WithOne(a => a.Sculpture)
                .HasForeignKey(a => a.SculptureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sensor>()
                .HasMany(s => s.SensorData)
                .WithOne(sd => sd.Sensor)
                .HasForeignKey(sd => sd.SensorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Material>()
                .HasMany(m => m.MaterialScores)
                .WithOne(ms => ms.Material)
                .HasForeignKey(ms => ms.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MigrationPrediction>()
                .HasMany(mp => mp.PredictionPoints)
                .WithOne(pp => pp.MigrationPrediction)
                .HasForeignKey(pp => pp.MigrationPredictionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SensorData>()
                .HasIndex(sd => sd.Timestamp);

            modelBuilder.Entity<SensorData>()
                .HasIndex(sd => sd.SensorId);

            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.CreatedAt);

            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.Status);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AlertThreshold>()
                .HasIndex(at => at.ParameterName)
                .IsUnique();
        }
    }
}
