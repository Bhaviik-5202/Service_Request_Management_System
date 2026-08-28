using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Requestor;

        public int? DepartmentId { get; set; }

        public string? Phone { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Relationships

        public virtual Department? Department { get; set; }

        public virtual UserSettings? UserSettings { get; set; }

        public virtual ICollection<DepartmentPersonnel> DepartmentPersonnels { get; set; }
            = new List<DepartmentPersonnel>();

        public virtual ICollection<ServiceRequest> RequestedRequests { get; set; }
            = new List<ServiceRequest>();

        public virtual ICollection<ServiceRequest> AssignedRequests { get; set; }
            = new List<ServiceRequest>();

        public virtual ICollection<ServiceRequestReply> Replies { get; set; }
            = new List<ServiceRequestReply>();

        public virtual ICollection<ServiceRequestTimeline> TimelineEntries { get; set; }
            = new List<ServiceRequestTimeline>();

        public virtual ICollection<ServiceRequestAttachment> UploadedAttachments { get; set; }
            = new List<ServiceRequestAttachment>();

        public virtual ICollection<Approval> DecidedApprovals { get; set; }
            = new List<Approval>();

        public virtual ICollection<Asset> AssignedAssets { get; set; }
            = new List<Asset>();

        public virtual ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();
    }
}
