using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequests/GetServiceRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
        {
            return await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.UpdatedByUser)
                .Include(sr => sr.Replies)
                .Include(sr => sr.Timelines)
                .Include(sr => sr.Attachments)
                .Include(sr => sr.Approval)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        // GET: api/ServiceRequests/GetServiceRequest/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
        {
            var request = await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.UpdatedByUser)
                .Include(sr => sr.Replies)
                .ThenInclude(r => r.AuthorUser)
                .Include(sr => sr.Replies)
                .ThenInclude(r => r.Attachments)
                .Include(sr => sr.Timelines)
                .ThenInclude(t => t.Status)
                .Include(sr => sr.Timelines)
                .ThenInclude(t => t.ChangedByUser)
                .Include(sr => sr.Attachments)
                .ThenInclude(a => a.UploadedByUser)
                .Include(sr => sr.Approval!)
                .ThenInclude(a => a!.DecidedByUser)
                .FirstOrDefaultAsync(sr => sr.RequestId == id);

            if (request == null)
            {
                return NotFound($"Service Request with ID {id} not found.");
            }

            return Ok(request);
        }

        // GET: api/ServiceRequests/GetRequestsByRequester/requester/5
        [HttpGet("requester/{userId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetRequestsByRequester(int userId)
        {
            var requests = await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted && sr.RequesterUserId == userId)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.Approval)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/ServiceRequests/GetRequestsByAssignee/assignee/5
        [HttpGet("assignee/{userId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetRequestsByAssignee(int userId)
        {
            var requests = await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted && sr.AssigneeUserId == userId)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.Approval)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/ServiceRequests/GetRequestsByStatus/status/5
        [HttpGet("status/{statusId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetRequestsByStatus(int statusId)
        {
            var requests = await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted && sr.StatusId == statusId)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.Approval)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/ServiceRequests/GetRequestsByDepartment/department/5
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetRequestsByDepartment(int departmentId)
        {
            var requests = await _context.ServiceRequests
                .Where(sr => !sr.IsDeleted && sr.DepartmentId == departmentId)
                .Include(sr => sr.ServiceType)
                .Include(sr => sr.RequestType)
                .Include(sr => sr.Department)
                .Include(sr => sr.RequesterUser)
                .Include(sr => sr.AssigneeUser)
                .Include(sr => sr.Status)
                .Include(sr => sr.Approval)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // POST: api/ServiceRequests/PostServiceRequest
        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> PostServiceRequest(ServiceRequest request)
        {
            if (request.Description.Trim().Length < 20)
            {
                return BadRequest("Description must be at least 20 characters long.");
            }

            var validPriorities = new[] { "Critical", "High", "Medium", "Low" };
            if (!validPriorities.Contains(request.Priority))
            {
                return BadRequest($"Invalid priority. Valid values: {string.Join(", ", validPriorities)}");
            }

            if (!await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeId == request.ServiceTypeId && !st.IsDeleted))
            {
                return BadRequest($"Service Type with ID {request.ServiceTypeId} does not exist.");
            }

            var requestType = await _context.RequestTypes
                .FirstOrDefaultAsync(rt => rt.RequestTypeId == request.RequestTypeId && !rt.IsDeleted);

            if (requestType == null)
            {
                return BadRequest($"Request Type with ID {request.RequestTypeId} does not exist.");
            }

            if (!await _context.Departments.AnyAsync(d => d.DepartmentId == request.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {request.DepartmentId} does not exist.");
            }

            var requester = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.RequesterUserId && !u.IsDeleted);

            if (requester == null)
            {
                return BadRequest($"Requester with ID {request.RequesterUserId} not found or is deleted.");
            }

            var createdBy = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.CreatedByUserId && !u.IsDeleted);

            if (createdBy == null)
            {
                return BadRequest($"CreatedBy user with ID {request.CreatedByUserId} not found or is deleted.");
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s => s.StatusId == request.StatusId);

            if (status == null)
            {
                return BadRequest($"Status with ID {request.StatusId} does not exist.");
            }

            if (!request.AssigneeUserId.HasValue)
            {
                var mapping = await _context.RequestTypeTechnicianMappings
                    .Where(rm => !rm.IsDeleted && rm.IsActive && rm.RequestTypeId == request.RequestTypeId)
                    .Include(rm => rm.DepartmentPersonnel)
                    .ThenInclude(dp => dp.User)
                    .FirstOrDefaultAsync();

                if (mapping != null && mapping.DepartmentPersonnel.User.IsDeleted == false)
                {
                    request.AssigneeUserId = mapping.DepartmentPersonnel.UserId;
                }
            }

            var year = DateTime.UtcNow.Year;
            var count = await _context.ServiceRequests.CountAsync(sr => sr.CreatedAt.Year == year) + 1;
            request.RequestNumber = $"SR-{year}-{count:D4}";

            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            request.IsDeleted = false;

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            var timeline = new ServiceRequestTimeline
            {
                RequestId = request.RequestId,
                StatusId = request.StatusId,
                ChangedByUserId = request.CreatedByUserId,
                ChangedAt = DateTime.UtcNow,
                Note = $"Request created with status '{status.StatusName}'"
            };

            _context.ServiceRequestTimelines.Add(timeline);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceRequest), new { id = request.RequestId }, request);
        }

        // PUT: api/ServiceRequests/PutServiceRequest/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceRequest(int id, ServiceRequest request)
        {
            if (id != request.RequestId)
            {
                return BadRequest("Request ID mismatch.");
            }

            var existing = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.RequestId == id && !sr.IsDeleted);

            if (existing == null)
            {
                return NotFound($"Service Request with ID {id} not found.");
            }

            if (request.Description.Trim().Length < 20)
            {
                return BadRequest("Description must be at least 20 characters long.");
            }

            var validPriorities = new[] { "Critical", "High", "Medium", "Low" };
            if (!validPriorities.Contains(request.Priority))
            {
                return BadRequest($"Invalid priority. Valid values: {string.Join(", ", validPriorities)}");
            }

            if (!await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeId == request.ServiceTypeId && !st.IsDeleted))
            {
                return BadRequest($"Service Type with ID {request.ServiceTypeId} does not exist.");
            }

            if (!await _context.RequestTypes.AnyAsync(rt => rt.RequestTypeId == request.RequestTypeId && !rt.IsDeleted))
            {
                return BadRequest($"Request Type with ID {request.RequestTypeId} does not exist.");
            }

            if (!await _context.Departments.AnyAsync(d => d.DepartmentId == request.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {request.DepartmentId} does not exist.");
            }

            if (!await _context.Users.AnyAsync(u => u.UserId == request.RequesterUserId && !u.IsDeleted))
            {
                return BadRequest($"Requester with ID {request.RequesterUserId} not found or is deleted.");
            }

            if (!await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusId == request.StatusId))
            {
                return BadRequest($"Status with ID {request.StatusId} does not exist.");
            }

            bool statusChanged = existing.StatusId != request.StatusId;

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.ServiceTypeId = request.ServiceTypeId;
            existing.RequestTypeId = request.RequestTypeId;
            existing.DepartmentId = request.DepartmentId;
            existing.RequesterUserId = request.RequesterUserId;
            existing.AssigneeUserId = request.AssigneeUserId;
            existing.StatusId = request.StatusId;
            existing.Priority = request.Priority;
            existing.UpdatedByUserId = request.UpdatedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (statusChanged)
            {
                var status = await _context.ServiceRequestStatuses.FindAsync(request.StatusId);

                var timeline = new ServiceRequestTimeline
                {
                    RequestId = request.RequestId,
                    StatusId = request.StatusId,
                    ChangedByUserId = request.UpdatedByUserId,
                    ChangedAt = DateTime.UtcNow,
                    Note = $"Status updated to '{status?.StatusName}'"
                };

                _context.ServiceRequestTimelines.Add(timeline);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // DELETE: api/ServiceRequests/DeleteServiceRequest/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id, [FromQuery] int? deletedByUserId)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.RequestId == id && !sr.IsDeleted);

            if (request == null)
            {
                return NotFound($"Service Request with ID {id} not found.");
            }

            request.IsDeleted = true;
            request.DeletedAt = DateTime.UtcNow;
            request.DeletedByUserId = deletedByUserId;
            request.UpdatedAt = DateTime.UtcNow;

            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/ServiceRequests/RestoreServiceRequest/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreServiceRequest(int id)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.RequestId == id && sr.IsDeleted);

            if (request == null)
            {
                return NotFound($"Service Request with ID {id} not found or not deleted.");
            }

            request.IsDeleted = false;
            request.DeletedAt = null;
            request.DeletedByUserId = null;
            request.UpdatedAt = DateTime.UtcNow;

            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}