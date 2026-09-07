namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceRequestTimeline
    {
        public int TimelineId { get; set; }

        public int RequestId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string Note { get; set; } = string.Empty;

        public ServiceRequest? ServiceRequest { get; set; }

        public User? ChangedBy { get; set; }
    }
}
