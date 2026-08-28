using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RequestId))]
        public virtual ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey(nameof(AuthorUserId))]
        public virtual User? Author { get; set; }

        [ForeignKey(nameof(StatusTransitionId))]
        public virtual ServiceRequestStatus? StatusTransition { get; set; }

        public virtual ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();
    }
}
