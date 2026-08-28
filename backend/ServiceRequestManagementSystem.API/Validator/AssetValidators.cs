using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.Assets;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class CreateAssetDtoValidator : AbstractValidator<CreateAssetDto>
    {
        public CreateAssetDtoValidator()
        {
            RuleFor(x => x.AssetTag)
                .NotEmpty()
                .WithMessage("Asset tag is required.")
                .MaximumLength(30)
                .WithMessage("Asset tag cannot exceed 30 characters.");

            RuleFor(x => x.AssetName)
                .NotEmpty()
                .WithMessage("Asset name is required.")
                .MaximumLength(100)
                .WithMessage("Asset name cannot exceed 100 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Asset category is required.")
                .MaximumLength(50)
                .WithMessage("Asset category cannot exceed 50 characters.");

            RuleFor(x => x.SerialNumber)
                .NotEmpty()
                .WithMessage("Serial number is required.")
                .MaximumLength(15)
                .WithMessage("Serial number cannot exceed 15 characters.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .When(x => x.DepartmentId.HasValue)
                .WithMessage("Department ID must be greater than 0.");

            RuleFor(x => x.AssignedToUserId)
                .GreaterThan(0)
                .When(x => x.AssignedToUserId.HasValue)
                .WithMessage("Assigned user ID must be greater than 0.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Valid asset status must be specified.");

            RuleFor(x => x.PurchaseDate)
                .NotEmpty()
                .WithMessage("Purchase date is required.");

            RuleFor(x => x.WarrantyUntil)
                .NotEmpty()
                .WithMessage("Warranty end date is required.")
                .GreaterThanOrEqualTo(x => x.PurchaseDate)
                .WithMessage("Warranty end date must be on or after the purchase date.");

            RuleFor(x => x.BookValue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Book value cannot be negative.");
        }
    }

    public class UpdateAssetDtoValidator : AbstractValidator<UpdateAssetDto>
    {
        public UpdateAssetDtoValidator()
        {
            RuleFor(x => x.AssetName)
                .NotEmpty()
                .WithMessage("Asset name is required.")
                .MaximumLength(100)
                .WithMessage("Asset name cannot exceed 100 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Asset category is required.")
                .MaximumLength(50)
                .WithMessage("Asset category cannot exceed 50 characters.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .When(x => x.DepartmentId.HasValue)
                .WithMessage("Department ID must be greater than 0.");

            RuleFor(x => x.AssignedToUserId)
                .GreaterThan(0)
                .When(x => x.AssignedToUserId.HasValue)
                .WithMessage("Assigned user ID must be greater than 0.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Valid asset status must be specified.");

            RuleFor(x => x.PurchaseDate)
                .NotEmpty()
                .WithMessage("Purchase date is required.");

            RuleFor(x => x.WarrantyUntil)
                .NotEmpty()
                .WithMessage("Warranty end date is required.")
                .GreaterThanOrEqualTo(x => x.PurchaseDate)
                .WithMessage("Warranty end date must be on or after the purchase date.");

            RuleFor(x => x.BookValue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Book value cannot be negative.");
        }
    }
}
