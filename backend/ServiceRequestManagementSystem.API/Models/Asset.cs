using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("Assets")]
    public class Asset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssetId { get; set; }

        [Required]
        [MaxLength(30)]
        public string AssetTag { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AssetName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        public int? AssignedToUserId { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime WarrantyUntil { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BookValue { get; set; } = 0.00m;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AssignedToUserId))]
        public virtual User? AssignedTo { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }
    }
}
