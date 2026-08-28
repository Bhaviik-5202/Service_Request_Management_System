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

        public ApprovalsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetApprovals(
            [FromQuery] ApprovalStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.Approvals
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Requester)
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Department)
                .Include(a => a.DecidedBy)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var approvals = await query
                .OrderByDescending(a => a.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return Ok(new ApiResponseDto<IEnumerable<ApprovalResponseDto>>
            {
                Success = true,
                Message = "Approvals fetched successfully.",
                Data = approvals,
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
        public async Task<ActionResult<ApiResponseDto<ApprovalResponseDto>>> GetApprovalById(int id)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Requester)
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Department)
                .Include(a => a.DecidedBy)
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
            {
                return NotFound(new ApiResponseDto<ApprovalResponseDto>
                {
                    Success = false,
                    Message = "Approval not found."
                });
            }

            var response = new ApprovalResponseDto
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
                DecidedBy = approval.DecidedBy?.FullName,
                DecidedAt = approval.DecidedAt,
                Remarks = approval.Remarks
            };

            return Ok(new ApiResponseDto<ApprovalResponseDto>
            {
                Success = true,
                Message = "Approval fetched successfully.",
                Data = response
            });
        }

        [HttpPut("{id}/decision")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakeDecision(
            int id,
            [FromBody] ApprovalDecisionDto dto)
        {
            if (!Enum.IsDefined(typeof(ApprovalStatus), dto.Decision))
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid approval decision."
                });
            }

            if (dto.Decision != ApprovalStatus.Approved &&
                dto.Decision != ApprovalStatus.Rejected)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Decision must be Approved or Rejected."
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
                    Message = "Approval not found."
                });
            }

            if (approval.Status != ApprovalStatus.Pending)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "This approval has already been decided."
                });
            }

            var decidedByUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    !u.IsDeleted &&
                    u.Status == UserStatus.Active);

            if (decidedByUser == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "No active user is available to make the decision."
                });
            }

            approval.Status = dto.Decision;
            approval.DecidedByUserId = decidedByUser.UserId;
            approval.DecidedAt = DateTime.UtcNow;
            approval.Remarks = dto.Remarks;

            if (approval.ServiceRequest != null)
            {
                var statusName = dto.Decision == ApprovalStatus.Approved
                    ? "Open"
                    : "Rejected";

                var requestStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s =>
                        s.StatusName == statusName &&
                        s.IsActive);

                if (requestStatus == null)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"Required status '{statusName}' was not found."
                    });
                }

                approval.ServiceRequest.StatusId = requestStatus.StatusId;
                approval.ServiceRequest.UpdatedAt = DateTime.UtcNow;

                _context.ServiceRequestTimeline.Add(
                    new ServiceRequestTimeline
                    {
                        RequestId = approval.RequestId,
                        StatusName = requestStatus.StatusName,
                        ChangedByUserId = decidedByUser.UserId,
                        ChangedAt = DateTime.UtcNow,
                        Note = string.IsNullOrWhiteSpace(dto.Remarks)
                            ? $"Request {dto.Decision.ToString().ToLower()}."
                            : dto.Remarks
                    });
            }

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = decidedByUser.UserId,
                Action = dto.Decision == ApprovalStatus.Approved ? "Approve" : "Reject",
                TargetType = "Approval",
                TargetId = approval.ApprovalId.ToString(),
                TargetDisplay = approval.ServiceRequest?.RequestNumber,
                Detail = $"Approval decision {dto.Decision} with remarks: {dto.Remarks}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = $"Approval {dto.Decision.ToString().ToLower()} successfully.",
                Data = true
            });
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApprovalResponseDto>>>> GetPendingApprovals()
        {
            var approvals = await _context.Approvals
                .Where(a => a.Status == ApprovalStatus.Pending)
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Requester)
                .Include(a => a.ServiceRequest)
                    .ThenInclude(r => r!.Department)
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
    }
}
