namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceRequestAttachment
    {
        public int AttachmentId { get; set; }

        public int RequestId { get; set; }

        public int? ReplyId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int FileSizeKB { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public int UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ServiceRequest? ServiceRequest { get; set; }

        public ServiceRequestReply? Reply { get; set; }

        public User? UploadedBy { get; set; }
    }
}
