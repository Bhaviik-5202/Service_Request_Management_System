using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Assets")]
    public partial class Asset : IHasCreatedAt, IHasUpdatedAt
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
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        public int? AssignedToUserId { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Available";

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime PurchaseDate { get; set; }

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime WarrantyUntil { get; set; }

        [Required]
        [Column(TypeName = "DECIMAL(18,2)")]
        public decimal BookValue { get; set; } = 0.00m;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        public virtual AssetCategory Category { get; set; } = null!;

        [ForeignKey(nameof(AssignedToUserId))]
        [JsonIgnore]
        public virtual User? AssignedToUser { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department? Department { get; set; }
    }
}
