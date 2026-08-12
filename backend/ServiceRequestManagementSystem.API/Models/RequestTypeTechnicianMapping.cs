using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("RequestTypeTechnicianMappings")]
    public class RequestTypeTechnicianMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MappingId { get; set; }

        [Required]
        public int RequestTypeId { get; set; }

        [Required]
        public int DepartmentPersonnelId { get; set; }

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
        [ForeignKey(nameof(RequestTypeId))]
        public virtual RequestType? RequestType { get; set; }

        [ForeignKey(nameof(DepartmentPersonnelId))]
        public virtual DepartmentPersonnel? DepartmentPersonnel { get; set; }
    }
}
