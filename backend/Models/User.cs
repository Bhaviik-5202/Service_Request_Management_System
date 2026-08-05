using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Users")]
    public partial class User : IHasCreatedAt, IHasUpdatedAt
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
        public int RoleId { get; set; }

        public int? DepartmentId { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(15)]
        public string Status { get; set; } = "Active";

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow.Date;

        [Column(TypeName = "DATETIME2")]
        public DateTime? LastLoginAt { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Column(TypeName = "DATETIME2")]
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department? Department { get; set; }

        [JsonIgnore]
        public virtual UserSetting? UserSetting { get; set; }

        [JsonIgnore]
        public virtual ICollection<DepartmentPersonnel> DepartmentPersonnel { get; set; } = new List<DepartmentPersonnel>();

        // Service Request Relationships
        [InverseProperty(nameof(ServiceRequest.RequesterUser))]
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> RequestedServiceRequests { get; set; } = new List<ServiceRequest>();

        [InverseProperty(nameof(ServiceRequest.AssigneeUser))]
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> AssignedServiceRequests { get; set; } = new List<ServiceRequest>();

        [InverseProperty(nameof(ServiceRequest.CreatedByUser))]
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> CreatedServiceRequests { get; set; } = new List<ServiceRequest>();

        [InverseProperty(nameof(ServiceRequest.UpdatedByUser))]
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> UpdatedServiceRequests { get; set; } = new List<ServiceRequest>();

        [InverseProperty(nameof(ServiceRequest.DeletedByUser))]
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> DeletedServiceRequests { get; set; } = new List<ServiceRequest>();

        [JsonIgnore]
        public virtual ICollection<ServiceRequestReply> ServiceRequestReplies { get; set; } = new List<ServiceRequestReply>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestTimeline> ServiceRequestTimelines { get; set; } = new List<ServiceRequestTimeline>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestAttachment> ServiceRequestAttachments { get; set; } = new List<ServiceRequestAttachment>();

        [JsonIgnore]
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

        // Asset Relationships
        [InverseProperty(nameof(Asset.AssignedToUser))]
        [JsonIgnore]
        public virtual ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();

        [JsonIgnore]
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        [JsonIgnore]
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Deleted By relationships
        [InverseProperty(nameof(Department.DeletedByUser))]
        [JsonIgnore]
        public virtual ICollection<Department> DeletedDepartments { get; set; } = new List<Department>();
    }
}
