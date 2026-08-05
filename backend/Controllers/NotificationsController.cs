using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Notifications/GetNotifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // GET: api/Notifications/GetNotification/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            return Ok(notification);
        }

        // GET: api/Notifications/GetNotificationsByUser/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByUser(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/Notifications/GetUnreadNotificationsByUser/user/5/unread
        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUnreadNotificationsByUser(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/Notifications/GetUnreadNotificationCount/user/5/count
        [HttpGet("user/{userId}/count")]
        public async Task<ActionResult<int>> GetUnreadNotificationCount(int userId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(count);
        }

        // POST: api/Notifications/PostNotification
        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == notification.UserId && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest($"User with ID {notification.UserId} not found or is deleted.");
            }

            var validTypes = new[] { "request", "approval", "asset", "system" };
            if (!validTypes.Contains(notification.NotificationType))
            {
                return BadRequest($"Invalid notification type. Valid values: {string.Join(", ", validTypes)}");
            }

            notification.CreatedAt = DateTime.UtcNow;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notification);
        }

        // PUT: api/Notifications/MarkNotificationAsRead/5/mark-read
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            notification.IsRead = true;
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Notifications/MarkAllNotificationsAsRead/user/5/mark-all-read
        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
            {
                return NoContent();
            }

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Notifications/DeleteNotification/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Notifications/ClearAllNotifications/user/5/clear-all
        [HttpDelete("user/{userId}/clear-all")]
        public async Task<IActionResult> ClearAllNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!notifications.Any())
            {
                return NoContent();
            }

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
