using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.DTOs.Approvals
{
    public class ApprovalResponseDto
    {
        public int ApprovalId { get; set; }

        public int RequestId { get; set; }

        public string RequestNo { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Requester { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public Priority Priority { get; set; }

        public DateTime SubmittedAt { get; set; }

        public ApprovalStatus Status { get; set; }

        public string? DecidedBy { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string? Remarks { get; set; }
    }

    public class ApprovalDecisionDto
    {
        public int DecidedByUserId { get; set; }

        public ApprovalStatus Decision { get; set; }

        public string? Remarks { get; set; }
    }
}
