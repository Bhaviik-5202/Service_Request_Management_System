using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.Masters;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class DepartmentDtoValidator : AbstractValidator<DepartmentDto>
    {
        public DepartmentDtoValidator()
        {
            RuleFor(x => x.DepartmentName)
                .NotEmpty()
                .WithMessage("Department name is required.")
                .MaximumLength(50)
                .WithMessage("Department name cannot exceed 50 characters.");

            RuleFor(x => x.DepartmentCode)
                .NotEmpty()
                .WithMessage("Department code is required.")
                .MaximumLength(10)
                .WithMessage("Department code cannot exceed 10 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");
        }
    }

    public class ServiceTypeDtoValidator : AbstractValidator<ServiceTypeDto>
    {
        public ServiceTypeDtoValidator()
        {
            RuleFor(x => x.ServiceTypeName)
                .NotEmpty()
                .WithMessage("Service type name is required.")
                .MaximumLength(50)
                .WithMessage("Service type name cannot exceed 50 characters.");

            RuleFor(x => x.ServiceTypeCode)
                .NotEmpty()
                .WithMessage("Service type code is required.")
                .MaximumLength(10)
                .WithMessage("Service type code cannot exceed 10 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");
        }
    }

    public class RequestTypeDtoValidator : AbstractValidator<RequestTypeDto>
    {
        public RequestTypeDtoValidator()
        {
            RuleFor(x => x.ServiceTypeId)
                .GreaterThan(0)
                .WithMessage("Valid service type ID is required.");

            RuleFor(x => x.RequestTypeName)
                .NotEmpty()
                .WithMessage("Request type name is required.")
                .MaximumLength(100)
                .WithMessage("Request type name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");
        }
    }

    public class StatusDtoValidator : AbstractValidator<StatusDto>
    {
        public StatusDtoValidator()
        {
            RuleFor(x => x.StatusName)
                .NotEmpty()
                .WithMessage("Status name is required.")
                .MaximumLength(50)
                .WithMessage("Status name cannot exceed 50 characters.");

            RuleFor(x => x.ColorCode)
                .MaximumLength(100)
                .WithMessage("Color code cannot exceed 100 characters.");
        }
    }

    public class DepartmentPersonnelDtoValidator : AbstractValidator<DepartmentPersonnelDto>
    {
        public DepartmentPersonnelDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("Valid user ID is required.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Valid department ID is required.");
        }
    }

    public class RequestTypeTechnicianMappingDtoValidator : AbstractValidator<RequestTypeTechnicianMappingDto>
    {
        public RequestTypeTechnicianMappingDtoValidator()
        {
            RuleFor(x => x.RequestTypeId)
                .GreaterThan(0)
                .WithMessage("Valid request type ID is required.");

            RuleFor(x => x.DepartmentPersonnelId)
                .GreaterThan(0)
                .WithMessage("Valid department personnel ID is required.");
        }
    }
}
