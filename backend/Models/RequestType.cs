using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("RequestTypes")]
    public partial class RequestType : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequestTypeName { get; set; } = string.Empty;

        [Required]
        public int ServiceTypeId { get; set; }

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

        // Navigation Properties
        [ForeignKey(nameof(ServiceTypeId))]
        [JsonIgnore]
        public virtual ServiceType ServiceType { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        [JsonIgnore]
        public virtual ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = new List<RequestTypeTechnicianMapping>();
    }
}
