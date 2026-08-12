using System.ComponentModel.DataAnnotations;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.DTOs.Assets
{
    public class AssetResponseDto
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? Department { get; set; }
        public int? DepartmentId { get; set; }
        public AssetStatus Status { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime WarrantyUntil { get; set; }
        public decimal BookValue { get; set; }
    }

    public class CreateAssetDto
    {
        [Required]
        [StringLength(30)]
        public string AssetTag { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AssetName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public int? AssignedToUserId { get; set; }

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime WarrantyUntil { get; set; }

        [Required]
        [Range(0, 999999999)]
        public decimal BookValue { get; set; }
    }

    public class UpdateAssetDto
    {
        [Required]
        [StringLength(100)]
        public string AssetName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public int? AssignedToUserId { get; set; }

        [Required]
        public AssetStatus Status { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime WarrantyUntil { get; set; }

        [Required]
        public decimal BookValue { get; set; }
    }

    public class AssignAssetDto
    {
        public int? AssignedToUserId { get; set; }
    }
}
