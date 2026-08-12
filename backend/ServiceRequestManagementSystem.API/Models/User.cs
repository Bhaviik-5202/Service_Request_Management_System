using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Requestor;

        public int? DepartmentId { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [Required]
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        public virtual UserSettings? UserSettings { get; set; }

        public virtual ICollection<DepartmentPersonnel> DepartmentPersonnels { get; set; } = new List<DepartmentPersonnel>();

        [InverseProperty(nameof(ServiceRequest.Requester))]
        public virtual ICollection<ServiceRequest> RequestedRequests { get; set; } = new List<ServiceRequest>();

        [InverseProperty(nameof(ServiceRequest.Assignee))]
        public virtual ICollection<ServiceRequest> AssignedRequests { get; set; } = new List<ServiceRequest>();

        public virtual ICollection<ServiceRequestReply> Replies { get; set; } = new List<ServiceRequestReply>();

        public virtual ICollection<ServiceRequestTimeline> TimelineEntries { get; set; } = new List<ServiceRequestTimeline>();

        public virtual ICollection<ServiceRequestAttachment> UploadedAttachments { get; set; } = new List<ServiceRequestAttachment>();

        public virtual ICollection<Approval> DecidedApprovals { get; set; } = new List<Approval>();

        public virtual ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
