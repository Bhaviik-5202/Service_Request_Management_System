using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("ServiceRequestTimeline")]
    public class ServiceRequestTimeline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TimelineId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; } = string.Empty;

        [Required]
        public int ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        [ForeignKey(nameof(RequestId))]
        public virtual ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey(nameof(ChangedByUserId))]
        public virtual User? ChangedBy { get; set; }
    }
}
