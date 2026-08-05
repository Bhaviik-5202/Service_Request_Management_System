using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Departments")]
    public partial class Department : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string DepartmentCode { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

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

        [Column(TypeName = "DATETIME2")]
        public DateTime? DeletedAt { get; set; }

        public int? DeletedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(DeletedByUserId))]
        [JsonIgnore]
        public virtual User? DeletedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        [JsonIgnore]
        public virtual ICollection<DepartmentPersonnel> DepartmentPersonnel { get; set; } = new List<DepartmentPersonnel>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        [JsonIgnore]
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
