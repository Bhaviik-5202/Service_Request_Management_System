using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class RequestTypeTechnicianMappingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RequestTypeTechnicianMappingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RequestTypeTechnicianMappings/GetMappings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestTypeTechnicianMapping>>> GetMappings()
        {
            return await _context.RequestTypeTechnicianMappings
                .Where(rm => !rm.IsDeleted)
                .Include(rm => rm.RequestType)
                .ThenInclude(rt => rt.ServiceType)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.Department)
                .ToListAsync();
        }

        // GET: api/RequestTypeTechnicianMappings/GetMapping/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestTypeTechnicianMapping>> GetMapping(int id)
        {
            var mapping = await _context.RequestTypeTechnicianMappings
                .Where(rm => !rm.IsDeleted)
                .Include(rm => rm.RequestType)
                .ThenInclude(rt => rt.ServiceType)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.Department)
                .FirstOrDefaultAsync(rm => rm.MappingId == id);

            if (mapping == null)
            {
                return NotFound($"Mapping with ID {id} not found.");
            }

            return Ok(mapping);
        }

        // GET: api/RequestTypeTechnicianMappings/GetMappingsByRequestType/requesttype/5
        [HttpGet("requesttype/{requestTypeId}")]
        public async Task<ActionResult<IEnumerable<RequestTypeTechnicianMapping>>> GetMappingsByRequestType(int requestTypeId)
        {
            var mappings = await _context.RequestTypeTechnicianMappings
                .Where(rm => !rm.IsDeleted && rm.RequestTypeId == requestTypeId)
                .Include(rm => rm.RequestType)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.Department)
                .ToListAsync();

            return Ok(mappings);
        }

        // GET: api/RequestTypeTechnicianMappings/GetMappingsByPersonnel/personnel/5
        [HttpGet("personnel/{departmentPersonnelId}")]
        public async Task<ActionResult<IEnumerable<RequestTypeTechnicianMapping>>> GetMappingsByPersonnel(int departmentPersonnelId)
        {
            var mappings = await _context.RequestTypeTechnicianMappings
                .Where(rm => !rm.IsDeleted && rm.DepartmentPersonnelId == departmentPersonnelId)
                .Include(rm => rm.RequestType)
                .ThenInclude(rt => rt.ServiceType)
                .Include(rm => rm.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .ToListAsync();

            return Ok(mappings);
        }

        // POST: api/RequestTypeTechnicianMappings/PostMapping
        [HttpPost]
        public async Task<ActionResult<RequestTypeTechnicianMapping>> PostMapping(RequestTypeTechnicianMapping mapping)
        {
            var requestType = await _context.RequestTypes
                .FirstOrDefaultAsync(rt => rt.RequestTypeId == mapping.RequestTypeId && !rt.IsDeleted);

            if (requestType == null)
            {
                return BadRequest($"Request Type with ID {mapping.RequestTypeId} not found.");
            }

            var personnel = await _context.DepartmentPersonnel
                .FirstOrDefaultAsync(dp => dp.DepartmentPersonnelId == mapping.DepartmentPersonnelId && !dp.IsDeleted);

            if (personnel == null)
            {
                return BadRequest($"Department Personnel with ID {mapping.DepartmentPersonnelId} not found.");
            }

            if (await _context.RequestTypeTechnicianMappings.AnyAsync(rm =>
                rm.RequestTypeId == mapping.RequestTypeId &&
                rm.DepartmentPersonnelId == mapping.DepartmentPersonnelId &&
                !rm.IsDeleted))
            {
                return BadRequest("This mapping already exists.");
            }

            mapping.CreatedAt = DateTime.UtcNow;
            mapping.UpdatedAt = DateTime.UtcNow;
            mapping.IsDeleted = false;

            _context.RequestTypeTechnicianMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMapping), new { id = mapping.MappingId }, mapping);
        }

        // PUT: api/RequestTypeTechnicianMappings/PutMapping/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMapping(int id, RequestTypeTechnicianMapping mapping)
        {
            if (id != mapping.MappingId)
            {
                return BadRequest("Mapping ID mismatch.");
            }

            var existing = await _context.RequestTypeTechnicianMappings
                .FirstOrDefaultAsync(rm => rm.MappingId == id && !rm.IsDeleted);

            if (existing == null)
            {
                return NotFound($"Mapping with ID {id} not found.");
            }

            var requestType = await _context.RequestTypes
                .FirstOrDefaultAsync(rt => rt.RequestTypeId == mapping.RequestTypeId && !rt.IsDeleted);

            if (requestType == null)
            {
                return BadRequest($"Request Type with ID {mapping.RequestTypeId} not found.");
            }

            var personnel = await _context.DepartmentPersonnel
                .FirstOrDefaultAsync(dp => dp.DepartmentPersonnelId == mapping.DepartmentPersonnelId && !dp.IsDeleted);

            if (personnel == null)
            {
                return BadRequest($"Department Personnel with ID {mapping.DepartmentPersonnelId} not found.");
            }

            if (await _context.RequestTypeTechnicianMappings.AnyAsync(rm =>
                rm.RequestTypeId == mapping.RequestTypeId &&
                rm.DepartmentPersonnelId == mapping.DepartmentPersonnelId &&
                rm.MappingId != id &&
                !rm.IsDeleted))
            {
                return BadRequest("This mapping already exists.");
            }

            existing.RequestTypeId = mapping.RequestTypeId;
            existing.DepartmentPersonnelId = mapping.DepartmentPersonnelId;
            existing.IsActive = mapping.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/RequestTypeTechnicianMappings/DeleteMapping/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            var mapping = await _context.RequestTypeTechnicianMappings
                .FirstOrDefaultAsync(rm => rm.MappingId == id && !rm.IsDeleted);

            if (mapping == null)
            {
                return NotFound($"Mapping with ID {id} not found.");
            }

            mapping.IsDeleted = true;
            mapping.IsActive = false;
            mapping.UpdatedAt = DateTime.UtcNow;

            _context.Entry(mapping).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}