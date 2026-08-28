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
    [Route("api/v1/requests")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestsController(AppDbContext context)
        {
            _context = context;
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
            var query = _context.ServiceRequests
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.Requester)
                .Include(sr => sr.Assignee)
                .Include(sr => sr.Status)
                .AsQueryable();

            if (statusId.HasValue)
                query = query.Where(sr => sr.StatusId == statusId.Value);

            if (priority.HasValue)
                query = query.Where(sr => sr.Priority == priority.Value);

            if (serviceTypeId.HasValue)
                query = query.Where(sr => sr.ServiceTypeId == serviceTypeId.Value);

            if (departmentId.HasValue)
                query = query.Where(sr => sr.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(sr => sr.RequestNumber.ToLower().Contains(s) ||
                                          sr.Title.ToLower().Contains(s) ||
                                          sr.Description.ToLower().Contains(s) ||
                                          sr.Requester!.FullName.ToLower().Contains(s));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(sr => sr.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sr => new ServiceRequestResponseDto
                {
                    RequestId = sr.RequestId,
                    RequestNumber = sr.RequestNumber,
                    Title = sr.Title,
                    Description = sr.Description,
                    ServiceType = sr.ServiceType!.ServiceTypeName,
                    RequestType = sr.RequestType!.RequestTypeName,
                    Department = sr.Department!.DepartmentName,
                    Requester = sr.Requester!.FullName,
                    RequesterEmail = sr.Requester.Email,
                    Assignee = sr.Assignee != null ? sr.Assignee.FullName : null,
                    Status = sr.Status!.StatusName,
                    Priority = sr.Priority,
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>
            {
                Success = true,
                Message = "Service requests fetched successfully.",
                Data = items,
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
            var sr = await _context.ServiceRequests
                .Include(r => r.ServiceType)
                .Include(r => r.RequestType)
                .Include(r => r.Department)
                .Include(r => r.Requester)
                .Include(r => r.Assignee)
                .Include(r => r.Status)
                .Include(r => r.Replies).ThenInclude(rep => rep.Author)
                .Include(r => r.Replies).ThenInclude(rep => rep.StatusTransition)
                .Include(r => r.TimelineEntries).ThenInclude(t => t.ChangedBy)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (sr == null)
                return NotFound(new ApiResponseDto<ServiceRequestDetailResponseDto> { Success = false, Message = "Service request not found." });

            var detail = new ServiceRequestDetailResponseDto
            {
                RequestId = sr.RequestId,
                RequestNumber = sr.RequestNumber,
                Title = sr.Title,
                Description = sr.Description,
                ServiceType = sr.ServiceType!.ServiceTypeName,
                RequestType = sr.RequestType!.RequestTypeName,
                Department = sr.Department!.DepartmentName,
                Requester = sr.Requester!.FullName,
                RequesterEmail = sr.Requester.Email,
                Assignee = sr.Assignee?.FullName,
                Status = sr.Status!.StatusName,
                Priority = sr.Priority,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt,
                Replies = sr.Replies.OrderBy(r => r.CreatedAt).Select(rep => new ServiceRequestReplyResponseDto
                {
                    ReplyId = rep.ReplyId,
                    Author = rep.Author!.FullName,
                    Role = rep.Author.Role.ToString(),
                    Message = rep.Message,
                    CreatedAt = rep.CreatedAt,
                    StatusTransition = rep.StatusTransition?.StatusName
                }).ToList(),
                Timeline = sr.TimelineEntries.OrderBy(t => t.ChangedAt).Select(t => new ServiceRequestTimelineResponseDto
                {
                    TimelineId = t.TimelineId,
                    Status = t.StatusName,
                    ChangedBy = t.ChangedBy!.FullName,
                    ChangedAt = t.ChangedAt,
                    Note = t.Note
                }).ToList(),
                Attachments = sr.Attachments.Select(a => new ServiceRequestAttachmentResponseDto
                {
                    AttachmentId = a.AttachmentId,
                    FileName = a.FileName,
                    FileSizeKB = a.FileSizeKB,
                    FileUrl = a.FileUrl,
                    UploadedAt = a.UploadedAt
                }).ToList()
            };

            return Ok(new ApiResponseDto<ServiceRequestDetailResponseDto> { Success = true, Data = detail });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestResponseDto>>> CreateRequest([FromBody] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto> { Success = false, Message = "Validation failed." });

            var currentYear = DateTime.UtcNow.Year;
            var yearPrefix = $"SR-{currentYear}-";
            var countThisYear = await _context.ServiceRequests.CountAsync(sr => sr.RequestNumber.StartsWith(yearPrefix));
            var requestNo = $"{yearPrefix}{(1000 + countThisYear + 1)}";

            var reqType = await _context.RequestTypes.FindAsync(dto.RequestTypeId);
            if (reqType == null)
                return BadRequest(new ApiResponseDto<ServiceRequestResponseDto> { Success = false, Message = "Invalid Request Type." });

            // Status resolution: if requires approval -> Pending Approval (ID 3), else Open (ID 1)
            var initialStatusName = reqType.RequiresApproval ? "Pending Approval" : "Open";
            var status = await _context.ServiceRequestStatuses.FirstOrDefaultAsync(s => s.StatusName == initialStatusName)
                         ?? await _context.ServiceRequestStatuses.FirstAsync();

            // Default requester set to first active user (will be populated from User context in auth phase)
            var defaultRequester = await _context.Users.FirstOrDefaultAsync() ?? new User { UserId = 1 };

            var sr = new ServiceRequest
            {
                RequestNumber = requestNo,
                Title = dto.Title,
                Description = dto.Description,
                ServiceTypeId = dto.ServiceTypeId,
                RequestTypeId = dto.RequestTypeId,
                DepartmentId = dto.DepartmentId,
                RequesterUserId = defaultRequester.UserId,
                StatusId = status.StatusId,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();

            // Create HOD Approval record if approval is required
            if (reqType.RequiresApproval)
            {
                _context.Approvals.Add(new Approval
                {
                    RequestId = sr.RequestId,
                    Status = ApprovalStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                });
            }

            // Create initial timeline entry
            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = sr.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = defaultRequester.UserId,
                ChangedAt = DateTime.UtcNow,
                Note = reqType.RequiresApproval ? "Request raised and awaiting HOD approval" : "Request raised"
            });

            await _context.SaveChangesAsync();

            var response = new ServiceRequestResponseDto
            {
                RequestId = sr.RequestId,
                RequestNumber = sr.RequestNumber,
                Title = sr.Title,
                Description = sr.Description,
                Status = status.StatusName,
                Priority = sr.Priority,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            };

            return CreatedAtAction(nameof(GetRequestById), new { id = sr.RequestId }, new ApiResponseDto<ServiceRequestResponseDto> { Success = true, Message = "Service request created successfully.", Data = response });
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateStatus(int id, [FromBody] UpdateServiceRequestStatusDto dto)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Request not found." });

            var status = await _context.ServiceRequestStatuses.FindAsync(dto.StatusId);
            if (status == null)
                return BadRequest(new ApiResponseDto<bool> { Success = false, Message = "Invalid status ID." });

            sr.StatusId = status.StatusId;
            sr.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = sr.RequestId,
                StatusName = status.StatusName,
                ChangedByUserId = sr.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = dto.Note ?? $"Status updated to {status.StatusName}"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponseDto<bool> { Success = true, Message = "Status updated successfully.", Data = true });
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignTechnician(int id, [FromBody] AssignTechnicianDto dto)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Request not found." });

            var tech = await _context.Users.FindAsync(dto.AssigneeUserId);
            if (tech == null)
                return BadRequest(new ApiResponseDto<bool> { Success = false, Message = "Invalid technician ID." });

            sr.AssigneeUserId = tech.UserId;
            sr.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = sr.RequestId,
                StatusName = "Assigned",
                ChangedByUserId = sr.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = $"Assigned to technician {tech.FullName}"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponseDto<bool> { Success = true, Message = $"Assigned to {tech.FullName}.", Data = true });
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelRequest(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Request not found." });

            var cancelStatus = await _context.ServiceRequestStatuses.FirstOrDefaultAsync(s => s.StatusName == "Cancelled")
                                ?? await _context.ServiceRequestStatuses.FirstOrDefaultAsync(s => s.StatusName == "Closed")
                                ?? await _context.ServiceRequestStatuses.FirstAsync();

            sr.StatusId = cancelStatus.StatusId;
            sr.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = sr.RequestId,
                StatusName = cancelStatus.StatusName,
                ChangedByUserId = sr.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = "Ticket cancelled by requester"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponseDto<bool> { Success = true, Message = "Request cancelled successfully.", Data = true });
        }

        [HttpPut("{id}/reopen")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ReopenRequest(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Request not found." });

            var inProgressStatus = await _context.ServiceRequestStatuses.FirstOrDefaultAsync(s => s.StatusName == "In Progress")
                                    ?? await _context.ServiceRequestStatuses.FirstAsync();

            sr.StatusId = inProgressStatus.StatusId;
            sr.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequestTimeline.Add(new ServiceRequestTimeline
            {
                RequestId = sr.RequestId,
                StatusName = inProgressStatus.StatusName,
                ChangedByUserId = sr.RequesterUserId,
                ChangedAt = DateTime.UtcNow,
                Note = "Ticket reopened by requester"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponseDto<bool> { Success = true, Message = "Request reopened successfully.", Data = true });
        }

        [HttpGet("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>>> GetReplies(int id)
        {
            var replies = await _context.ServiceRequestReplies
                .Include(r => r.Author)
                .Include(r => r.StatusTransition)
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

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>> { Success = true, Data = replies });
        }

        [HttpPost("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestReplyResponseDto>>> PostReply(int id, [FromBody] CreateReplyDto dto)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null)
                return NotFound(new ApiResponseDto<ServiceRequestReplyResponseDto> { Success = false, Message = "Request not found." });

            var author = await _context.Users.FirstOrDefaultAsync() ?? new User { UserId = 1, FullName = "User" };

            var reply = new ServiceRequestReply
            {
                RequestId = id,
                AuthorUserId = author.UserId,
                Message = dto.Message,
                StatusTransitionId = dto.StatusTransitionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequestReplies.Add(reply);

            if (dto.StatusTransitionId.HasValue)
            {
                sr.StatusId = dto.StatusTransitionId.Value;
                sr.UpdatedAt = DateTime.UtcNow;
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

            return Ok(new ApiResponseDto<ServiceRequestReplyResponseDto> { Success = true, Message = "Reply posted successfully.", Data = response });
        }

        [HttpGet("{id}/timeline")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>>> GetTimeline(int id)
        {
            var timeline = await _context.ServiceRequestTimeline
                .Include(t => t.ChangedBy)
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

            return Ok(new ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>> { Success = true, Data = timeline });
        }

        [HttpPost("attachments/upload")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestAttachmentResponseDto>>> UploadAttachment(
            [FromForm] IFormFile file,
            [FromForm] int requestId,
            [FromForm] int? replyId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponseDto<ServiceRequestAttachmentResponseDto> { Success = false, Message = "No file uploaded." });

            var sr = await _context.ServiceRequests.FindAsync(requestId);
            if (sr == null)
                return NotFound(new ApiResponseDto<ServiceRequestAttachmentResponseDto> { Success = false, Message = "Associated service request not found." });

            var user = await _context.Users.FirstOrDefaultAsync() ?? new User { UserId = 1 };

            var fileSizeKb = (int)Math.Ceiling(file.Length / 1024.0);
            var fileGuid = Guid.NewGuid().ToString("N");
            var extension = Path.GetExtension(file.FileName);
            var savedFileName = $"{fileGuid}{extension}";
            var fileUrl = $"/uploads/{savedFileName}";

            var attachment = new ServiceRequestAttachment
            {
                RequestId = requestId,
                ReplyId = replyId,
                FileName = file.FileName,
                FileSizeKB = fileSizeKb,
                FileUrl = fileUrl,
                UploadedByUserId = user.UserId,
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
                Message = "File metadata uploaded successfully.",
                Data = response
            });
        }
    }
}
