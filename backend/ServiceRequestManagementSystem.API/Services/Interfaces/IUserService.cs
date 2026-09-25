using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponseDto<IEnumerable<UserResponseDto>>> GetUsersAsync(
            string? search, string? role, int? departmentId);

        Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(int id);

        Task<ApiResponseDto<UserResponseDto>> CreateUserAsync(
            CreateUserDto dto, string? ipAddress);

        Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(
            int id, UpdateUserDto dto, string? ipAddress);

        Task<ApiResponseDto<object>> DeleteUserAsync(int id, string? ipAddress);
    }
}
