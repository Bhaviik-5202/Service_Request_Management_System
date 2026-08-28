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

        public virtual ServiceRequest? ServiceRequest { get; set; }

        public virtual User? ChangedBy { get; set; }
    }
}
