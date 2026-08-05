using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("ServiceRequestReplies")]
    public class ServiceRequestReply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReplyId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int AuthorUserId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public int? StatusTransitionId { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(RequestId))]
        [JsonIgnore]
        public virtual ServiceRequest ServiceRequest { get; set; } = null!;

        [ForeignKey(nameof(AuthorUserId))]
        [JsonIgnore]
        public virtual User AuthorUser { get; set; } = null!;

        [ForeignKey(nameof(StatusTransitionId))]
        [JsonIgnore]
        public virtual ServiceRequestStatus? StatusTransition { get; set; }

        [JsonIgnore]
        public virtual ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();
    }
}
