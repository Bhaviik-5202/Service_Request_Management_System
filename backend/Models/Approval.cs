using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Approvals")]
    public class Approval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApprovalId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(15)]
        public string Status { get; set; } = "Pending";

        public int? DecidedByUserId { get; set; }

        [Column(TypeName = "DATETIME2")]
        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(RequestId))]
        [JsonIgnore]
        public virtual ServiceRequest ServiceRequest { get; set; } = null!;

        [ForeignKey(nameof(DecidedByUserId))]
        [JsonIgnore]
        public virtual User? DecidedByUser { get; set; }
    }
}
