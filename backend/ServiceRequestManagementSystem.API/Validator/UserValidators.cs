using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.Users;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.")
                .MaximumLength(20).WithMessage("Employee ID cannot exceed 20 characters.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .Length(2, 50).WithMessage("Full name must be between 2 and 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(256).WithMessage("Email address cannot exceed 256 characters.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("A valid user role must be specified.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
        }
    }

    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("A valid user role must be specified.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("A valid user status must be specified.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
        }
    }
}
