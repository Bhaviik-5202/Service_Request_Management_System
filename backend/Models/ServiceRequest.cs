using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("ServiceRequests")]
    public partial class ServiceRequest : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public int RequestTypeId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int RequesterUserId { get; set; }

        public int? AssigneeUserId { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Required]
        [MaxLength(15)]
        public string Priority { get; set; } = "Medium";

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int CreatedByUserId { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int UpdatedByUserId { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Column(TypeName = "DATETIME2")]
        public DateTime? DeletedAt { get; set; }

        public int? DeletedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ServiceTypeId))]
        [JsonIgnore]
        public virtual ServiceType ServiceType { get; set; } = null!;

        [ForeignKey(nameof(RequestTypeId))]
        [JsonIgnore]
        public virtual RequestType RequestType { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey(nameof(RequesterUserId))]
        [JsonIgnore]
        public virtual User RequesterUser { get; set; } = null!;

        [ForeignKey(nameof(AssigneeUserId))]
        [JsonIgnore]
        public virtual User? AssigneeUser { get; set; }

        [ForeignKey(nameof(StatusId))]
        [JsonIgnore]
        public virtual ServiceRequestStatus Status { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUserId))]
        [JsonIgnore]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey(nameof(UpdatedByUserId))]
        [JsonIgnore]
        public virtual User UpdatedByUser { get; set; } = null!;

        [ForeignKey(nameof(DeletedByUserId))]
        [JsonIgnore]
        public virtual User? DeletedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ServiceRequestReply> Replies { get; set; } = new List<ServiceRequestReply>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestTimeline> Timelines { get; set; } = new List<ServiceRequestTimeline>();
        [JsonIgnore]
        public virtual ICollection<ServiceRequestAttachment> Attachments { get; set; } = new List<ServiceRequestAttachment>();
        [JsonIgnore]
        public virtual Approval? Approval { get; set; }
    }
}
