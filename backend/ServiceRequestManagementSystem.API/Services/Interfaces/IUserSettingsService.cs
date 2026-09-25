using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IUserSettingsService
    {
        Task<ApiResponseDto<UserSettingsDto>> GetSettingsAsync(int userId);
        Task<ApiResponseDto<UserSettingsDto>> UpdateSettingsAsync(int userId, UserSettingsDto dto);
    }
}
