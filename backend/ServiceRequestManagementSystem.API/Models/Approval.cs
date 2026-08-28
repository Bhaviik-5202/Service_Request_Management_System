using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Models
{
    public class Approval
    {
        public int ApprovalId { get; set; }

        public int RequestId { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public int? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string? Remarks { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public virtual ServiceRequest? ServiceRequest { get; set; }

        public virtual User? DecidedBy { get; set; }
    }
}
