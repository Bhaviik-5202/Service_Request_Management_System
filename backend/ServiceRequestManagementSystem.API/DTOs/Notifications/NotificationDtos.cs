using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.DTOs.Notifications
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
