using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class ApprovalService : IApprovalService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;

        public ApprovalService(IUnitOfWork uow, AppDbContext context)
        {
            _uow = uow;
            _context = context;
        }

        public async Task<ApiResponseDto<IEnumerable<ApprovalResponseDto>>> GetApprovalsAsync()
        {
            var approvals = await _context.Approvals
                .AsNoTracking()
                .OrderByDescending(a => a.SubmittedAt)
                .Select(ApprovalSelector)
                .ToListAsync();

            return Ok("Approvals fetched successfully.", (IEnumerable<ApprovalResponseDto>)approvals);
        }

        public async Task<ApiResponseDto<ApprovalResponseDto>> GetApprovalByIdAsync(int id)
        {
            var approval = await _context.Approvals
                .AsNoTracking()
                .Where(a => a.ApprovalId == id)
                .Select(ApprovalSelector)
                .FirstOrDefaultAsync();

            if (approval == null)
                return Fail<ApprovalResponseDto>("Approval not found.");

            return Ok("Approval fetched successfully.", approval);
        }

        public async Task<ApiResponseDto<IEnumerable<ApprovalResponseDto>>> GetPendingApprovalsAsync()
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

            return Ok("Pending approvals fetched successfully.", (IEnumerable<ApprovalResponseDto>)approvals);
        }

        public async Task<ApiResponseDto<bool>> MakeDecisionAsync(int id, ApprovalDecisionDto dto, string? ipAddress)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
                return Fail<bool>("Approval not found.");

            if (approval.Status != ApprovalStatus.Pending)
                return Fail<bool>("This approval has already been decided.");

            var decidedByUser = await _uow.Users.FirstOrDefaultAsync(u =>
                u.UserId == dto.DecidedByUserId && u.Status == UserStatus.Active);

            if (decidedByUser == null)
                return Fail<bool>("Invalid or inactive decision maker user.");

            var statusName = dto.Decision == ApprovalStatus.Approved
                ? RequestStatusNames.Open
                : RequestStatusNames.Rejected;

            var requestStatus = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                s.StatusName == statusName && s.IsActive);

            if (requestStatus == null)
                return Fail<bool>($"Required status '{statusName}' was not found.");

            var now = DateTime.UtcNow;
            approval.Status = dto.Decision;
            approval.DecidedByUserId = decidedByUser.UserId;
            approval.DecidedAt = now;
            approval.Remarks = dto.Remarks;

            if (approval.ServiceRequest != null)
            {
                approval.ServiceRequest.StatusId = requestStatus.StatusId;
                approval.ServiceRequest.UpdatedAt = now;

                await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
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

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = decidedByUser.UserId,
                Action = dto.Decision == ApprovalStatus.Approved ? "Approve" : "Reject",
                TargetType = "Approval",
                TargetId = approval.ApprovalId.ToString(),
                TargetDisplay = approval.ServiceRequest?.RequestNumber,
                Detail = $"Approval decision {dto.Decision} with remarks: {dto.Remarks}",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();

            return Ok($"Approval {dto.Decision.ToString().ToLowerInvariant()} successfully.", true);
        }

        private static readonly System.Linq.Expressions.Expression<Func<Approval, ApprovalResponseDto>> ApprovalSelector =
            a => new ApprovalResponseDto
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
            };

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
