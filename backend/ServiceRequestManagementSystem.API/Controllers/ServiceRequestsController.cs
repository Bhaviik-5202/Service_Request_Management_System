using FluentValidation;
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
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>>> GetRequests(
            [FromQuery] int? statusId,
            [FromQuery] Priority? priority,
            [FromQuery] int? serviceTypeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.ServiceRequests
                .Where(r => !r.IsDeleted)
                .Include(r => r.ServiceType)
                .Include(r => r.RequestType)
                .Include(r => r.Department)
                .Include(r => r.Requester)
                .Include(r => r.Assignee)
                .Include(r => r.Status)
                .AsQueryable();

            if (statusId.HasValue)
                query = query.Where(r => r.StatusId == statusId.Value);

            if (priority.HasValue)
                query = query.Where(r => r.Priority == priority.Value);

            if (serviceTypeId.HasValue)
                query = query.Where(r => r.ServiceTypeId == serviceTypeId.Value);

            if (departmentId.HasValue)
                query = query.Where(r => r.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim().ToLower();

                query = query.Where(r =>
                    r.RequestNumber.ToLower().Contains(searchText) ||
                    r.Title.ToLower().Contains(searchText) ||
                    r.Description.ToLower().Contains(searchText) ||
                    r.Requester!.FullName.ToLower().Contains(searchText));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>
            {
                Success = true,
                Message = "Service requests fetched successfully.",
                Data = requests,
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
        public async Task<ActionResult<ApiResponseDto<ServiceRequestDetailResponseDto>>> GetRequestById(int id)
        {
            var request = await _context.ServiceRequests
                .Where(r => !r.IsDeleted)
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
                    Message = "Service request not found."
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

                Replies = request.Replies
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new ServiceRequestReplyResponseDto
                    {
                        ReplyId = r.ReplyId,
                        Author = r.Author!.FullName,
                        Role = r.Author.Role.ToString(),
                        Message = r.Message,
                        CreatedAt = r.CreatedAt,
                        StatusTransition = r.StatusTransition?.StatusName
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
            [FromBody] CreateServiceRequestDto dto)
        {
            var result = await _createValidator.ValidateAsync(dto);

            if (!result.IsValid)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = result.Errors.First().ErrorMessage
                });
            }

            var serviceTypeExists = await _context.ServiceTypes
                .AnyAsync(s =>
                    s.ServiceTypeId == dto.ServiceTypeId &&
                    !s.IsDeleted &&
                    s.IsActive);

            if (!serviceTypeExists)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive service type."
                });
            }

            var requestType = await _context.RequestTypes
                .FirstOrDefaultAsync(r =>
                    r.RequestTypeId == dto.RequestTypeId &&
                    !r.IsDeleted &&
                    r.IsActive);

            if (requestType == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive request type."
                });
            }

            if (requestType.ServiceTypeId != dto.ServiceTypeId)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Request type does not belong to the selected service type."
                });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentId == dto.DepartmentId &&
                    !d.IsDeleted &&
                    d.IsActive);

            if (!departmentExists)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "Invalid or inactive department."
                });
            }

            var requester = await _context.Users
                .FirstOrDefaultAsync(u =>
                    !u.IsDeleted &&
                    u.Status == UserStatus.Active);

            if (requester == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to create the request."
                });
            }

            var initialStatusName = requestType.RequiresApproval
                ? "Pending Approval"
                : "Open";

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == initialStatusName &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = false,
                    Message = $"Required status '{initialStatusName}' was not found."
                });
            }

            var currentYear = DateTime.UtcNow.Year;
            var prefix = $"SR-{currentYear}-";

            var lastRequest = await _context.ServiceRequests
                .Where(r => r.RequestNumber.StartsWith(prefix))
                .OrderByDescending(r => r.RequestId)
                .Select(r => r.RequestNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1001;

            if (!string.IsNullOrEmpty(lastRequest))
            {
                var numberPart = lastRequest.Replace(prefix, "");

                if (int.TryParse(numberPart, out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            var request = new ServiceRequest
            {
                RequestNumber = $"{prefix}{nextNumber}",
                Title = dto.Title,
                Description = dto.Description,
                ServiceTypeId = dto.ServiceTypeId,
                RequestTypeId = dto.RequestTypeId,
                DepartmentId = dto.DepartmentId,
                RequesterUserId = requester.UserId,
                StatusId = status.StatusId,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            if (requestType.RequiresApproval)
            {
                _context.Approvals.Add(new Approval
                {
                    RequestId = request.RequestId,
                    Status = ApprovalStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                });
            }

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = requester.UserId,
                ChangedAt = DateTime.UtcNow,
                Note = requestType.RequiresApproval
                    ? "Request raised and awaiting HOD approval."
                    : "Request raised."
            });

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = requester.UserId,
                Action = "Create",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request '{request.Title}' ({request.RequestNumber}) created.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var response = new ServiceRequestResponseDto
            {
                RequestId = request.RequestId,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                ServiceType = dto.ServiceTypeId.ToString(),
                RequestType = dto.RequestTypeId.ToString(),
                Department = dto.DepartmentId.ToString(),
                Requester = requester.FullName,
                RequesterEmail = requester.Email,
                Status = status.StatusName,
                Priority = request.Priority,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
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
            [FromBody] UpdateServiceRequestStatusDto dto)
        {
            var result = await _statusValidator.ValidateAsync(dto);

            if (!result.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = result.Errors.First().ErrorMessage
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found."
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
                    Message = "Invalid or inactive status."
                });
            }

            request.StatusId = status.StatusId;
            request.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = request.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = string.IsNullOrWhiteSpace(dto.Note)
                    ? $"Status updated to {status.StatusName}."
                    : dto.Note
            });

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = request.RequesterUserId,
                Action = "StatusChange",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Status updated to {status.StatusName}.",
                CreatedAt = DateTime.UtcNow
            });

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
            [FromBody] AssignTechnicianDto dto)
        {
            var result = await _assignValidator.ValidateAsync(dto);

            if (!result.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = result.Errors.First().ErrorMessage
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found."
                });
            }

            var technician = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.AssigneeUserId &&
                    !u.IsDeleted &&
                    u.Status == UserStatus.Active &&
                    u.Role == UserRole.Technician);

            if (technician == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or inactive technician."
                });
            }

            request.AssigneeUserId = technician.UserId;
            request.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = "Assigned",
                ChangedByUserId = request.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = $"Request assigned to {technician.FullName}."
            });

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = request.RequesterUserId,
                Action = "Assign",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Request assigned to technician {technician.FullName}.",
                CreatedAt = DateTime.UtcNow
            });

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
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found."
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == "Cancelled" &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Cancelled status was not found."
                });
            }

            request.StatusId = status.StatusId;
            request.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = request.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = "Service request cancelled."
            });

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = request.RequesterUserId,
                Action = "Cancel",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request {request.RequestNumber} cancelled.",
                CreatedAt = DateTime.UtcNow
            });

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
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Service request not found."
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s =>
                    s.StatusName == "Open" &&
                    s.IsActive);

            if (status == null)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Open status was not found."
                });
            }

            request.StatusId = status.StatusId;
            request.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = request.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = "Service request reopened."
            });

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = request.RequesterUserId,
                Action = "Reopen",
                TargetType = "ServiceRequest",
                TargetId = request.RequestId.ToString(),
                TargetDisplay = request.RequestNumber,
                Detail = $"Service request {request.RequestNumber} reopened.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Service request reopened successfully.",
                Data = true
            });
        }

        [HttpGet("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>>> GetReplies(int id)
        {
            var requestExists = await _context.ServiceRequests
                .AnyAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>
                {
                    Success = false,
                    Message = "Service request not found."
                });
            }

            var replies = await _context.ServiceRequestReplies
                .Where(r => r.RequestId == id)
                .Include(r => r.Author)
                .Include(r => r.StatusTransition)
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
            [FromBody] CreateReplyDto dto)
        {
            var result = await _replyValidator.ValidateAsync(dto);

            if (!result.IsValid)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = result.Errors.First().ErrorMessage
                });
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (request == null)
            {
                return NotFound(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = "Service request not found."
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
                        Message = "Invalid status transition."
                    });
                }
            }

            var author = await _context.Users
                .FirstOrDefaultAsync(u =>
                    !u.IsDeleted &&
                    u.Status == UserStatus.Active);

            if (author == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestReplyResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to post the reply."
                });
            }

            var reply = new ServiceRequestReply
            {
                RequestId = request.RequestId,
                AuthorUserId = author.UserId,
                Message = dto.Message,
                StatusTransitionId = dto.StatusTransitionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequestReplies.Add(reply);

            if (dto.StatusTransitionId.HasValue)
            {
                request.StatusId = dto.StatusTransitionId.Value;
                request.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var response = new ServiceRequestReplyResponseDto
            {
                ReplyId = reply.ReplyId,
                Author = author.FullName,
                Role = author.Role.ToString(),
                Message = reply.Message,
                CreatedAt = reply.CreatedAt
            };

            return Ok(new ApiResponseDto<ServiceRequestReplyResponseDto>
            {
                Success = true,
                Message = "Reply posted successfully.",
                Data = response
            });
        }

        [HttpGet("{id}/timeline")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>>> GetTimeline(int id)
        {
            var requestExists = await _context.ServiceRequests
                .AnyAsync(r =>
                    r.RequestId == id &&
                    !r.IsDeleted);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>
                {
                    Success = false,
                    Message = "Service request not found."
                });
            }

            var timeline = await _context.ServiceRequestTimeline
                .Where(t => t.RequestId == id)
                .Include(t => t.ChangedBy)
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
            [FromForm] int? replyId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "No file uploaded."
                });
            }

            var requestExists = await _context.ServiceRequests
                .AnyAsync(r =>
                    r.RequestId == requestId &&
                    !r.IsDeleted);

            if (!requestExists)
            {
                return NotFound(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "Service request not found."
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
                        Message = "Reply does not belong to the selected service request."
                    });
                }
            }

            var uploader = await _context.Users
                .FirstOrDefaultAsync(u =>
                    !u.IsDeleted &&
                    u.Status == UserStatus.Active);

            if (uploader == null)
            {
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto>
                {
                    Success = false,
                    Message = "No active user is available to upload the attachment."
                });
            }

            var fileSizeKb = (int)Math.Ceiling(file.Length / 1024.0);
            var extension = Path.GetExtension(file.FileName);
            var savedFileName = $"{Guid.NewGuid():N}{extension}";
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
                Message = "Attachment metadata uploaded successfully.",
                Data = response
            });
        }
    }
}
