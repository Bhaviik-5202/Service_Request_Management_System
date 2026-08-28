using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Notifications;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<NotificationResponseDto>>>> GetNotifications(
            [FromQuery] int userId,
            [FromQuery] NotificationType? type,
            [FromQuery] bool? isRead,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsQueryable();

            if (type.HasValue)
                query = query.Where(n => n.NotificationType == type.Value);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            var totalRecords = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

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
                Message = "Notifications fetched successfully.",
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

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount(
            [FromQuery] int userId)
        {
            var count = await _context.Notifications
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
            [FromQuery] int userId)
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
                    Message = "Notification not found."
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
            [FromQuery] int userId)
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
