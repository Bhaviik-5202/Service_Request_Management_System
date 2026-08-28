using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.AuditLogs;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AuditLogResponseDto>>>> GetAuditLogs(
            [FromQuery] int? actorUserId,
            [FromQuery] string? action,
            [FromQuery] string? targetType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.AuditLogs
                .Include(a => a.Actor)
                .AsQueryable();

            if (actorUserId.HasValue)
                query = query.Where(a => a.ActorUserId == actorUserId.Value);

            if (!string.IsNullOrWhiteSpace(action))
            {
                var actionText = action.Trim().ToLower();
                query = query.Where(a => a.Action.ToLower().Contains(actionText));
            }

            if (!string.IsNullOrWhiteSpace(targetType))
            {
                var targetTypeText = targetType.Trim().ToLower();
                query = query.Where(a => a.TargetType.ToLower().Contains(targetTypeText));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var auditLogs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogResponseDto
                {
                    AuditLogId = a.AuditLogId,
                    ActorUserId = a.ActorUserId,
                    ActorName = a.Actor != null ? a.Actor.FullName : null,
                    Action = a.Action,
                    TargetType = a.TargetType,
                    TargetId = a.TargetId,
                    TargetDisplay = a.TargetDisplay,
                    Detail = a.Detail,
                    IpAddress = a.IpAddress,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<AuditLogResponseDto>>
            {
                Success = true,
                Message = "Audit logs fetched successfully.",
                Data = auditLogs,
                Pagination = new PaginationMetadataDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AuditLogResponseDto>>> GetAuditLogById(long id)
        {
            var auditLog = await _context.AuditLogs
                .Include(a => a.Actor)
                .FirstOrDefaultAsync(a => a.AuditLogId == id);

            if (auditLog == null)
            {
                return NotFound(new ApiResponseDto<AuditLogResponseDto>
                {
                    Success = false,
                    Message = "Audit log not found."
                });
            }

            var response = new AuditLogResponseDto
            {
                AuditLogId = auditLog.AuditLogId,
                ActorUserId = auditLog.ActorUserId,
                ActorName = auditLog.Actor?.FullName,
                Action = auditLog.Action,
                TargetType = auditLog.TargetType,
                TargetId = auditLog.TargetId,
                TargetDisplay = auditLog.TargetDisplay,
                Detail = auditLog.Detail,
                IpAddress = auditLog.IpAddress,
                CreatedAt = auditLog.CreatedAt
            };

            return Ok(new ApiResponseDto<AuditLogResponseDto>
            {
                Success = true,
                Message = "Audit log fetched successfully.",
                Data = response
            });
        }
    }
}
