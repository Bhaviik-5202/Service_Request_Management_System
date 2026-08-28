using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.Approvals;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class ApprovalDecisionDtoValidator : AbstractValidator<ApprovalDecisionDto>
    {
        public ApprovalDecisionDtoValidator()
        {
            RuleFor(x => x.Decision)
                .IsInEnum().WithMessage("Valid approval decision (Approved or Rejected) is required.");

            RuleFor(x => x.Remarks)
                .MaximumLength(1000).WithMessage("Remarks cannot exceed 1000 characters.");
        }
    }
}
