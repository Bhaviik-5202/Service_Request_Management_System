using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;
        private readonly IValidator<CreateServiceRequestDto> _createValidator;
        private readonly IValidator<UpdateServiceRequestStatusDto> _statusValidator;
        private readonly IValidator<AssignTechnicianDto> _assignValidator;
        private readonly IValidator<CreateReplyDto> _replyValidator;

        public ServiceRequestService(
            IUnitOfWork uow,
            AppDbContext context,
            IValidator<CreateServiceRequestDto> createValidator,
            IValidator<UpdateServiceRequestStatusDto> statusValidator,
            IValidator<AssignTechnicianDto> assignValidator,
            IValidator<CreateReplyDto> replyValidator)
        {
            _uow = uow;
            _context = context;
            _createValidator = createValidator;
            _statusValidator = statusValidator;
            _assignValidator = assignValidator;
            _replyValidator = replyValidator;
        }

        public async Task<ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>> GetRequestsAsync()
        {
            var requests = await _context.ServiceRequests
                .AsNoTracking()
                .OrderBy(r => r.RequestId)
                .Select(r => new ServiceRequestResponseDto
                {
                    RequestId = r.RequestId,
                    RequestNumber = r.RequestNumber,
                    Title = r.Title,
                    Description = r.Description,
                    ServiceType = r.ServiceType!.ServiceTypeName,
                    RequestType = r.RequestType!.RequestTypeName,
                    Department = r.Department!.DepartmentName,
                    Requester = r.Requester!.FullName,
                    RequesterEmail = r.Requester.Email,
                    Assignee = r.Assignee != null ? r.Assignee.FullName : null,
                    Status = r.Status!.StatusName,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    ResolvedAt = r.ResolvedAt
                })
                .ToListAsync();

            return Ok("Service requests fetched successfully.", (IEnumerable<ServiceRequestResponseDto>)requests);
        }

        public async Task<ApiResponseDto<ServiceRequestDetailResponseDto>> GetRequestByIdAsync(int id)
        {
            var request = await _context.ServiceRequests
                .AsSplitQuery()
                .Include(r => r.ServiceType)
                .Include(r => r.RequestType)
                .Include(r => r.Department)
                .Include(r => r.Requester)
                .Include(r => r.Assignee)
                .Include(r => r.Status)
                .Include(r => r.Replies)
                    .ThenInclude(r => r.Author)
                .Include(r => r.Replies)
                    .ThenInclude(r => r.StatusTransition)
                .Include(r => r.TimelineEntries)
                    .ThenInclude(t => t.ChangedBy)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return Fail<ServiceRequestDetailResponseDto>("Service request not found.");

            var response = new ServiceRequestDetailResponseDto
            {
                RequestId = request.RequestId,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                ServiceType = request.ServiceType!.ServiceTypeName,
                RequestType = request.RequestType!.RequestTypeName,
                Department = request.Department!.DepartmentName,
                Requester = request.Requester!.FullName,
                RequesterEmail = request.Requester.Email,
                Assignee = request.Assignee?.FullName,
                Status = request.Status!.StatusName,
                Priority = request.Priority,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ResolvedAt = request.ResolvedAt,

                Replies = request.Replies
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new ServiceRequestReplyResponseDto
                    {
                        ReplyId = r.ReplyId,
                        Author = r.Author!.FullName,
                        Role = r.Author.Role.ToString(),
                        Message = r.Message,
                        CreatedAt = r.CreatedAt,
                        StatusTransition = r.StatusTransition != null
                            ? r.StatusTransition.StatusName
                            : null
                    })
                    .ToList(),

                Timeline = request.TimelineEntries
                    .OrderBy(t => t.ChangedAt)
                    .Select(t => new ServiceRequestTimelineResponseDto
                    {
                        TimelineId = t.TimelineId,
                        Status = t.StatusName,
                        ChangedBy = t.ChangedBy!.FullName,
                        ChangedAt = t.ChangedAt,
                        Note = t.Note
                    })
                    .ToList(),

                Attachments = request.Attachments
                    .Select(a => new ServiceRequestAttachmentResponseDto
                    {
                        AttachmentId = a.AttachmentId,
                        FileName = a.FileName,
                        FileSizeKB = a.FileSizeKB,
                        FileUrl = a.FileUrl,
                        UploadedAt = a.UploadedAt
                    })
                    .ToList()
            };

            return Ok("Service request fetched successfully.", response);
        }

        public async Task<ApiResponseDto<ServiceRequestResponseDto>> CreateRequestAsync(
            CreateServiceRequestDto dto, string? ipAddress)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<ServiceRequestResponseDto>(validation.Errors.First().ErrorMessage);

            var serviceType = await _uow.ServiceTypes.FirstOrDefaultAsync(s =>
                s.ServiceTypeId == dto.ServiceTypeId && s.IsActive);

            if (serviceType == null)
                return Fail<ServiceRequestResponseDto>("Invalid or inactive service type.");

            var requestType = await _uow.RequestTypes.FirstOrDefaultAsync(r =>
                r.RequestTypeId == dto.RequestTypeId && r.IsActive);

            if (requestType == null)
                return Fail<ServiceRequestResponseDto>("Invalid or inactive request type.");

            if (requestType.ServiceTypeId != dto.ServiceTypeId)
                return Fail<ServiceRequestResponseDto>("Request type does not belong to the selected service type.");

            var department = await _uow.Departments.FirstOrDefaultAsync(d =>
                d.DepartmentId == dto.DepartmentId && d.IsActive);

            if (department == null)
                return Fail<ServiceRequestResponseDto>("Invalid or inactive department.");

            var requester = await _uow.Users.FirstOrDefaultAsync(u =>
                u.UserId == dto.RequesterUserId && u.Status == UserStatus.Active);

            if (requester == null)
                return Fail<ServiceRequestResponseDto>("No active user is available to create the request.");

            var initialStatusName = requestType.RequiresApproval
                ? RequestStatusNames.PendingApproval
                : RequestStatusNames.Open;

            var status = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                s.StatusName == initialStatusName && s.IsActive);

            if (status == null)
                return Fail<ServiceRequestResponseDto>($"Required status '{initialStatusName}' was not found.");

            var now = DateTime.UtcNow;
            var request = new ServiceRequest
            {
                RequestNumber = $"TMP-{Guid.NewGuid():N}"[..16],
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                ServiceTypeId = dto.ServiceTypeId,
                RequestTypeId = dto.RequestTypeId,
                DepartmentId = dto.DepartmentId,
                RequesterUserId = requester.UserId,
                StatusId = status.StatusId,
                Priority = dto.Priority,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _uow.ServiceRequests.AddAsync(request);
            await _uow.SaveChangesAsync();

            request.RequestNumber = $"SR-{now:yyyy}-{request.RequestId:D6}";

            if (requestType.RequiresApproval)
            {
                await _uow.Approvals.AddAsync(new Approval
                {
                    RequestId = request.RequestId,
                    Status = ApprovalStatus.Pending,
                    SubmittedAt = now
                });
            }

            await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = requester.UserId,
                ChangedAt = now,
                Note = requestType.RequiresApproval
                    ? "Request raised and awaiting HOD approval."
                    : "Request raised."
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = requester.UserId,
                Action = "Create",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request '{request.Title}' ({request.RequestNumber}) created.",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();

            var response = new ServiceRequestResponseDto
            {
                RequestId = request.RequestId,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                ServiceType = serviceType.ServiceTypeName,
                RequestType = requestType.RequestTypeName,
                Department = department.DepartmentName,
                Requester = requester.FullName,
                RequesterEmail = requester.Email,
                Status = status.StatusName,
                Priority = request.Priority,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ResolvedAt = request.ResolvedAt
            };

            return Ok("Service request created successfully.", response);
        }

        public async Task<ApiResponseDto<object>> UpdateStatusAsync(
            int id, UpdateServiceRequestStatusDto dto, int actorUserId, string? ipAddress)
        {
            var validation = await _statusValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<object>(validation.Errors.First().ErrorMessage);

            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<object>("Service request not found.");

            var status = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                s.StatusId == dto.StatusId && s.IsActive);

            if (status == null)
                return Fail<object>("Invalid or inactive status.");

            var updatedAt = DateTime.UtcNow;
            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = status.StatusName == RequestStatusNames.Resolved
                ? request.ResolvedAt ?? updatedAt
                : null;

            await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = actorUserId > 0 ? actorUserId : request.RequesterUserId,
                ChangedAt = updatedAt,
                Note = string.IsNullOrWhiteSpace(dto.Note)
                    ? $"Status updated to {status.StatusName}."
                    : dto.Note
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = actorUserId > 0 ? actorUserId : null,
                Action = "StatusChange",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Status updated to {status.StatusName}.",
                IpAddress = ipAddress,
                CreatedAt = updatedAt
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Status updated successfully.", true);
        }

        public async Task<ApiResponseDto<object>> AssignTechnicianAsync(
            int id, AssignTechnicianDto dto, int actorUserId, string? ipAddress)
        {
            var validation = await _assignValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<object>(validation.Errors.First().ErrorMessage);

            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<object>("Service request not found.");

            var technician = await _uow.Users.FirstOrDefaultAsync(u =>
                u.UserId == dto.AssigneeUserId &&
                u.Status == UserStatus.Active &&
                u.Role == UserRole.Technician);

            if (technician == null)
                return Fail<object>("Invalid or inactive technician.");

            var updatedAt = DateTime.UtcNow;
            request.AssigneeUserId = technician.UserId;
            request.UpdatedAt = updatedAt;

            await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = RequestStatusNames.Assigned,
                ChangedByUserId = actorUserId > 0 ? actorUserId : request.RequesterUserId,
                ChangedAt = updatedAt,
                Note = $"Request assigned to {technician.FullName}."
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = actorUserId > 0 ? actorUserId : null,
                Action = "Assign",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Request assigned to technician {technician.FullName}.",
                IpAddress = ipAddress,
                CreatedAt = updatedAt
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Technician assigned successfully.", true);
        }

        public async Task<ApiResponseDto<object>> CancelRequestAsync(int id, int actorUserId, string? ipAddress)
        {
            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<object>("Service request not found.");

            var status = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                s.StatusName == RequestStatusNames.Cancelled && s.IsActive);

            if (status == null)
                return Fail<object>("Cancelled status was not found.");

            var updatedAt = DateTime.UtcNow;
            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = null;

            await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = actorUserId > 0 ? actorUserId : request.RequesterUserId,
                ChangedAt = updatedAt,
                Note = "Service request cancelled."
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = actorUserId > 0 ? actorUserId : null,
                Action = "Cancel",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request {request.RequestNumber} cancelled.",
                IpAddress = ipAddress,
                CreatedAt = updatedAt
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Service request cancelled successfully.", true);
        }

        public async Task<ApiResponseDto<object>> ReopenRequestAsync(int id, int actorUserId, string? ipAddress)
        {
            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<object>("Service request not found.");

            var status = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                s.StatusName == RequestStatusNames.Open && s.IsActive);

            if (status == null)
                return Fail<object>("Open status was not found.");

            var updatedAt = DateTime.UtcNow;
            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = null;

            await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = actorUserId > 0 ? actorUserId : request.RequesterUserId,
                ChangedAt = updatedAt,
                Note = "Service request reopened."
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = actorUserId > 0 ? actorUserId : null,
                Action = "Reopen",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request {request.RequestNumber} reopened.",
                IpAddress = ipAddress,
                CreatedAt = updatedAt
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Service request reopened successfully.", true);
        }

        public async Task<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>> GetRepliesAsync(int id)
        {
            var requestExists = await _uow.ServiceRequests.AnyAsync(r => r.RequestId == id);
            if (!requestExists)
                return Fail<IEnumerable<ServiceRequestReplyResponseDto>>("Service request not found.");

            var replies = await _context.ServiceRequestReplies
                .AsNoTracking()
                .Where(r => r.RequestId == id)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new ServiceRequestReplyResponseDto
                {
                    ReplyId = r.ReplyId,
                    Author = r.Author!.FullName,
                    Role = r.Author.Role.ToString(),
                    Message = r.Message,
                    CreatedAt = r.CreatedAt,
                    StatusTransition = r.StatusTransition != null ? r.StatusTransition.StatusName : null
                })
                .ToListAsync();

            return Ok("Replies fetched successfully.", (IEnumerable<ServiceRequestReplyResponseDto>)replies);
        }

        public async Task<ApiResponseDto<ServiceRequestReplyResponseDto>> AddReplyAsync(
            int id, CreateReplyDto dto, string? ipAddress)
        {
            var validation = await _replyValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<ServiceRequestReplyResponseDto>(validation.Errors.First().ErrorMessage);

            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<ServiceRequestReplyResponseDto>("Service request not found.");

            if (dto.StatusTransitionId.HasValue)
            {
                var statusExists = await _uow.ServiceRequestStatuses.AnyAsync(s =>
                    s.StatusId == dto.StatusTransitionId.Value && s.IsActive);

                if (!statusExists)
                    return Fail<ServiceRequestReplyResponseDto>("Invalid status transition.");
            }

            var author = await _uow.Users.FirstOrDefaultAsync(u =>
                u.UserId == dto.AuthorUserId && u.Status == UserStatus.Active);

            if (author == null)
                return Fail<ServiceRequestReplyResponseDto>("No active user is available to post the reply.");

            var now = DateTime.UtcNow;
            var reply = new ServiceRequestReply
            {
                RequestId = request.RequestId,
                AuthorUserId = author.UserId,
                Message = dto.Message.Trim(),
                StatusTransitionId = dto.StatusTransitionId,
                CreatedAt = now
            };

            await _uow.ServiceRequestReplies.AddAsync(reply);

            string? statusTransitionName = null;
            if (dto.StatusTransitionId.HasValue)
            {
                var newStatus = await _uow.ServiceRequestStatuses.FirstOrDefaultAsync(s =>
                    s.StatusId == dto.StatusTransitionId.Value && s.IsActive);

                request.StatusId = dto.StatusTransitionId.Value;
                request.UpdatedAt = now;
                request.ResolvedAt = newStatus?.StatusName == RequestStatusNames.Resolved
                    ? request.ResolvedAt ?? now
                    : null;

                if (newStatus != null)
                {
                    statusTransitionName = newStatus.StatusName;
                    await _uow.ServiceRequestTimeline.AddAsync(new ServiceRequestTimeline
                    {
                        RequestId = request.RequestId,
                        StatusName = newStatus.StatusName,
                        ChangedByUserId = author.UserId,
                        ChangedAt = now,
                        Note = $"Status updated to {newStatus.StatusName} through reply."
                    });
                }
            }

            await _uow.SaveChangesAsync();

            var response = new ServiceRequestReplyResponseDto
            {
                ReplyId = reply.ReplyId,
                Author = author.FullName,
                Role = author.Role.ToString(),
                Message = reply.Message,
                CreatedAt = reply.CreatedAt,
                StatusTransition = statusTransitionName
            };

            return Ok("Reply posted successfully.", response);
        }

        public async Task<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>> GetTimelineAsync(int id)
        {
            var requestExists = await _uow.ServiceRequests.AnyAsync(r => r.RequestId == id);
            if (!requestExists)
                return Fail<IEnumerable<ServiceRequestTimelineResponseDto>>("Service request not found.");

            var timeline = await _context.ServiceRequestTimeline
                .AsNoTracking()
                .Where(t => t.RequestId == id)
                .OrderBy(t => t.ChangedAt)
                .Select(t => new ServiceRequestTimelineResponseDto
                {
                    TimelineId = t.TimelineId,
                    Status = t.StatusName,
                    ChangedBy = t.ChangedBy!.FullName,
                    ChangedAt = t.ChangedAt,
                    Note = t.Note
                })
                .ToListAsync();

            return Ok("Timeline fetched successfully.", (IEnumerable<ServiceRequestTimelineResponseDto>)timeline);
        }

        public async Task<ApiResponseDto<ServiceRequestAttachmentResponseDto>> UploadAttachmentAsync(
            IFormFile file, int requestId, int? replyId, int uploadedByUserId, string? ipAddress)
        {
            if (file == null || file.Length == 0)
                return Fail<ServiceRequestAttachmentResponseDto>("No file uploaded.");

            var requestExists = await _uow.ServiceRequests.AnyAsync(r => r.RequestId == requestId);
            if (!requestExists)
                return Fail<ServiceRequestAttachmentResponseDto>("Service request not found.");

            if (replyId.HasValue)
            {
                var replyExists = await _uow.ServiceRequestReplies.AnyAsync(r =>
                    r.ReplyId == replyId.Value && r.RequestId == requestId);

                if (!replyExists)
                    return Fail<ServiceRequestAttachmentResponseDto>("Reply does not belong to the selected service request.");
            }

            var uploader = await _uow.Users.FirstOrDefaultAsync(u =>
                u.UserId == uploadedByUserId && u.Status == UserStatus.Active);

            if (uploader == null)
                return Fail<ServiceRequestAttachmentResponseDto>("No active user is available to upload the attachment.");

            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return Fail<ServiceRequestAttachmentResponseDto>("File size cannot exceed 10 MB.");

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Fail<ServiceRequestAttachmentResponseDto>("File type is not supported.");

            var fileSizeKb = (int)Math.Ceiling(file.Length / 1024.0);
            var savedFileName = $"{Guid.NewGuid():N}{extension}";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, savedFileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{savedFileName}";
            var attachment = new ServiceRequestAttachment
            {
                RequestId = requestId,
                ReplyId = replyId,
                FileName = file.FileName,
                FileSizeKB = fileSizeKb,
                FileUrl = fileUrl,
                UploadedByUserId = uploader.UserId,
                UploadedAt = DateTime.UtcNow
            };

            await _uow.ServiceRequestAttachments.AddAsync(attachment);
            await _uow.SaveChangesAsync();

            var response = new ServiceRequestAttachmentResponseDto
            {
                AttachmentId = attachment.AttachmentId,
                FileName = attachment.FileName,
                FileSizeKB = attachment.FileSizeKB,
                FileUrl = attachment.FileUrl,
                UploadedAt = attachment.UploadedAt
            };

            return Ok("Attachment uploaded successfully.", response);
        }

        public async Task<ApiResponseDto<object>> DeleteRequestAsync(int id, string? ipAddress)
        {
            var request = await _uow.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null)
                return Fail<object>("Service request not found.");

            var now = DateTime.UtcNow;
            request.IsDeleted = true;
            request.DeletedAt = now;
            request.UpdatedAt = now;

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = null,
                Action = "Delete",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request '{request.Title}' ({request.RequestNumber}) soft deleted.",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Service request deleted successfully.", true);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
