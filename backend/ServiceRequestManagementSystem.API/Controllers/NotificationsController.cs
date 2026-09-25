using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<NotificationResponseDto>>>> GetNotifications([FromQuery] int userId)
        {
            var result = await _notificationService.GetNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount([FromQuery] int userId)
        {
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(int id, [FromQuery] int userId)
        {
            var result = await _notificationService.MarkAsReadAsync(id, userId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead([FromQuery] int userId)
        {
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(result);
        }
    }
}
