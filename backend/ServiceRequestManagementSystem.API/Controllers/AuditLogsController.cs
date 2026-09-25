using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.AuditLogs;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AuditLogResponseDto>>>> GetAuditLogs()
        {
            var result = await _auditLogService.GetAuditLogsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AuditLogResponseDto>>> GetAuditLogById(long id)
        {
            var result = await _auditLogService.GetAuditLogByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
