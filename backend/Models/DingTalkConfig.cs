using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SculptureMonitor.Models
{
    public class DingTalkConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string WebhookUrl { get; set; }

        [MaxLength(100)]
        public string Secret { get; set; }

        [MaxLength(100)]
        public string ConfigName { get; set; }

        public bool? IsEnabled { get; set; } = true;

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
