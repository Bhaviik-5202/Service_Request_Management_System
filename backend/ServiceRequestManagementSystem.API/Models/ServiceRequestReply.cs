namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceRequestReply
    {
        public int ReplyId { get; set; }

        public int RequestId { get; set; }

        public int AuthorUserId { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? StatusTransitionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ServiceRequest? ServiceRequest { get; set; }

        public User? Author { get; set; }

        public ServiceRequestStatus? StatusTransition { get; set; }

        public ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();
    }
}
