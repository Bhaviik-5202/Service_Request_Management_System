using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departments/GetDepartments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            return await _context.Departments
                .Where(d => !d.IsDeleted)
                .Include(d => d.Users)
                .Include(d => d.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .Include(d => d.ServiceRequests)
                .Include(d => d.Assets)
                .ToListAsync();
        }

        // GET: api/Departments/GetDepartment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Where(d => !d.IsDeleted)
                .Include(d => d.Users)
                .Include(d => d.DepartmentPersonnel)
                .ThenInclude(dp => dp.User)
                .Include(d => d.ServiceRequests)
                .Include(d => d.Assets)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            return Ok(department);
        }

        // POST: api/Departments/PostDepartment
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && !d.IsDeleted))
            {
                return BadRequest($"Department '{department.DepartmentName}' already exists.");
            }

            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode && !d.IsDeleted))
            {
                return BadRequest($"Department code '{department.DepartmentCode}' already exists.");
            }

            department.CreatedAt = DateTime.UtcNow;
            department.UpdatedAt = DateTime.UtcNow;
            department.IsDeleted = false;

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, department);
        }

        // PUT: api/Departments/PutDepartment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest("Department ID mismatch.");
            }

            var existingDepartment = await _context.Departments.FindAsync(id);
            if (existingDepartment == null || existingDepartment.IsDeleted)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && d.DepartmentId != id && !d.IsDeleted))
            {
                return BadRequest($"Department '{department.DepartmentName}' already exists.");
            }

            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode && d.DepartmentId != id && !d.IsDeleted))
            {
                return BadRequest($"Department code '{department.DepartmentCode}' already exists.");
            }

            existingDepartment.DepartmentName = department.DepartmentName;
            existingDepartment.DepartmentCode = department.DepartmentCode;
            existingDepartment.Description = department.Description;
            existingDepartment.IsActive = department.IsActive;
            existingDepartment.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingDepartment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Departments/DeleteDepartment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id, [FromQuery] int? deletedByUserId)
        {
            var department = await _context.Departments
                .Include(d => d.Users)
                .Include(d => d.DepartmentPersonnel)
                .Include(d => d.ServiceRequests)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null || department.IsDeleted)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            if (department.Users.Any(u => u.IsDeleted == false))
            {
                return BadRequest("Cannot delete department with active users.");
            }

            if (department.DepartmentPersonnel.Any(dp => !dp.IsDeleted))
            {
                return BadRequest("Cannot delete department with active personnel.");
            }

            if (department.ServiceRequests.Any(sr => !sr.IsDeleted))
            {
                return BadRequest("Cannot delete department with active service requests.");
            }

            department.IsDeleted = true;
            department.DeletedAt = DateTime.UtcNow;
            department.DeletedByUserId = deletedByUserId;

            _context.Entry(department).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Departments/RestoreDepartment/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            if (!department.IsDeleted)
            {
                return BadRequest("Department is not deleted.");
            }

            department.IsDeleted = false;
            department.DeletedAt = null;
            department.DeletedByUserId = null;
            department.UpdatedAt = DateTime.UtcNow;

            _context.Entry(department).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}