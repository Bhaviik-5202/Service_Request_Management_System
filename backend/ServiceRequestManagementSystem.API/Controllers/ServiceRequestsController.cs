using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateServiceRequestDto> _createValidator;
        private readonly IValidator<UpdateServiceRequestStatusDto> _statusValidator;
        private readonly IValidator<AssignTechnicianDto> _assignValidator;
        private readonly IValidator<CreateReplyDto> _replyValidator;

        public ServiceRequestsController(
            AppDbContext context,
            IValidator<CreateServiceRequestDto> createValidator,
            IValidator<UpdateServiceRequestStatusDto> statusValidator,
            IValidator<AssignTechnicianDto> assignValidator,
            IValidator<CreateReplyDto> replyValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _statusValidator = statusValidator;
            _assignValidator = assignValidator;
            _replyValidator = replyValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>>> GetRequests()
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
                    Assignee = r.Assignee != null
                        ? r.Assignee.FullName
                        : null,
                    Status = r.Status!.StatusName,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    ResolvedAt = r.ResolvedAt
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>
            {
                Success = true,
                Message = "Service requests fetched successfully.",
                Data = requests
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestDetailResponseDto>>> GetRequestById(int id)
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
            {
                return NotFound(new ApiResponseDto<ServiceRequestDetailResponseDto>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = null
                });
            }

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

            return Ok(new ApiResponseDto<ServiceRequestDetailResponseDto>
            {
                Success = true,
                Message = "Service request fetched successfully.",
                Data = response
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestResponseDto>>> CreateRequest(
            CreateServiceRequestDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var serviceTypeName = await _context.ServiceTypes
                .Where(s =>
                    s.ServiceTypeId == dto.ServiceTypeId &&
                    s.IsActive)
                .Select(s => s.ServiceTypeName)
                .FirstOrDefaultAsync();

            if (serviceTypeName == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive service type.",
                    Data = null
                });
            }

            var requestType = await _context.RequestTypes
                .FirstOrDefaultAsync(r =>
                    r.RequestTypeId == dto.RequestTypeId &&
                    r.IsActive);

            if (requestType == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive request type.",
                    Data = null
                });
            }

            if (requestType.ServiceTypeId != dto.ServiceTypeId)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Request type does not belong to the selected service type.",
                    Data = null
                });
            }

            var departmentName = await _context.Departments
                .Where(d =>
                    d.DepartmentId == dto.DepartmentId &&
                    d.IsActive)
                .Select(d => d.DepartmentName)
                .FirstOrDefaultAsync();

            if (departmentName == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive department.",
                    Data = null
                });
            }

            var requester = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.RequesterUserId &&
                    u.Status == UserStatus.Active);

            if (requester == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to create the request.",
                    Data = null
                });
            }

            var initialStatusName = requestType.RequiresApproval
                ? RequestStatusNames.PendingApproval
                : RequestStatusNames.Open;

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == initialStatusName &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = $"Required status '{initialStatusName}' was not found.",
                    Data = null
                });
            }

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

            _context.ServiceRequests.Add(request);

            await _context.SaveChangesAsync();

            request.RequestNumber = $"SR-{now:yyyy}-{request.RequestId:D6}";

            if (requestType.RequiresApproval)
            {
                _context.Approvals.Add(new Approval
                {
                    RequestId = request.RequestId,
                    Status = ApprovalStatus.Pending,
                    SubmittedAt = now
                });
            }

            AddTimeline(
                request,
                status.StatusName,
                requester.UserId,
                now,
                requestType.RequiresApproval
                    ? "Request raised and awaiting HOD approval."
                    : "Request raised.");

            AddRequestAuditLog(
                request,
                "Create",
                $"Service request '{request.Title}' ({request.RequestNumber}) created.",
                now);

            await _context.SaveChangesAsync();

            var response = new ServiceRequestResponseDto
            {
                RequestId = request.RequestId,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                ServiceType = serviceTypeName,
                RequestType = requestType.RequestTypeName,
                Department = departmentName,
                Requester = requester.FullName,
                RequesterEmail = requester.Email,
                Status = status.StatusName,
                Priority = request.Priority,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ResolvedAt = request.ResolvedAt
            };

            return CreatedAtAction(
                nameof(GetRequestById),
                new { id = request.RequestId },
                new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = true,
                    Message = "Service request created successfully.",
                    Data = response
                });
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateStatus(
            int id,
            UpdateServiceRequestStatusDto dto)
        {
            var validation = await _statusValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = false
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = false
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusId == dto.StatusId &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or inactive status.",
                    Data = false
                });
            }

            var updatedAt = DateTime.UtcNow;

            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = status.StatusName == RequestStatusNames.Resolved
                ? request.ResolvedAt ?? updatedAt
                : null;

            AddTimeline(
                request,
                status.StatusName,
                request.RequesterUserId,
                updatedAt,
                string.IsNullOrWhiteSpace(dto.Note)
                    ? $"Status updated to {status.StatusName}."
                    : dto.Note);

            AddRequestAuditLog(
                request,
                "StatusChange",
                $"Status updated to {status.StatusName}.",
                updatedAt);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Status updated successfully.",
                Data = true
            });
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignTechnician(
            int id,
            AssignTechnicianDto dto)
        {
            var validation = await _assignValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = false
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = false
                });
            }

            var technician = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.AssigneeUserId &&
                    u.Status == UserStatus.Active &&
                    u.Role == UserRole.Technician);

            if (technician == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or inactive technician.",
                    Data = false
                });
            }

            var updatedAt = DateTime.UtcNow;

            request.AssigneeUserId = technician.UserId;
            request.UpdatedAt = updatedAt;

            AddTimeline(
                request,
                RequestStatusNames.Assigned,
                request.RequesterUserId,
                updatedAt,
                $"Request assigned to {technician.FullName}.");

            AddRequestAuditLog(
                request,
                "Assign",
                $"Request assigned to technician {technician.FullName}.",
                updatedAt);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Technician assigned successfully.",
                Data = true
            });
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelRequest(int id)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = false
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == RequestStatusNames.Cancelled &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Cancelled status was not found.",
                    Data = false
                });
            }

            var updatedAt = DateTime.UtcNow;

            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = null;

            AddTimeline(
                request,
                status.StatusName,
                request.RequesterUserId,
                updatedAt,
                "Service request cancelled.");

            AddRequestAuditLog(
                request,
                "Cancel",
                $"Service request {request.RequestNumber} cancelled.",
                updatedAt);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Service request cancelled successfully.",
                Data = true
            });
        }

        [HttpPut("{id}/reopen")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ReopenRequest(int id)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = false
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == RequestStatusNames.Open &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Open status was not found.",
                    Data = false
                });
            }

            var updatedAt = DateTime.UtcNow;

            request.StatusId = status.StatusId;
            request.UpdatedAt = updatedAt;
            request.ResolvedAt = null;

            AddTimeline(
                request,
                status.StatusName,
                request.RequesterUserId,
                updatedAt,
                "Service request reopened.");

            AddRequestAuditLog(
                request,
                "Reopen",
                $"Service request {request.RequestNumber} reopened.",
                updatedAt);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Service request reopened successfully.",
                Data = true
            });
        }

        [HttpGet("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>>> GetReplies(
            int id)
        {
            var requestExists = await _context.ServiceRequests
                .AnyAsync(r => r.RequestId == id);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = null
                });
            }

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
                    StatusTransition = r.StatusTransition != null
                        ? r.StatusTransition.StatusName
                        : null
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>
            {
                Success = true,
                Message = "Replies fetched successfully.",
                Data = replies
            });
        }

        [HttpPost("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestReplyResponseDto>>> PostReply(
            int id,
            CreateReplyDto dto)
        {
            var validation = await _replyValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = null
                });
            }

            if (dto.StatusTransitionId.HasValue)
            {
                var statusExists = await _context.ServiceRequestStatuses
                    .AnyAsync(s =>
                        s.StatusId == dto.StatusTransitionId.Value &&
                        s.IsActive);

                if (!statusExists)
                {
                    return BadRequest(new ApiResponseDto<ServiceRequestReplyResponseDto>
                    {
                        Success = false,
                        Message = "Invalid status transition.",
                        Data = null
                    });
                }
            }

            var author = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.AuthorUserId &&
                    u.Status == UserStatus.Active);

            if (author == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to post the reply.",
                    Data = null
                });
            }

            var reply = new ServiceRequestReply
            {
                RequestId = request.RequestId,
                AuthorUserId = author.UserId,
                Message = dto.Message.Trim(),
                StatusTransitionId = dto.StatusTransitionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequestReplies.Add(reply);

            if (dto.StatusTransitionId.HasValue)
            {
                var newStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s =>
                        s.StatusId == dto.StatusTransitionId.Value &&
                        s.IsActive);

                var updatedAt = DateTime.UtcNow;

                request.StatusId = dto.StatusTransitionId.Value;
                request.UpdatedAt = updatedAt;
                request.ResolvedAt = newStatus?.StatusName == RequestStatusNames.Resolved
                    ? request.ResolvedAt ?? updatedAt
                    : null;

                if (newStatus != null)
                {
                    AddTimeline(
                        request,
                        newStatus.StatusName,
                        author.UserId,
                        updatedAt,
                        $"Status updated to {newStatus.StatusName} through reply.");
                }
            }

            await _context.SaveChangesAsync();

            var statusTransitionName = dto.StatusTransitionId.HasValue
                ? await _context.ServiceRequestStatuses
                    .Where(s => s.StatusId == dto.StatusTransitionId.Value)
                    .Select(s => s.StatusName)
                    .FirstOrDefaultAsync()
                : null;

            var response = new ServiceRequestReplyResponseDto
            {
                ReplyId = reply.ReplyId,
                Author = author.FullName,
                Role = author.Role.ToString(),
                Message = reply.Message,
                CreatedAt = reply.CreatedAt,
                StatusTransition = statusTransitionName
            };

            return Ok(new ApiResponseDto<ServiceRequestReplyResponseDto>
            {
                Success = true,
                Message = "Reply posted successfully.",
                Data = response
            });
        }

        [HttpGet("{id}/timeline")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>>> GetTimeline(
            int id)
        {
            var requestExists = await _context.ServiceRequests
                .AnyAsync(r => r.RequestId == id);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = null
                });
            }

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

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>
            {
                Success = true,
                Message = "Timeline fetched successfully.",
                Data = timeline
            });
        }

        [HttpPost("attachments/upload")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestAttachmentResponseDto>>> UploadAttachment(
            [FromForm] IFormFile file,
            [FromForm] int requestId,
            [FromForm] int? replyId,
            [FromForm] int uploadedByUserId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "No file uploaded.",
                    Data = null
                });
            }

            var requestExists = await _context.ServiceRequests
                .AnyAsync(r => r.RequestId == requestId);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "Service request not found.",
                    Data = null
                });
            }

            if (replyId.HasValue)
            {
                var replyExists = await _context.ServiceRequestReplies
                    .AnyAsync(r =>
                        r.ReplyId == replyId.Value &&
                        r.RequestId == requestId);

                if (!replyExists)
                {
                    return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                    {
                        Success = false,
                        Message = "Reply does not belong to the selected service request.",
                        Data = null
                    });
                }
            }

            var uploader = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == uploadedByUserId &&
                    u.Status == UserStatus.Active);

            if (uploader == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to upload the attachment.",
                    Data = null
                });
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "File size cannot exceed 10 MB.",
                    Data = null
                });
            }

            var allowedExtensions = new[]
            {
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".png",
                ".jpg",
                ".jpeg"
            };

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "File type is not supported.",
                    Data = null
                });
            }

            var fileSizeKb = (int)Math.Ceiling(file.Length / 1024.0);
            var savedFileName = $"{Guid.NewGuid():N}{extension}";

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(
                uploadsFolder,
                savedFileName);

            await using (var stream = new FileStream(
                filePath,
                FileMode.Create))
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

            _context.ServiceRequestAttachments.Add(attachment);

            await _context.SaveChangesAsync();

            var response = new ServiceRequestAttachmentResponseDto
            {
                AttachmentId = attachment.AttachmentId,
                FileName = attachment.FileName,
                FileSizeKB = attachment.FileSizeKB,
                FileUrl = attachment.FileUrl,
                UploadedAt = attachment.UploadedAt
            };

            return Ok(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
            {
                Success = true,
                Message = "Attachment uploaded successfully.",
                Data = response
            });
        }

        private void AddTimeline(
            ServiceRequest request,
            string statusName,
            int changedByUserId,
            DateTime changedAt,
            string note)
        {
            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = statusName,
                ChangedByUserId = changedByUserId,
                ChangedAt = changedAt,
                Note = note
            });
        }

        private void AddRequestAuditLog(
            ServiceRequest request,
            string action,
            string detail,
            DateTime createdAt)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = null,
                Action = action,
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = detail,
                CreatedAt = createdAt
            });
        }
    }
}
