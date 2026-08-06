using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles/GetRoles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        // GET: api/Roles/GetRole/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            return Ok(role);
        }

        // POST: api/Roles/PostRole
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName))
            {
                return BadRequest($"Role '{role.RoleName}' already exists.");
            }

            role.CreatedAt = DateTime.UtcNow;
            role.UpdatedAt = DateTime.UtcNow;

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
        }

        // PUT: api/Roles/PutRole/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, Role role)
        {
            if (id != role.RoleId)
            {
                return BadRequest("Role ID mismatch.");
            }

            var existingRole = await _context.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName && r.RoleId != id))
            {
                return BadRequest($"Role '{role.RoleName}' already exists.");
            }

            existingRole.RoleName = role.RoleName;
            existingRole.Description = role.Description;
            existingRole.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingRole).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Roles/DeleteRole/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            if (role.Users.Any())
            {
                return BadRequest($"Cannot delete role '{role.RoleName}' because it is assigned to {role.Users.Count} user(s).");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}