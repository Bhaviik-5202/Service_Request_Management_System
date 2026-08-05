using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("ServiceTypes")]
    public partial class ServiceType : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServiceTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ServiceTypeCode { get; set; } = string.Empty;

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
        [JsonIgnore]
        public virtual ICollection<RequestType> RequestTypes { get; set; } = new List<RequestType>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
