using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponseDto<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(int userId);
        Task<ApiResponseDto<int>> GetUnreadCountAsync(int userId);
        Task<ApiResponseDto<bool>> MarkAsReadAsync(int id, int userId);
        Task<ApiResponseDto<bool>> MarkAllAsReadAsync(int userId);
    }
}
