using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
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
        public int StatusId { get; set; }

        [Required]
        public int ChangedByUserId { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey(nameof(RequestId))]
        [JsonIgnore]
        public virtual ServiceRequest ServiceRequest { get; set; } = null!;

        [ForeignKey(nameof(StatusId))]
        [JsonIgnore]
        public virtual ServiceRequestStatus Status { get; set; } = null!;

        [ForeignKey(nameof(ChangedByUserId))]
        [JsonIgnore]
        public virtual User ChangedByUser { get; set; } = null!;
    }
}
