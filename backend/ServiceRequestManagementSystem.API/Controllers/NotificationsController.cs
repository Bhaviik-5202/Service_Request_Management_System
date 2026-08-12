using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/v1/notifications
        /// Get paginated inbox alerts for current user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<NotificationResponseDto>>>> GetNotifications(
            [FromQuery] NotificationType? type,
            [FromQuery] bool? isRead,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Notifications.AsQueryable();

            if (type.HasValue)
                query = query.Where(n => n.NotificationType == type.Value);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                Message = "Notifications retrieved.",
                Data = notifications,
                Pagination = new PaginationMetadataDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            });
        }

        /// <summary>
        /// GET /api/v1/notifications/unread-count
        /// Get unread notification count for top navbar badge.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount()
        {
            var count = await _context.Notifications.CountAsync(n => !n.IsRead);
            return Ok(new ApiResponseDto<int> { Success = true, Data = count });
        }

        /// <summary>
        /// PUT /api/v1/notifications/{id}/read
        /// Mark single notification as read.
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Notification not found." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool> { Success = true, Message = "Notification marked as read.", Data = true });
        }

        /// <summary>
        /// PUT /api/v1/notifications/read-all
        /// Mark all notifications for user as read.
        /// </summary>
        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead()
        {
            var unread = await _context.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var item in unread)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponseDto<bool> { Success = true, Message = "All notifications marked as read.", Data = true });
        }
    }
}
