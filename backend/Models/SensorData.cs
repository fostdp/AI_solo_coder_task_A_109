using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class SensorData
    {
        [Key]
        public long Id { get; set; }

        public int SensorId { get; set; }

        [ForeignKey(nameof(SensorId))]
        public virtual Sensor Sensor { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public double? Temperature { get; set; }

        public double? Humidity { get; set; }

        public double? PH { get; set; }

        public double? SaltConcentration { get; set; }

        public double? SodiumIon { get; set; }

        public double? ChlorideIon { get; set; }

        public double? SulfateIon { get; set; }

        public double? NitrateIon { get; set; }

        public double? Conductivity { get; set; }

        public double? CrystalCoverage { get; set; }

        public double? WaterContent { get; set; }

        [MaxLength(500)]
        public string RawData { get; set; }
    }
}
