using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        public int SculptureId { get; set; }

        [ForeignKey(nameof(SculptureId))]
        public virtual Sculpture Sculpture { get; set; }

        [Required]
        [MaxLength(50)]
        public string Severity { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }

        [MaxLength(100)]
        public string AlertType { get; set; }

        public double? ThresholdValue { get; set; }

        public double? ActualValue { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? ResolvedAt { get; set; }

        [MaxLength(500)]
        public string ResolvedNote { get; set; }

        public bool? DingTalkNotified { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime? DingTalkNotifiedAt { get; set; }
    }
}
