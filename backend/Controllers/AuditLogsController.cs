using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AuditLogs/GetAuditLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            return await _context.AuditLogs
                .Include(al => al.ActorUser)
                .OrderByDescending(al => al.CreatedAt)
                .Take(1000)
                .ToListAsync();
        }

        // GET: api/AuditLogs/GetAuditLog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(long id)
        {
            var auditLog = await _context.AuditLogs
                .Include(al => al.ActorUser)
                .FirstOrDefaultAsync(al => al.AuditLogId == id);

            if (auditLog == null)
            {
                return NotFound($"Audit Log with ID {id} not found.");
            }

            return Ok(auditLog);
        }

        // GET: api/AuditLogs/GetAuditLogsByUser/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogsByUser(int userId)
        {
            var logs = await _context.AuditLogs
                .Where(al => al.ActorUserId == userId)
                .Include(al => al.ActorUser)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/AuditLogs/GetAuditLogsByTargetType/target/ServiceRequest
        [HttpGet("target/{targetType}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogsByTargetType(string targetType)
        {
            var logs = await _context.AuditLogs
                .Where(al => al.TargetType == targetType)
                .Include(al => al.ActorUser)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/AuditLogs/GetAuditLogsByTarget/target/ServiceRequest/123
        [HttpGet("target/{targetType}/{targetId}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogsByTarget(string targetType, string targetId)
        {
            var logs = await _context.AuditLogs
                .Where(al => al.TargetType == targetType && al.TargetId == targetId)
                .Include(al => al.ActorUser)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/AuditLogs/GetAuditLogsByDateRange/date-range
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var logs = await _context.AuditLogs
                .Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .Include(al => al.ActorUser)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // POST: api/AuditLogs/PostAuditLog
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog auditLog)
        {
            if (auditLog.ActorUserId.HasValue)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == auditLog.ActorUserId && !u.IsDeleted);

                if (user == null)
                {
                    return BadRequest($"User with ID {auditLog.ActorUserId} not found or is deleted.");
                }
            }

            if (string.IsNullOrWhiteSpace(auditLog.Action))
            {
                return BadRequest("Action is required.");
            }

            if (string.IsNullOrWhiteSpace(auditLog.TargetType))
            {
                return BadRequest("TargetType is required.");
            }

            auditLog.CreatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.AuditLogId }, auditLog);
        }

        // DELETE: api/AuditLogs/DeleteAuditLog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditLog(long id)
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(al => al.AuditLogId == id);

            if (auditLog == null)
            {
                return NotFound($"Audit Log with ID {id} not found.");
            }

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/AuditLogs/DeleteAuditLogsOlderThan/older-than/90
        [HttpDelete("older-than/{days}")]
        public async Task<IActionResult> DeleteAuditLogsOlderThan(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var logsToDelete = await _context.AuditLogs
                .Where(al => al.CreatedAt < cutoffDate)
                .ToListAsync();

            if (!logsToDelete.Any())
            {
                return NoContent();
            }

            _context.AuditLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();

            return Ok($"Deleted {logsToDelete.Count} audit logs older than {days} days.");
        }
    }
}