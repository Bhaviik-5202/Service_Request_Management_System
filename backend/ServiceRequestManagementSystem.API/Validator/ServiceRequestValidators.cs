using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class CreateServiceRequestDtoValidator : AbstractValidator<CreateServiceRequestDto>
    {
        public CreateServiceRequestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Request title is required.")
                .Length(5, 150).WithMessage("Title must be between 5 and 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MinimumLength(20).WithMessage("Description must be at least 20 characters long.");

            RuleFor(x => x.ServiceTypeId)
                .GreaterThan(0).WithMessage("Valid service type selection is required.");

            RuleFor(x => x.RequestTypeId)
                .GreaterThan(0).WithMessage("Valid request type selection is required.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Valid department selection is required.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Valid priority level is required.");
        }
    }

    public class UpdateServiceRequestStatusDtoValidator : AbstractValidator<UpdateServiceRequestStatusDto>
    {
        public UpdateServiceRequestStatusDtoValidator()
        {
            RuleFor(x => x.StatusId)
                .GreaterThan(0).WithMessage("Valid status ID is required.");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.");
        }
    }

    public class AssignTechnicianDtoValidator : AbstractValidator<AssignTechnicianDto>
    {
        public AssignTechnicianDtoValidator()
        {
            RuleFor(x => x.AssigneeUserId)
                .GreaterThan(0).WithMessage("Valid assignee user ID is required.");
        }
    }

    public class CreateReplyDtoValidator : AbstractValidator<CreateReplyDto>
    {
        public CreateReplyDtoValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Reply message content cannot be empty.");
        }
    }
}
