using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
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
        [Column(TypeName = "DATETIME2")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(RequestId))]
        [JsonIgnore]
        public virtual ServiceRequest ServiceRequest { get; set; } = null!;

        [ForeignKey(nameof(ReplyId))]
        [JsonIgnore]
        public virtual ServiceRequestReply? Reply { get; set; }

        [ForeignKey(nameof(UploadedByUserId))]
        [JsonIgnore]
        public virtual User UploadedByUser { get; set; } = null!;
    }
}
