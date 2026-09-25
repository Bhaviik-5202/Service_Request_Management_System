using ServiceRequestManagementSystem.API.DTOs.AuditLogs;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<ApiResponseDto<IEnumerable<AuditLogResponseDto>>> GetAuditLogsAsync();
        Task<ApiResponseDto<AuditLogResponseDto>> GetAuditLogByIdAsync(long id);
    }
}
