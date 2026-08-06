using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestTimelineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestTimelineController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequestTimeline
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestTimeline>>> GetTimelines()
        {
            try
            {
                return await _context.ServiceRequestTimelines
                    .Include(t => t.ServiceRequest)
                    .Include(t => t.Status)
                    .Include(t => t.ChangedByUser)
                    .OrderByDescending(t => t.ChangedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestTimeline/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestTimeline>> GetTimeline(int id)
        {
            try
            {
                var timeline = await _context.ServiceRequestTimelines
                    .Include(t => t.ServiceRequest)
                    .Include(t => t.Status)
                    .Include(t => t.ChangedByUser)
                    .FirstOrDefaultAsync(t => t.TimelineId == id);

                if (timeline == null)
                {
                    return NotFound($"Timeline entry with ID {id} not found.");
                }

                return Ok(timeline);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestTimeline/request/5
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestTimeline>>> GetTimelinesByRequest(int requestId)
        {
            try
            {
                var timelines = await _context.ServiceRequestTimelines
                    .Where(t => t.RequestId == requestId)
                    .Include(t => t.Status)
                    .Include(t => t.ChangedByUser)
                    .OrderBy(t => t.ChangedAt)
                    .ToListAsync();

                return Ok(timelines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestTimeline/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestTimeline>>> GetTimelinesByUser(int userId)
        {
            try
            {
                var timelines = await _context.ServiceRequestTimelines
                    .Where(t => t.ChangedByUserId == userId)
                    .Include(t => t.ServiceRequest)
                    .Include(t => t.Status)
                    .OrderByDescending(t => t.ChangedAt)
                    .ToListAsync();

                return Ok(timelines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ServiceRequestTimeline
        [HttpPost]
        public async Task<ActionResult<ServiceRequestTimeline>> PostTimeline(ServiceRequestTimeline timeline)
        {
            try
            {
                // Validate ServiceRequest exists
                var serviceRequest = await _context.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.RequestId == timeline.RequestId && !sr.IsDeleted);

                if (serviceRequest == null)
                {
                    return BadRequest($"Service Request with ID {timeline.RequestId} not found.");
                }

                // Validate Status exists
                if (!await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusId == timeline.StatusId))
                {
                    return BadRequest($"Status with ID {timeline.StatusId} does not exist.");
                }

                // Validate ChangedByUser exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == timeline.ChangedByUserId && !u.IsDeleted);

                if (user == null)
                {
                    return BadRequest($"User with ID {timeline.ChangedByUserId} not found or is deleted.");
                }

                // Note is required and max length is 500
                if (string.IsNullOrWhiteSpace(timeline.Note))
                {
                    return BadRequest("Note is required.");
                }

                if (timeline.Note.Length > 500)
                {
                    return BadRequest("Note cannot exceed 500 characters.");
                }

                timeline.ChangedAt = DateTime.UtcNow;

                _context.ServiceRequestTimelines.Add(timeline);
                await _context.SaveChangesAsync();

                // Update the service request's status
                serviceRequest.StatusId = timeline.StatusId;
                serviceRequest.UpdatedAt = DateTime.UtcNow;
                _context.Entry(serviceRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTimeline), new { id = timeline.TimelineId }, timeline);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceRequestTimeline/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeline(int id)
        {
            try
            {
                var timeline = await _context.ServiceRequestTimelines
                    .FirstOrDefaultAsync(t => t.TimelineId == id);

                if (timeline == null)
                {
                    return NotFound($"Timeline entry with ID {id} not found.");
                }

                _context.ServiceRequestTimelines.Remove(timeline);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> TimelineExistsAsync(int id)
        {
            return await _context.ServiceRequestTimelines.AnyAsync(t => t.TimelineId == id);
        }
    }
}