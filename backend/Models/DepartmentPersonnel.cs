using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("DepartmentPersonnel")]
    public partial class DepartmentPersonnel : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentPersonnelId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public bool IsHOD { get; set; } = false;

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
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department Department { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = new List<RequestTypeTechnicianMapping>();
    }
}
