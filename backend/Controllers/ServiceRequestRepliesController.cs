using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestRepliesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestRepliesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequestReplies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestReply>>> GetReplies()
        {
            try
            {
                return await _context.ServiceRequestReplies
                    .Include(r => r.ServiceRequest)
                    .Include(r => r.AuthorUser)
                    .Include(r => r.StatusTransition)
                    .Include(r => r.Attachments)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestReplies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestReply>> GetReply(int id)
        {
            try
            {
                var reply = await _context.ServiceRequestReplies
                    .Include(r => r.ServiceRequest)
                    .Include(r => r.AuthorUser)
                    .Include(r => r.StatusTransition)
                    .Include(r => r.Attachments)
                    .FirstOrDefaultAsync(r => r.ReplyId == id);

                if (reply == null)
                {
                    return NotFound($"Reply with ID {id} not found.");
                }

                return Ok(reply);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestReplies/request/5
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestReply>>> GetRepliesByRequest(int requestId)
        {
            try
            {
                var replies = await _context.ServiceRequestReplies
                    .Where(r => r.RequestId == requestId)
                    .Include(r => r.AuthorUser)
                    .Include(r => r.StatusTransition)
                    .Include(r => r.Attachments)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(replies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestReplies/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestReply>>> GetRepliesByUser(int userId)
        {
            try
            {
                var replies = await _context.ServiceRequestReplies
                    .Where(r => r.AuthorUserId == userId)
                    .Include(r => r.ServiceRequest)
                    .Include(r => r.StatusTransition)
                    .Include(r => r.Attachments)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(replies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ServiceRequestReplies
        [HttpPost]
        public async Task<ActionResult<ServiceRequestReply>> PostReply(ServiceRequestReply reply)
        {
            try
            {
                // Validate ServiceRequest exists
                var serviceRequest = await _context.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.RequestId == reply.RequestId && !sr.IsDeleted);

                if (serviceRequest == null)
                {
                    return BadRequest($"Service Request with ID {reply.RequestId} not found.");
                }

                // Validate Author exists
                var author = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == reply.AuthorUserId && !u.IsDeleted);

                if (author == null)
                {
                    return BadRequest($"Author with ID {reply.AuthorUserId} not found or is deleted.");
                }

                // Validate StatusTransition exists if provided
                if (reply.StatusTransitionId.HasValue)
                {
                    if (!await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusId == reply.StatusTransitionId))
                    {
                        return BadRequest($"Status with ID {reply.StatusTransitionId} does not exist.");
                    }
                }

                reply.CreatedAt = DateTime.UtcNow;

                _context.ServiceRequestReplies.Add(reply);
                await _context.SaveChangesAsync();

                // Update the service request's UpdatedAt timestamp
                serviceRequest.UpdatedAt = DateTime.UtcNow;
                _context.Entry(serviceRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetReply), new { id = reply.ReplyId }, reply);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/ServiceRequestReplies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReply(int id, ServiceRequestReply reply)
        {
            if (id != reply.ReplyId)
            {
                return BadRequest("Reply ID mismatch.");
            }

            try
            {
                var existing = await _context.ServiceRequestReplies
                    .FirstOrDefaultAsync(r => r.ReplyId == id);

                if (existing == null)
                {
                    return NotFound($"Reply with ID {id} not found.");
                }

                // Only allow updating message - other fields should remain as original
                existing.Message = reply.Message;
                existing.CreatedAt = DateTime.UtcNow;

                _context.Entry(existing).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ReplyExistsAsync(id))
                {
                    return NotFound($"Reply with ID {id} not found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceRequestReplies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReply(int id)
        {
            try
            {
                var reply = await _context.ServiceRequestReplies
                    .Include(r => r.Attachments)
                    .FirstOrDefaultAsync(r => r.ReplyId == id);

                if (reply == null)
                {
                    return NotFound($"Reply with ID {id} not found.");
                }

                // Check if reply has attachments
                if (reply.Attachments.Any())
                {
                    return BadRequest($"Cannot delete reply with {reply.Attachments.Count} attachments. Delete attachments first.");
                }

                _context.ServiceRequestReplies.Remove(reply);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> ReplyExistsAsync(int id)
        {
            return await _context.ServiceRequestReplies.AnyAsync(r => r.ReplyId == id);
        }
    }
}