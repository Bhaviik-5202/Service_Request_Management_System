using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HOD")]
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        public ApprovalsController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetApprovals()
        {
            var result = await _approvalService.GetApprovalsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ApprovalResponseDto>>> GetApprovalById(int id)
        {
            var result = await _approvalService.GetApprovalByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetPendingApprovals()
        {
            var result = await _approvalService.GetPendingApprovalsAsync();
            return Ok(result);
        }

        [HttpPut("{id}/decision")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakeDecision(int id, [FromBody] ApprovalDecisionDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _approvalService.MakeDecisionAsync(id, dto, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Approval not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
