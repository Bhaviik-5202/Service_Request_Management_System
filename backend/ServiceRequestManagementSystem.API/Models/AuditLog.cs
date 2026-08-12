using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditLogId { get; set; }

        public int? ActorUserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TargetType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TargetId { get; set; }

        [MaxLength(100)]
        public string? TargetDisplay { get; set; }

        public string? Detail { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey(nameof(ActorUserId))]
        public virtual User? Actor { get; set; }
    }
}
