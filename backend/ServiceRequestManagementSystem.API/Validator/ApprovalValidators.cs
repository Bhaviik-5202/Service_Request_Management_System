using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class ApprovalDecisionDtoValidator : AbstractValidator<ApprovalDecisionDto>
    {
        public ApprovalDecisionDtoValidator()
        {
            RuleFor(x => x.Decision)
                .Must(decision =>
                    decision == ApprovalStatus.Approved ||
                    decision == ApprovalStatus.Rejected)
                .WithMessage("Decision must be Approved or Rejected.");

            RuleFor(x => x.Remarks)
                .MaximumLength(1000)
                .WithMessage("Remarks cannot exceed 1000 characters.");
        }
    }
}
