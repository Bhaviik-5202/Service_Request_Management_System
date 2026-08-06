using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class DepartmentPersonnelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentPersonnelController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DepartmentPersonnel/GetDepartmentPersonnel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentPersonnel>>> GetDepartmentPersonnel()
        {
            return await _context.DepartmentPersonnel
                .Where(dp => !dp.IsDeleted)
                .Include(dp => dp.User)
                .Include(dp => dp.Department)
                .Include(dp => dp.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.RequestType)
                .ToListAsync();
        }

        // GET: api/DepartmentPersonnel/GetDepartmentPersonnel/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentPersonnel>> GetDepartmentPersonnel(int id)
        {
            var departmentPersonnel = await _context.DepartmentPersonnel
                .Where(dp => !dp.IsDeleted)
                .Include(dp => dp.User)
                .Include(dp => dp.Department)
                .Include(dp => dp.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.RequestType)
                .FirstOrDefaultAsync(dp => dp.DepartmentPersonnelId == id);

            if (departmentPersonnel == null)
            {
                return NotFound($"Department Personnel record with ID {id} not found.");
            }

            return Ok(departmentPersonnel);
        }

        // GET: api/DepartmentPersonnel/GetDepartmentPersonnelByDepartment/department/5
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<DepartmentPersonnel>>> GetDepartmentPersonnelByDepartment(int departmentId)
        {
            var personnel = await _context.DepartmentPersonnel
                .Where(dp => !dp.IsDeleted && dp.DepartmentId == departmentId)
                .Include(dp => dp.User)
                .Include(dp => dp.Department)
                .ToListAsync();

            return Ok(personnel);
        }

        // GET: api/DepartmentPersonnel/GetDepartmentPersonnelByUser/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<DepartmentPersonnel>>> GetDepartmentPersonnelByUser(int userId)
        {
            var personnel = await _context.DepartmentPersonnel
                .Where(dp => !dp.IsDeleted && dp.UserId == userId)
                .Include(dp => dp.Department)
                .Include(dp => dp.RequestTypeTechnicianMappings)
                .ThenInclude(rm => rm.RequestType)
                .ToListAsync();

            return Ok(personnel);
        }

        // GET: api/DepartmentPersonnel/GetHODByDepartment/hod/department/5
        [HttpGet("hod/department/{departmentId}")]
        public async Task<ActionResult<DepartmentPersonnel>> GetHODByDepartment(int departmentId)
        {
            var hod = await _context.DepartmentPersonnel
                .Where(dp => !dp.IsDeleted && dp.DepartmentId == departmentId && dp.IsHOD)
                .Include(dp => dp.User)
                .Include(dp => dp.Department)
                .FirstOrDefaultAsync();

            if (hod == null)
            {
                return NotFound($"HOD not found for Department ID {departmentId}.");
            }

            return Ok(hod);
        }

        // POST: api/DepartmentPersonnel/PostDepartmentPersonnel
        [HttpPost]
        public async Task<ActionResult<DepartmentPersonnel>> PostDepartmentPersonnel(DepartmentPersonnel departmentPersonnel)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == departmentPersonnel.UserId && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest($"User with ID {departmentPersonnel.UserId} not found or is deleted.");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentPersonnel.DepartmentId && !d.IsDeleted);

            if (department == null)
            {
                return BadRequest($"Department with ID {departmentPersonnel.DepartmentId} not found or is deleted.");
            }

            if (await _context.DepartmentPersonnel.AnyAsync(dp =>
                dp.UserId == departmentPersonnel.UserId &&
                dp.DepartmentId == departmentPersonnel.DepartmentId &&
                !dp.IsDeleted))
            {
                return BadRequest($"User {user.FullName} is already mapped to department {department.DepartmentName}.");
            }

            if (departmentPersonnel.IsHOD)
            {
                var existingHOD = await _context.DepartmentPersonnel
                    .FirstOrDefaultAsync(dp => dp.DepartmentId == departmentPersonnel.DepartmentId && dp.IsHOD && !dp.IsDeleted);

                if (existingHOD != null)
                {
                    return BadRequest($"Department {department.DepartmentName} already has an HOD. Please remove the existing HOD first.");
                }
            }

            departmentPersonnel.CreatedAt = DateTime.UtcNow;
            departmentPersonnel.UpdatedAt = DateTime.UtcNow;
            departmentPersonnel.IsDeleted = false;

            _context.DepartmentPersonnel.Add(departmentPersonnel);
            await _context.SaveChangesAsync();

            await _context.Entry(departmentPersonnel).Reference(dp => dp.User).LoadAsync();
            await _context.Entry(departmentPersonnel).Reference(dp => dp.Department).LoadAsync();

            return CreatedAtAction(nameof(GetDepartmentPersonnel), new { id = departmentPersonnel.DepartmentPersonnelId }, departmentPersonnel);
        }

        // PUT: api/DepartmentPersonnel/PutDepartmentPersonnel/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartmentPersonnel(int id, DepartmentPersonnel departmentPersonnel)
        {
            if (id != departmentPersonnel.DepartmentPersonnelId)
            {
                return BadRequest("Department Personnel ID mismatch.");
            }

            var existing = await _context.DepartmentPersonnel
                .FirstOrDefaultAsync(dp => dp.DepartmentPersonnelId == id && !dp.IsDeleted);

            if (existing == null)
            {
                return NotFound($"Department Personnel record with ID {id} not found.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == departmentPersonnel.UserId && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest($"User with ID {departmentPersonnel.UserId} not found or is deleted.");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentPersonnel.DepartmentId && !d.IsDeleted);

            if (department == null)
            {
                return BadRequest($"Department with ID {departmentPersonnel.DepartmentId} not found or is deleted.");
            }

            if (await _context.DepartmentPersonnel.AnyAsync(dp =>
                dp.UserId == departmentPersonnel.UserId &&
                dp.DepartmentId == departmentPersonnel.DepartmentId &&
                dp.DepartmentPersonnelId != id &&
                !dp.IsDeleted))
            {
                return BadRequest($"User {user.FullName} is already mapped to department {department.DepartmentName}.");
            }

            if (departmentPersonnel.IsHOD)
            {
                var existingHOD = await _context.DepartmentPersonnel
                    .FirstOrDefaultAsync(dp => dp.DepartmentId == departmentPersonnel.DepartmentId &&
                                                 dp.IsHOD &&
                                                 dp.DepartmentPersonnelId != id &&
                                                 !dp.IsDeleted);

                if (existingHOD != null)
                {
                    return BadRequest($"Department {department.DepartmentName} already has an HOD. Please remove the existing HOD first.");
                }
            }

            existing.UserId = departmentPersonnel.UserId;
            existing.DepartmentId = departmentPersonnel.DepartmentId;
            existing.IsHOD = departmentPersonnel.IsHOD;
            existing.IsActive = departmentPersonnel.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/DepartmentPersonnel/DeleteDepartmentPersonnel/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartmentPersonnel(int id)
        {
            var departmentPersonnel = await _context.DepartmentPersonnel
                .Include(dp => dp.RequestTypeTechnicianMappings)
                .FirstOrDefaultAsync(dp => dp.DepartmentPersonnelId == id);

            if (departmentPersonnel == null || departmentPersonnel.IsDeleted)
            {
                return NotFound($"Department Personnel record with ID {id} not found.");
            }

            if (departmentPersonnel.RequestTypeTechnicianMappings.Any(rm => !rm.IsDeleted))
            {
                return BadRequest($"Cannot delete personnel with {departmentPersonnel.RequestTypeTechnicianMappings.Count} active technician mappings.");
            }

            departmentPersonnel.IsDeleted = true;
            departmentPersonnel.IsActive = false;
            departmentPersonnel.UpdatedAt = DateTime.UtcNow;

            _context.Entry(departmentPersonnel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}