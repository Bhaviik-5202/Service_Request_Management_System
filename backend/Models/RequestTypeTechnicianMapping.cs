using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("RequestTypeTechnicianMappings")]
    public partial class RequestTypeTechnicianMapping : IHasCreatedAt, IHasUpdatedAt
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
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(RequestTypeId))]
        [JsonIgnore]
        public virtual RequestType RequestType { get; set; } = null!;

        [ForeignKey(nameof(DepartmentPersonnelId))]
        [JsonIgnore]
        public virtual DepartmentPersonnel DepartmentPersonnel { get; set; } = null!;
    }
}
