using ServiceRequestManagementSystem.API.DTOs.AuditLogs;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<IEnumerable<AuditLogResponseDto>>> GetAuditLogsAsync()
        {
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
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

            return Ok("Audit logs fetched successfully.", (IEnumerable<AuditLogResponseDto>)logs);
        }

        public async Task<ApiResponseDto<AuditLogResponseDto>> GetAuditLogByIdAsync(long id)
        {
            var log = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.AuditLogId == id)
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
                .FirstOrDefaultAsync();

            if (log == null)
                return Fail<AuditLogResponseDto>("Audit log not found.");

            return Ok("Audit log fetched successfully.", log);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
