using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class MaterialScore
    {
        [Key]
        public int Id { get; set; }

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public virtual Material Material { get; set; }

        public int SculptureId { get; set; }

        [ForeignKey(nameof(SculptureId))]
        public virtual Sculpture Sculpture { get; set; }

        public double ContactAngleScore { get; set; }

        public double PenetrationDepthScore { get; set; }

        public double StrengthMatchingScore { get; set; }

        public double WeatherResistanceScore { get; set; }

        public double ReversibilityScore { get; set; }

        public double CostPerformanceScore { get; set; }

        public double TotalScore { get; set; }

        [MaxLength(500)]
        public string Recommendation { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
