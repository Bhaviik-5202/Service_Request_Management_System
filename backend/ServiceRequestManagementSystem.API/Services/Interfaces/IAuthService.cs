using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponseDto<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress);
        Task<ApiResponseDto<AuthResponse>> RegisterAsync(RegisterRequestDto dto, string? ipAddress);
        Task<ApiResponseDto<object>> MeAsync(int userId);
        Task<ApiResponseDto<object>> ChangePasswordAsync(int userId, ChangePasswordDto dto, string? ipAddress);
    }
}
