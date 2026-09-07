using FluentValidation;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;

namespace ServiceRequestManagementSystem.API.Validator
{
    public class UserSettingsDtoValidator : AbstractValidator<UserSettingsDto>
    {
        public UserSettingsDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("Valid user ID is required.");

            RuleFor(x => x.Theme)
                .NotEmpty()
                .WithMessage("Theme is required.");

            RuleFor(x => x.Theme)
                .Must(x => x == "light" || x == "dark")
                .WithMessage("Theme must be light or dark.");
        }
    }
}
