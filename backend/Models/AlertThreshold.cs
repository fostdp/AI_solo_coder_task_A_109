using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class AlertThreshold
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ParameterName { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; }

        public double WarningThreshold { get; set; }

        public double CriticalThreshold { get; set; }

        [MaxLength(50)]
        public string Severity { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool? IsEnabled { get; set; } = true;

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
