using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class RequestTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RequestTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RequestTypes/GetRequestTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestType>>> GetRequestTypes()
        {
            return await _context.RequestTypes
                .Where(rt => !rt.IsDeleted)
                .Include(rt => rt.ServiceType)
                .Include(rt => rt.ServiceRequests)
                .Include(rt => rt.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .ToListAsync();
        }

        // GET: api/RequestTypes/GetRequestType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestType>> GetRequestType(int id)
        {
            var requestType = await _context.RequestTypes
                .Where(rt => !rt.IsDeleted)
                .Include(rt => rt.ServiceType)
                .Include(rt => rt.ServiceRequests)
                .Include(rt => rt.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .FirstOrDefaultAsync(rt => rt.RequestTypeId == id);

            if (requestType == null)
            {
                return NotFound($"Request Type with ID {id} not found.");
            }

            return Ok(requestType);
        }

        // GET: api/RequestTypes/GetRequestTypesByServiceType/service/5
        [HttpGet("service/{serviceTypeId}")]
        public async Task<ActionResult<IEnumerable<RequestType>>> GetRequestTypesByServiceType(int serviceTypeId)
        {
            var requestTypes = await _context.RequestTypes
                .Where(rt => !rt.IsDeleted && rt.ServiceTypeId == serviceTypeId)
                .Include(rt => rt.ServiceType)
                .Include(rt => rt.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .ToListAsync();

            return Ok(requestTypes);
        }

        // POST: api/RequestTypes/PostRequestType
        [HttpPost]
        public async Task<ActionResult<RequestType>> PostRequestType(RequestType requestType)
        {
            if (await _context.RequestTypes.AnyAsync(rt => rt.RequestTypeName == requestType.RequestTypeName && !rt.IsDeleted))
            {
                return BadRequest($"Request Type '{requestType.RequestTypeName}' already exists.");
            }

            if (!await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeId == requestType.ServiceTypeId && !st.IsDeleted))
            {
                return BadRequest($"Service Type with ID {requestType.ServiceTypeId} does not exist.");
            }

            requestType.CreatedAt = DateTime.UtcNow;
            requestType.UpdatedAt = DateTime.UtcNow;
            requestType.IsDeleted = false;

            _context.RequestTypes.Add(requestType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRequestType), new { id = requestType.RequestTypeId }, requestType);
        }

        // PUT: api/RequestTypes/PutRequestType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequestType(int id, RequestType requestType)
        {
            if (id != requestType.RequestTypeId)
            {
                return BadRequest("Request Type ID mismatch.");
            }

            var existing = await _context.RequestTypes.FindAsync(id);
            if (existing == null || existing.IsDeleted)
            {
                return NotFound($"Request Type with ID {id} not found.");
            }

            if (await _context.RequestTypes.AnyAsync(rt => rt.RequestTypeName == requestType.RequestTypeName && rt.RequestTypeId != id && !rt.IsDeleted))
            {
                return BadRequest($"Request Type '{requestType.RequestTypeName}' already exists.");
            }

            if (!await _context.ServiceTypes.AnyAsync(st => st.ServiceTypeId == requestType.ServiceTypeId && !st.IsDeleted))
            {
                return BadRequest($"Service Type with ID {requestType.ServiceTypeId} does not exist.");
            }

            existing.RequestTypeName = requestType.RequestTypeName;
            existing.ServiceTypeId = requestType.ServiceTypeId;
            existing.Description = requestType.Description;
            existing.IsActive = requestType.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/RequestTypes/DeleteRequestType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequestType(int id)
        {
            var requestType = await _context.RequestTypes
                .Include(rt => rt.ServiceRequests)
                .Include(rt => rt.RequestTypeTechnicianMappings)
                .FirstOrDefaultAsync(rt => rt.RequestTypeId == id);

            if (requestType == null || requestType.IsDeleted)
            {
                return NotFound($"Request Type with ID {id} not found.");
            }

            if (requestType.ServiceRequests.Any(sr => !sr.IsDeleted))
            {
                return BadRequest($"Cannot delete request type with {requestType.ServiceRequests.Count} active service requests.");
            }

            requestType.IsDeleted = true;
            requestType.IsActive = false;
            requestType.UpdatedAt = DateTime.UtcNow;

            _context.Entry(requestType).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}