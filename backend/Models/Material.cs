using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string Manufacturer { get; set; }

        public double? ContactAngle { get; set; }

        public double? PenetrationDepth { get; set; }

        public double? StrengthMatching { get; set; }

        public double? WeatherResistance { get; set; }

        public double? Reversibility { get; set; }

        public double? CostPerformance { get; set; }

        public double? Viscosity { get; set; }

        public double? SolidContent { get; set; }

        [MaxLength(200)]
        public string ApplicableSculptureType { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<MaterialScore> MaterialScores { get; set; } = new List<MaterialScore>();
    }
}
