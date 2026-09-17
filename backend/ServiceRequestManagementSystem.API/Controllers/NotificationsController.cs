using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<NotificationResponseDto>>>> GetNotifications(
            int userId)
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
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
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<NotificationResponseDto>>
            {
                Success = true,
                Message = "Notifications fetched successfully.",
                Data = notifications
            });
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount(
            int userId)
        {
            var count = await _context.Notifications
                .AsNoTracking()
                .CountAsync(n =>
                    n.UserId == userId &&
                    !n.IsRead);

            return Ok(new ApiResponseDto<int>
            {
                Success = true,
                Message = "Unread notification count fetched successfully.",
                Data = count
            });
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(
            int id,
            int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id &&
                    n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Notification not found.",
                    Data = false
                });
            }

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Notification marked as read.",
                Data = true
            });
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead(
            int userId)
        {
            var notifications = await _context.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "All notifications marked as read.",
                Data = true
            });
        }
    }
}
