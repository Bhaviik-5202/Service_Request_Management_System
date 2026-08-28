using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
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
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public int? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RequestId))]
        public virtual ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey(nameof(DecidedByUserId))]
        public virtual User? DecidedBy { get; set; }
    }
}
