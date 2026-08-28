using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("ServiceRequestAttachments")]
    public class ServiceRequestAttachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttachmentId { get; set; }

        [Required]
        public int RequestId { get; set; }

        public int? ReplyId { get; set; }

        [Required]
        [MaxLength(256)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public int FileSizeKB { get; set; }

        [Required]
        [MaxLength(2048)]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public int UploadedByUserId { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RequestId))]
        public virtual ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey(nameof(ReplyId))]
        public virtual ServiceRequestReply? Reply { get; set; }

        [ForeignKey(nameof(UploadedByUserId))]
        public virtual User? UploadedBy { get; set; }
    }
}
