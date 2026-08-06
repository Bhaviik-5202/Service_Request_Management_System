using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestAttachmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestAttachmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequestAttachments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestAttachment>>> GetAttachments()
        {
            try
            {
                return await _context.ServiceRequestAttachments
                    .Include(a => a.ServiceRequest)
                    .Include(a => a.Reply)
                    .Include(a => a.UploadedByUser)
                    .OrderByDescending(a => a.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestAttachments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestAttachment>> GetAttachment(int id)
        {
            try
            {
                var attachment = await _context.ServiceRequestAttachments
                    .Include(a => a.ServiceRequest)
                    .Include(a => a.Reply)
                    .Include(a => a.UploadedByUser)
                    .FirstOrDefaultAsync(a => a.AttachmentId == id);

                if (attachment == null)
                {
                    return NotFound($"Attachment with ID {id} not found.");
                }

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestAttachments/request/5
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestAttachment>>> GetAttachmentsByRequest(int requestId)
        {
            try
            {
                var attachments = await _context.ServiceRequestAttachments
                    .Where(a => a.RequestId == requestId)
                    .Include(a => a.Reply)
                    .Include(a => a.UploadedByUser)
                    .OrderByDescending(a => a.UploadedAt)
                    .ToListAsync();

                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestAttachments/reply/5
        [HttpGet("reply/{replyId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestAttachment>>> GetAttachmentsByReply(int replyId)
        {
            try
            {
                var attachments = await _context.ServiceRequestAttachments
                    .Where(a => a.ReplyId == replyId)
                    .Include(a => a.ServiceRequest)
                    .Include(a => a.UploadedByUser)
                    .OrderByDescending(a => a.UploadedAt)
                    .ToListAsync();

                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestAttachments/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestAttachment>>> GetAttachmentsByUser(int userId)
        {
            try
            {
                var attachments = await _context.ServiceRequestAttachments
                    .Where(a => a.UploadedByUserId == userId)
                    .Include(a => a.ServiceRequest)
                    .Include(a => a.Reply)
                    .OrderByDescending(a => a.UploadedAt)
                    .ToListAsync();

                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ServiceRequestAttachments
        [HttpPost]
        public async Task<ActionResult<ServiceRequestAttachment>> PostAttachment(ServiceRequestAttachment attachment)
        {
            try
            {
                // Validate ServiceRequest exists
                var serviceRequest = await _context.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.RequestId == attachment.RequestId && !sr.IsDeleted);

                if (serviceRequest == null)
                {
                    return BadRequest($"Service Request with ID {attachment.RequestId} not found.");
                }

                // Validate Reply exists if provided
                if (attachment.ReplyId.HasValue)
                {
                    if (!await _context.ServiceRequestReplies.AnyAsync(r => r.ReplyId == attachment.ReplyId))
                    {
                        return BadRequest($"Reply with ID {attachment.ReplyId} does not exist.");
                    }
                }

                // Validate UploadedByUser exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == attachment.UploadedByUserId && !u.IsDeleted);

                if (user == null)
                {
                    return BadRequest($"User with ID {attachment.UploadedByUserId} not found or is deleted.");
                }

                // Validate FileSize
                if (attachment.FileSizeKB <= 0)
                {
                    return BadRequest("File size must be greater than 0 KB.");
                }

                // Max file size: 10MB (10240 KB)
                if (attachment.FileSizeKB > 10240)
                {
                    return BadRequest("File size cannot exceed 10 MB.");
                }

                attachment.UploadedAt = DateTime.UtcNow;

                _context.ServiceRequestAttachments.Add(attachment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAttachment), new { id = attachment.AttachmentId }, attachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceRequestAttachments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            try
            {
                var attachment = await _context.ServiceRequestAttachments
                    .FirstOrDefaultAsync(a => a.AttachmentId == id);

                if (attachment == null)
                {
                    return NotFound($"Attachment with ID {id} not found.");
                }

                _context.ServiceRequestAttachments.Remove(attachment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> AttachmentExistsAsync(int id)
        {
            return await _context.ServiceRequestAttachments.AnyAsync(a => a.AttachmentId == id);
        }
    }
}