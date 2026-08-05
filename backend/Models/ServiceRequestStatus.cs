using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("ServiceRequestStatuses")]
    public class ServiceRequestStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ColorCode { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestReply> ServiceRequestReplies { get; set; } = new List<ServiceRequestReply>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestTimeline> ServiceRequestTimelines { get; set; } = new List<ServiceRequestTimeline>();
    }
}
