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

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual ServiceType? ServiceType { get; set; }

        public virtual RequestType? RequestType { get; set; }

        public virtual Department? Department { get; set; }

        public virtual User? Requester { get; set; }

        public virtual User? Assignee { get; set; }

        public virtual ServiceRequestStatus? Status { get; set; }

        public virtual ICollection<ServiceRequestReply> Replies { get; set; }
            = new List<ServiceRequestReply>();

        public virtual ICollection<ServiceRequestTimeline> TimelineEntries { get; set; }
            = new List<ServiceRequestTimeline>();

        public virtual ICollection<ServiceRequestAttachment> Attachments { get; set; }
            = new List<ServiceRequestAttachment>();

        public virtual Approval? Approval { get; set; }
    }
}
