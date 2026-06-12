using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class MigrationPrediction
    {
        [Key]
        public int Id { get; set; }

        public int SculptureId { get; set; }

        [ForeignKey(nameof(SculptureId))]
        public virtual Sculpture Sculpture { get; set; }

        [MaxLength(100)]
        public string PredictionType { get; set; }

        public int PredictionHours { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime PredictionTime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public double? MaxConcentration { get; set; }

        public double? AverageConcentration { get; set; }

        public double? SurfaceEvaporationRate { get; set; }

        public double? SurfaceEnrichmentRatio { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        public virtual ICollection<MigrationPredictionPoint> PredictionPoints { get; set; } = new List<MigrationPredictionPoint>();
    }
}
