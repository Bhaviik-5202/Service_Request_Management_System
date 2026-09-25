using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUnitOfWork _uow;

        public UserSettingsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponseDto<UserSettingsDto>> GetSettingsAsync(int userId)
        {
            var settings = await _uow.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
                return Fail<UserSettingsDto>("User settings not found.");

            return Ok("User settings fetched successfully.", MapToDto(settings));
        }

        public async Task<ApiResponseDto<UserSettingsDto>> UpdateSettingsAsync(int userId, UserSettingsDto dto)
        {
            dto.UserId = userId;

            var userExists = await _uow.Users.ExistsAsync(u => u.UserId == userId);
            if (!userExists)
                return Fail<UserSettingsDto>("User not found.");

            var settings = await _uow.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                await _uow.UserSettings.AddAsync(settings);
            }

            settings.Theme = dto.Theme.Trim().ToLowerInvariant();
            settings.TwoFactorEnabled = dto.TwoFactorEnabled;
            settings.NotifyRequestUpdates = dto.NotifyRequestUpdates;
            settings.NotifyApprovalAlerts = dto.NotifyApprovalAlerts;
            settings.NotifySLAWarnings = dto.NotifySLAWarnings;
            settings.NotifyAssetEvents = dto.NotifyAssetEvents;
            settings.NotifyEmailDigest = dto.NotifyEmailDigest;
            settings.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            return Ok("User settings updated successfully.", MapToDto(settings));
        }

        private static UserSettingsDto MapToDto(UserSettings s) => new()
        {
            UserId = s.UserId,
            Theme = s.Theme,
            TwoFactorEnabled = s.TwoFactorEnabled,
            NotifyRequestUpdates = s.NotifyRequestUpdates,
            NotifyApprovalAlerts = s.NotifyApprovalAlerts,
            NotifySLAWarnings = s.NotifySLAWarnings,
            NotifyAssetEvents = s.NotifyAssetEvents,
            NotifyEmailDigest = s.NotifyEmailDigest
        };

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
