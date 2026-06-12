using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class MigrationPredictionPoint
    {
        [Key]
        public int Id { get; set; }

        public int MigrationPredictionId { get; set; }

        [ForeignKey(nameof(MigrationPredictionId))]
        public virtual MigrationPrediction MigrationPrediction { get; set; }

        public double Depth { get; set; }

        public double Concentration { get; set; }

        public double? MoistureContent { get; set; }

        public double? TimeStep { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Timestamp { get; set; }
    }
}
