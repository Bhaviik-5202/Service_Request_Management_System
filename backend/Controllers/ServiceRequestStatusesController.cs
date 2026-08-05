using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ServiceRequestStatusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestStatusesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequestStatuses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestStatus>>> GetServiceRequestStatuses()
        {
            try
            {
                return await _context.ServiceRequestStatuses
                    .Include(srs => srs.ServiceRequests)
                    .Include(srs => srs.ServiceRequestReplies)
                    .Include(srs => srs.ServiceRequestTimelines)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceRequestStatuses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestStatus>> GetServiceRequestStatus(int id)
        {
            try
            {
                var status = await _context.ServiceRequestStatuses
                    .Include(srs => srs.ServiceRequests)
                    .Include(srs => srs.ServiceRequestReplies)
                    .Include(srs => srs.ServiceRequestTimelines)
                    .FirstOrDefaultAsync(srs => srs.StatusId == id);

                if (status == null)
                {
                    return NotFound($"Status with ID {id} not found.");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ServiceRequestStatuses
        [HttpPost]
        public async Task<ActionResult<ServiceRequestStatus>> PostServiceRequestStatus(ServiceRequestStatus status)
        {
            try
            {
                if (await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusName == status.StatusName))
                {
                    return BadRequest($"Status '{status.StatusName}' already exists.");
                }

                status.CreatedAt = DateTime.UtcNow;

                _context.ServiceRequestStatuses.Add(status);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServiceRequestStatus), new { id = status.StatusId }, status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/ServiceRequestStatuses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceRequestStatus(int id, ServiceRequestStatus status)
        {
            if (id != status.StatusId)
            {
                return BadRequest("Status ID mismatch.");
            }

            try
            {
                var existing = await _context.ServiceRequestStatuses.FindAsync(id);
                if (existing == null)
                {
                    return NotFound($"Status with ID {id} not found.");
                }

                if (await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusName == status.StatusName && s.StatusId != id))
                {
                    return BadRequest($"Status '{status.StatusName}' already exists.");
                }

                existing.StatusName = status.StatusName;
                existing.ColorCode = status.ColorCode;
                existing.Description = status.Description;
                existing.IsActive = status.IsActive;

                _context.Entry(existing).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceRequestStatusExistsAsync(id))
                {
                    return NotFound($"Status with ID {id} not found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceRequestStatuses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequestStatus(int id)
        {
            try
            {
                var status = await _context.ServiceRequestStatuses
                    .Include(s => s.ServiceRequests)
                    .FirstOrDefaultAsync(s => s.StatusId == id);

                if (status == null)
                {
                    return NotFound($"Status with ID {id} not found.");
                }

                if (status.ServiceRequests.Any())
                {
                    return BadRequest($"Cannot delete status '{status.StatusName}' because it is used in {status.ServiceRequests.Count} service requests.");
                }

                _context.ServiceRequestStatuses.Remove(status);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> ServiceRequestStatusExistsAsync(int id)
        {
            return await _context.ServiceRequestStatuses.AnyAsync(s => s.StatusId == id);
        }
    }
}