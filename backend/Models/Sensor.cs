using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class Sensor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SensorCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string SensorType { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public double? InstallationDepth { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public int SculptureId { get; set; }

        [ForeignKey(nameof(SculptureId))]
        public virtual Sculpture Sculpture { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? LastCalibration { get; set; }

        public virtual ICollection<SensorData> SensorData { get; set; } = new List<SensorData>();
    }
}
