using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Requestor;

        public int? DepartmentId { get; set; }

        public string? Phone { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Department? Department { get; set; }

        public UserSettings? UserSettings { get; set; }

        public ICollection<DepartmentPersonnel> DepartmentPersonnels { get; set; } = new List<DepartmentPersonnel>();

        public ICollection<ServiceRequest> RequestedRequests { get; set; } = new List<ServiceRequest>();

        public ICollection<ServiceRequest> AssignedRequests { get; set; } = new List<ServiceRequest>();

        public ICollection<ServiceRequestReply> Replies { get; set; } = new List<ServiceRequestReply>();

        public ICollection<ServiceRequestTimeline> TimelineEntries { get; set; } = new List<ServiceRequestTimeline>();

        public ICollection<ServiceRequestAttachment> UploadedAttachments { get; set; } = new List<ServiceRequestAttachment>();

        public ICollection<Approval> DecidedApprovals { get; set; } = new List<Approval>();

        public ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
