using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponseDto<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(int userId)
        {
            var notifications = await _uow.Notifications.FindAsync(n => n.UserId == userId);
            var result = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponseDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    NotificationType = n.NotificationType,
                    CreatedAt = n.CreatedAt
                });

            return Ok("Notifications fetched successfully.", result);
        }

        public async Task<ApiResponseDto<int>> GetUnreadCountAsync(int userId)
        {
            var count = await _uow.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return Ok("Unread notification count fetched successfully.", count);
        }

        public async Task<ApiResponseDto<bool>> MarkAsReadAsync(int id, int userId)
        {
            var notification = await _uow.Notifications.FirstOrDefaultAsync(n =>
                n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return Fail<bool>("Notification not found.");

            notification.IsRead = true;
            _uow.Notifications.Update(notification);
            await _uow.SaveChangesAsync();

            return Ok("Notification marked as read.", true);
        }

        public async Task<ApiResponseDto<bool>> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var n in notifications)
            {
                n.IsRead = true;
                _uow.Notifications.Update(n);
            }
            await _uow.SaveChangesAsync();

            return Ok("All notifications marked as read.", true);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
