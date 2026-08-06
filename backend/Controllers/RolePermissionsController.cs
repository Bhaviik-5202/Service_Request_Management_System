using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class RolePermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolePermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RolePermissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolePermission>>> GetRolePermissions()
        {
            try
            {
                return await _context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/RolePermissions/role/5
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<RolePermission>>> GetPermissionsByRole(int roleId)
        {
            try
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Include(rp => rp.Permission)
                    .ToListAsync();

                return Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/RolePermissions/permission/5
        [HttpGet("permission/{permissionId}")]
        public async Task<ActionResult<IEnumerable<RolePermission>>> GetRolesByPermission(int permissionId)
        {
            try
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.PermissionId == permissionId)
                    .Include(rp => rp.Role)
                    .ToListAsync();

                return Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/RolePermissions
        [HttpPost]
        public async Task<ActionResult<RolePermission>> PostRolePermission(RolePermission rolePermission)
        {
            try
            {
                // Validate Role exists
                if (!await _context.Roles.AnyAsync(r => r.RoleId == rolePermission.RoleId))
                {
                    return BadRequest($"Role with ID {rolePermission.RoleId} does not exist.");
                }

                // Validate Permission exists
                if (!await _context.Permissions.AnyAsync(p => p.PermissionId == rolePermission.PermissionId))
                {
                    return BadRequest($"Permission with ID {rolePermission.PermissionId} does not exist.");
                }

                // Check if mapping already exists
                if (await _context.RolePermissions.AnyAsync(rp => 
                    rp.RoleId == rolePermission.RoleId && 
                    rp.PermissionId == rolePermission.PermissionId))
                {
                    return BadRequest("This role-permission mapping already exists.");
                }

                _context.RolePermissions.Add(rolePermission);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                await _context.Entry(rolePermission).Reference(rp => rp.Role).LoadAsync();
                await _context.Entry(rolePermission).Reference(rp => rp.Permission).LoadAsync();

                return CreatedAtAction(nameof(GetRolePermissions), new { roleId = rolePermission.RoleId, permissionId = rolePermission.PermissionId }, rolePermission);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/RolePermissions/role/5/permission/10
        [HttpDelete("role/{roleId}/permission/{permissionId}")]
        public async Task<IActionResult> DeleteRolePermission(int roleId, int permissionId)
        {
            try
            {
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (rolePermission == null)
                {
                    return NotFound($"Role-Permission mapping not found for Role ID {roleId} and Permission ID {permissionId}.");
                }

                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/RolePermissions/role/5
        [HttpDelete("role/{roleId}")]
        public async Task<IActionResult> DeleteAllPermissionsForRole(int roleId)
        {
            try
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                if (!rolePermissions.Any())
                {
                    return NotFound($"No permissions found for Role ID {roleId}.");
                }

                _context.RolePermissions.RemoveRange(rolePermissions);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}