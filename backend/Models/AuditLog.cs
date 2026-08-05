using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
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
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(ActorUserId))]
        [JsonIgnore]
        public virtual User? ActorUser { get; set; }
    }
}
