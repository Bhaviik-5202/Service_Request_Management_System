using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ServiceTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceType>>> GetServiceTypes()
        {
            try
            {
                return await _context.ServiceTypes
                    .Where(st => !st.IsDeleted)
                    .Include(st => st.RequestTypes)
                    .Include(st => st.ServiceRequests)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceType>> GetServiceType(int id)
        {
            try
            {
                var serviceType = await _context.ServiceTypes
                    .Where(st => !st.IsDeleted)
                    .Include(st => st.RequestTypes)
                    .Include(st => st.ServiceRequests)
                    .FirstOrDefaultAsync(st => st.ServiceTypeId == id);

                if (serviceType == null)
                {
                    return NotFound($"Service Type with ID {id} not found.");
                }

                return Ok(serviceType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ServiceTypes
        [HttpPost]
        public async Task<ActionResult<ServiceType>> PostServiceType(ServiceType serviceType)
        {
            try
            {
                if (await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeName == serviceType.ServiceTypeName && !st.IsDeleted))
                {
                    return BadRequest($"Service Type '{serviceType.ServiceTypeName}' already exists.");
                }

                if (await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeCode == serviceType.ServiceTypeCode && !st.IsDeleted))
                {
                    return BadRequest($"Service Type code '{serviceType.ServiceTypeCode}' already exists.");
                }

                serviceType.CreatedAt = DateTime.UtcNow;
                serviceType.UpdatedAt = DateTime.UtcNow;
                serviceType.IsDeleted = false;

                _context.ServiceTypes.Add(serviceType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServiceType), new { id = serviceType.ServiceTypeId }, serviceType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/ServiceTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceType(int id, ServiceType serviceType)
        {
            if (id != serviceType.ServiceTypeId)
            {
                return BadRequest("Service Type ID mismatch.");
            }

            try
            {
                var existing = await _context.ServiceTypes.FindAsync(id);
                if (existing == null || existing.IsDeleted)
                {
                    return NotFound($"Service Type with ID {id} not found.");
                }

                if (await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeName == serviceType.ServiceTypeName && st.ServiceTypeId != id && !st.IsDeleted))
                {
                    return BadRequest($"Service Type '{serviceType.ServiceTypeName}' already exists.");
                }

                if (await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeCode == serviceType.ServiceTypeCode && st.ServiceTypeId != id && !st.IsDeleted))
                {
                    return BadRequest($"Service Type code '{serviceType.ServiceTypeCode}' already exists.");
                }

                existing.ServiceTypeName = serviceType.ServiceTypeName;
                existing.ServiceTypeCode = serviceType.ServiceTypeCode;
                existing.Description = serviceType.Description;
                existing.IsActive = serviceType.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Entry(existing).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceTypeExistsAsync(id))
                {
                    return NotFound($"Service Type with ID {id} not found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            try
            {
                var serviceType = await _context.ServiceTypes
                    .Include(st => st.RequestTypes)
                    .Include(st => st.ServiceRequests)
                    .FirstOrDefaultAsync(st => st.ServiceTypeId == id);

                if (serviceType == null || serviceType.IsDeleted)
                {
                    return NotFound($"Service Type with ID {id} not found.");
                }

                // Check if there are active Request Types
                if (serviceType.RequestTypes.Any(rt => !rt.IsDeleted))
                {
                    return BadRequest($"Cannot delete service type with {serviceType.RequestTypes.Count} active request types.");
                }

                // Check if there are active Service Requests
                if (serviceType.ServiceRequests.Any(sr => !sr.IsDeleted))
                {
                    return BadRequest($"Cannot delete service type with {serviceType.ServiceRequests.Count} active service requests.");
                }

                serviceType.IsDeleted = true;
                serviceType.IsActive = false;
                serviceType.UpdatedAt = DateTime.UtcNow;

                _context.Entry(serviceType).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> ServiceTypeExistsAsync(int id)
        {
            return await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeId == id && !st.IsDeleted);
        }
    }
}