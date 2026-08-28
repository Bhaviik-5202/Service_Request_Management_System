using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    public class Asset
    {
        public int AssetId { get; set; }

        public string AssetTag { get; set; } = string.Empty;

        public string AssetName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public int? AssignedToUserId { get; set; }

        public int? DepartmentId { get; set; }

        public AssetStatus Status { get; set; } = AssetStatus.Available;

        public DateTime PurchaseDate { get; set; }

        public DateTime WarrantyUntil { get; set; }

        public decimal BookValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual User? AssignedTo { get; set; }

        public virtual Department? Department { get; set; }
    }
}
