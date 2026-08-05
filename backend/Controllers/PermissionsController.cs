using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Permissions/GetPermissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            return await _context.Permissions
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .ToListAsync();
        }

        // GET: api/Permissions/GetPermission/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found.");
            }

            return Ok(permission);
        }

        // POST: api/Permissions/PostPermission
        [HttpPost]
        public async Task<ActionResult<Permission>> PostPermission(Permission permission)
        {
            if (await _context.Permissions.AnyAsync(p => p.PermissionKey == permission.PermissionKey))
            {
                return BadRequest($"Permission key '{permission.PermissionKey}' already exists.");
            }

            permission.CreatedAt = DateTime.UtcNow;

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermission), new { id = permission.PermissionId }, permission);
        }

        // PUT: api/Permissions/PutPermission/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, Permission permission)
        {
            if (id != permission.PermissionId)
            {
                return BadRequest("Permission ID mismatch.");
            }

            var existingPermission = await _context.Permissions.FindAsync(id);
            if (existingPermission == null)
            {
                return NotFound($"Permission with ID {id} not found.");
            }

            if (await _context.Permissions.AnyAsync(p => p.PermissionKey == permission.PermissionKey && p.PermissionId != id))
            {
                return BadRequest($"Permission key '{permission.PermissionKey}' already exists.");
            }

            existingPermission.PermissionKey = permission.PermissionKey;
            existingPermission.Description = permission.Description;

            _context.Entry(existingPermission).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Permissions/DeletePermission/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found.");
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}