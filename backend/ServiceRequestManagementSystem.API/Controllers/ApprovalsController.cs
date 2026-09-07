using FluentValidation;
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
    [Route("api/[controller]")]
    public class ApprovalsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<ApprovalDecisionDto> _decisionValidator;

        public ApprovalsController(
            AppDbContext context,
            IValidator<ApprovalDecisionDto> decisionValidator)
        {
            _context = context;
            _decisionValidator = decisionValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetApprovals()
        {
            var approvals = await _context.Approvals
                .AsNoTracking()
                .OrderByDescending(a => a.SubmittedAt)
                .Select(ApprovalResponseSelector)
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ApprovalResponseDto>>
            {
                Success = true,
                Message = "Approvals fetched successfully.",
                Data = approvals
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ApprovalResponseDto>>> GetApprovalById(
            int id)
        {
            var approval = await _context.Approvals
                .AsNoTracking()
                .Where(a => a.ApprovalId == id)
                .Select(ApprovalResponseSelector)
                .FirstOrDefaultAsync();

            if (approval == null)
            {
                return NotFound(new ApiResponseDto<ApprovalResponseDto>
                {
                    Success = false,
                    Message = "Approval not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<ApprovalResponseDto>
            {
                Success = true,
                Message = "Approval fetched successfully.",
                Data = approval
            });
        }

        [HttpPut("{id}/decision")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakeDecision(
            int id,
            ApprovalDecisionDto dto)
        {
            var validation = await _decisionValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = false
                });
            }

            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Approval not found.",
                    Data = false
                });
            }

            if (approval.Status != ApprovalStatus.Pending)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "This approval has already been decided.",
                    Data = false
                });
            }

            var decidedByUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.DecidedByUserId &&
                    u.Status == UserStatus.Active);

            if (decidedByUser == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or inactive decision maker user.",
                    Data = false
                });
            }

            var statusName = dto.Decision == ApprovalStatus.Approved
                ? RequestStatusNames.Open
                : RequestStatusNames.Rejected;

            var requestStatus = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == statusName &&
                    s.IsActive);

            if (requestStatus == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Required status '{statusName}' was not found.",
                    Data = false
                });
            }

            var now = DateTime.UtcNow;

            approval.Status = dto.Decision;
            approval.DecidedByUserId = decidedByUser.UserId;
            approval.DecidedAt = now;
            approval.Remarks = dto.Remarks;

            if (approval.ServiceRequest != null)
            {
                approval.ServiceRequest.StatusId = requestStatus.StatusId;
                approval.ServiceRequest.UpdatedAt = now;

                _context.ServiceRequestTimeline.Add(
                    new ServiceRequestTimeline
                    {
                        RequestId = approval.RequestId,
                        StatusName = requestStatus.StatusName,
                        ChangedByUserId = decidedByUser.UserId,
                        ChangedAt = now,
                        Note = string.IsNullOrWhiteSpace(dto.Remarks)
                            ? $"Request {dto.Decision.ToString().ToLowerInvariant()}."
                            : dto.Remarks
                    });
            }

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = decidedByUser.UserId,
                Action = dto.Decision == ApprovalStatus.Approved
                    ? "Approve"
                    : "Reject",
                TargetType = "Approval",
                TargetId = approval.ApprovalId.ToString(),
                TargetDisplay = approval.ServiceRequest?.RequestNumber,
                Detail = $"Approval decision {dto.Decision} with remarks: {dto.Remarks}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = $"Approval {dto.Decision.ToString().ToLowerInvariant()} successfully.",
                Data = true
            });
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetPendingApprovals()
        {
            var approvals = await _context.Approvals
                .AsNoTracking()
                .Where(a => a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.SubmittedAt)
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
                    DecidedBy = null,
                    DecidedAt = null,
                    Remarks = a.Remarks
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ApprovalResponseDto>>
            {
                Success = true,
                Message = "Pending approvals fetched successfully.",
                Data = approvals
            });
        }

        private static readonly System.Linq.Expressions.Expression<
            Func<Approval, ApprovalResponseDto>> ApprovalResponseSelector = approval =>
            new ApprovalResponseDto
            {
                ApprovalId = approval.ApprovalId,
                RequestId = approval.RequestId,
                RequestNo = approval.ServiceRequest!.RequestNumber,
                Title = approval.ServiceRequest.Title,
                Requester = approval.ServiceRequest.Requester!.FullName,
                Department = approval.ServiceRequest.Department!.DepartmentName,
                Priority = approval.ServiceRequest.Priority,
                SubmittedAt = approval.SubmittedAt,
                Status = approval.Status,
                DecidedBy = approval.DecidedBy != null
                    ? approval.DecidedBy.FullName
                    : null,
                DecidedAt = approval.DecidedAt,
                Remarks = approval.Remarks
            };
    }
}
