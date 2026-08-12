using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("RequestTypes")]
    public class RequestType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestTypeId { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequestTypeName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        public bool RequiresApproval { get; set; } = false;

        [Required]
        public bool IsActive { get; set; } = true;

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

        public virtual ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = new List<RequestTypeTechnicianMapping>();

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
