using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceRequest
    {
        public int RequestId { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ServiceTypeId { get; set; }

        public int RequestTypeId { get; set; }

        public int DepartmentId { get; set; }

        public int RequesterUserId { get; set; }

        public int? AssigneeUserId { get; set; }

        public int StatusId { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ServiceType? ServiceType { get; set; }

        public RequestType? RequestType { get; set; }

        public Department? Department { get; set; }

        public User? Requester { get; set; }

        public User? Assignee { get; set; }

        public ServiceRequestStatus? Status { get; set; }

        public Approval? Approval { get; set; }

        public ICollection<ServiceRequestReply> Replies { get; set; } = new List<ServiceRequestReply>();

        public ICollection<ServiceRequestTimeline> TimelineEntries { get; set; } = new List<ServiceRequestTimeline>();

        public ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();
    }
}
