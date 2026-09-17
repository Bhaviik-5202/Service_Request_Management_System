using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.AuditLogs;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AuditLogResponseDto>>>> GetAuditLogs()
        {
            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Select(AuditLogResponseSelector)
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<AuditLogResponseDto>>
            {
                Success = true,
                Message = "Audit logs fetched successfully.",
                Data = auditLogs
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AuditLogResponseDto>>> GetAuditLogById(
            long id)
        {
            var auditLog = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.AuditLogId == id)
                .Select(AuditLogResponseSelector)
                .FirstOrDefaultAsync();

            if (auditLog == null)
            {
                return NotFound(new ApiResponseDto<AuditLogResponseDto>
                {
                    Success = false,
                    Message = "Audit log not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<AuditLogResponseDto>
            {
                Success = true,
                Message = "Audit log fetched successfully.",
                Data = auditLog
            });
        }

        private static readonly System.Linq.Expressions.Expression<
            Func<AuditLog, AuditLogResponseDto>> AuditLogResponseSelector = auditLog =>
            new AuditLogResponseDto
            {
                AuditLogId = auditLog.AuditLogId,
                ActorUserId = auditLog.ActorUserId,
                ActorName = auditLog.Actor != null
                    ? auditLog.Actor.FullName
                    : null,
                Action = auditLog.Action,
                TargetType = auditLog.TargetType,
                TargetId = auditLog.TargetId,
                TargetDisplay = auditLog.TargetDisplay,
                Detail = auditLog.Detail,
                IpAddress = auditLog.IpAddress,
                CreatedAt = auditLog.CreatedAt
            };
    }
}
