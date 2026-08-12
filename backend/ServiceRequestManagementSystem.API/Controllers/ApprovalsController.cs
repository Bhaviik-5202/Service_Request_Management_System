using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/approvals")]
    public class ApprovalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApprovalsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/v1/approvals/pending
        /// Get pending approval requests for HOD department queue.
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetPendingApprovals()
        {
            var pending = await _context.Approvals
                .Include(a => a.ServiceRequest).ThenInclude(sr => sr!.Requester)
                .Include(a => a.ServiceRequest).ThenInclude(sr => sr!.Department)
                .Where(a => a.Status == ApprovalStatus.Pending)
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new ApprovalResponseDto
                {
                    ApprovalId = a.ApprovalId,
                    RequestId = a.RequestId,
                    RequestNo = a.ServiceRequest!.RequestNumber,
                    Title = a.ServiceRequest.Title,
                    Requester = a.ServiceRequest.Requester!.FullName,
                    Department = a.ServiceRequest.Department!.DepartmentName,
                    Priority = a.ServiceRequest.Priority,
                    SubmittedAt = a.SubmittedAt,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ApprovalResponseDto>> { Success = true, Data = pending });
        }

        /// <summary>
        /// GET /api/v1/approvals/history
        /// Get historical approval decisions log.
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetApprovalHistory()
        {
            var history = await _context.Approvals
                .Include(a => a.ServiceRequest).ThenInclude(sr => sr!.Requester)
                .Include(a => a.ServiceRequest).ThenInclude(sr => sr!.Department)
                .Include(a => a.DecidedBy)
                .Where(a => a.Status != ApprovalStatus.Pending)
                .OrderByDescending(a => a.DecidedAt)
                .Select(a => new ApprovalResponseDto
                {
                    ApprovalId = a.ApprovalId,
                    RequestId = a.RequestId,
                    RequestNo = a.ServiceRequest!.RequestNumber,
                    Title = a.ServiceRequest.Title,
                    Requester = a.ServiceRequest.Requester!.FullName,
                    Department = a.ServiceRequest.Department!.DepartmentName,
                    Priority = a.ServiceRequest.Priority,
                    SubmittedAt = a.SubmittedAt,
                    Status = a.Status,
                    DecidedBy = a.DecidedBy != null ? a.DecidedBy.FullName : null,
                    DecidedAt = a.DecidedAt,
                    Remarks = a.Remarks
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ApprovalResponseDto>> { Success = true, Data = history });
        }

        /// <summary>
        /// POST /api/v1/approvals/{id}/decide
        /// Process HOD approval decision (Approved or Rejected) with mandatory remarks on rejection.
        /// </summary>
        [HttpPost("{id}/decide")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakeDecision(int id, [FromBody] ApprovalDecisionDto dto)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .FirstOrDefaultAsync(a => a.ApprovalId == id || a.RequestId == id);

            if (approval == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Approval record not found." });

            if (dto.Decision == ApprovalStatus.Rejected && string.IsNullOrWhiteSpace(dto.Remarks))
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Rejection decision requires non-empty remarks explaining the reason."
                });
            }

            var hodUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.HOD)
                           ?? await _context.Users.FirstOrDefaultAsync()
                           ?? new User { UserId = 1 };

            approval.Status = dto.Decision;
            approval.DecidedByUserId = hodUser.UserId;
            approval.DecidedAt = DateTime.UtcNow;
            approval.Remarks = dto.Remarks;

            // Update linked ServiceRequest status
            if (approval.ServiceRequest != null)
            {
                var targetStatusName = dto.Decision == ApprovalStatus.Approved ? "Open" : "Rejected";
                var status = await _context.ServiceRequestStatuses.FirstOrDefaultAsync(s => s.StatusName == targetStatusName)
                             ?? await _context.ServiceRequestStatuses.FirstAsync();

                approval.ServiceRequest.StatusId = status.StatusId;
                approval.ServiceRequest.UpdatedAt = DateTime.UtcNow;

                _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
                {
                    RequestId = approval.RequestId,
                    StatusName = status.StatusName,
                    ChangedByUserId = hodUser.UserId,
                    ChangedAt = DateTime.UtcNow,
                    Note = $"HOD decision: {dto.Decision}. Remarks: {dto.Remarks}"
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = $"Request decision saved as {dto.Decision}.",
                Data = true
            });
        }
    }
}
