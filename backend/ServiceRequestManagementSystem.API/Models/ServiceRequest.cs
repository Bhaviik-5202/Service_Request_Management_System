using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("ServiceRequests")]
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public int RequestTypeId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int RequesterUserId { get; set; }

        public int? AssigneeUserId { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ServiceTypeId))]
        public virtual ServiceType? ServiceType { get; set; }

        [ForeignKey(nameof(RequestTypeId))]
        public virtual RequestType? RequestType { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [ForeignKey(nameof(RequesterUserId))]
        public virtual User? Requester { get; set; }

        [ForeignKey(nameof(AssigneeUserId))]
        public virtual User? Assignee { get; set; }

        [ForeignKey(nameof(StatusId))]
        public virtual ServiceRequestStatus? Status { get; set; }

        public virtual ICollection<ServiceRequestReply> Replies { get; set; } = new List<ServiceRequestReply>();

        public virtual ICollection<ServiceRequestTimeline> TimelineEntries { get; set; } = new List<ServiceRequestTimeline>();

        public virtual ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();

        public virtual Approval? Approval { get; set; }
    }
}
