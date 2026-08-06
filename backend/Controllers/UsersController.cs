using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UsersController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/Users/GetUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserSetting)
                .Include(u => u.DepartmentPersonnel)
                .ThenInclude(dp => dp.Department)
                .ToListAsync();
        }

        // GET: api/Users/GetUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => !u.IsDeleted)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserSetting)
                .Include(u => u.DepartmentPersonnel)
                .ThenInclude(dp => dp.Department)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        // GET: api/Users/GetUserByEmail/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _context.Users
                .Where(u => !u.IsDeleted && u.Email == email)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserSetting)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with email '{email}' not found.");
            }

            return Ok(user);
        }

        // GET: api/Users/GetUserByEmployeeId/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<User>> GetUserByEmployeeId(string employeeId)
        {
            var user = await _context.Users
                .Where(u => !u.IsDeleted && u.EmployeeId == employeeId)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserSetting)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with Employee ID '{employeeId}' not found.");
            }

            return Ok(user);
        }

        // POST: api/Users/PostUser
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email && !u.IsDeleted))
            {
                return BadRequest($"User with email '{user.Email}' already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.EmployeeId == user.EmployeeId && !u.IsDeleted))
            {
                return BadRequest($"User with Employee ID '{user.EmployeeId}' already exists.");
            }

            if (!await _context.Roles.AnyAsync(r => r.RoleId == user.RoleId))
            {
                return BadRequest($"Role with ID {user.RoleId} does not exist.");
            }

            if (user.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.DepartmentId == user.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {user.DepartmentId} does not exist.");
            }

            // Hash the password before storing
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            user.JoinedDate = DateTime.UtcNow.Date;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsDeleted = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userSetting = new UserSetting
            {
                UserId = user.UserId,
                Theme = "light",
                TwoFactorEnabled = false,
                NotifyRequestUpdates = true,
                NotifyApprovalAlerts = true,
                NotifySLAWarnings = true,
                NotifyAssetEvents = false,
                NotifyEmailDigest = false,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSettings.Add(userSetting);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.UserSetting).LoadAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        // PUT: api/Users/PutUser/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await _context.Users
                .Include(u => u.UserSetting)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (existingUser == null || existingUser.IsDeleted)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id && !u.IsDeleted))
            {
                return BadRequest($"User with email '{user.Email}' already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.EmployeeId == user.EmployeeId && u.UserId != id && !u.IsDeleted))
            {
                return BadRequest($"User with Employee ID '{user.EmployeeId}' already exists.");
            }

            if (!await _context.Roles.AnyAsync(r => r.RoleId == user.RoleId))
            {
                return BadRequest($"Role with ID {user.RoleId} does not exist.");
            }

            if (user.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.DepartmentId == user.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {user.DepartmentId} does not exist.");
            }

            existingUser.EmployeeId = user.EmployeeId;
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.RoleId = user.RoleId;
            existingUser.DepartmentId = user.DepartmentId;
            existingUser.Phone = user.Phone;
            existingUser.Status = user.Status;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Only rehash if a new password is provided and it differs from the stored hash
            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash != existingUser.PasswordHash)
            {
                // If the incoming value is already a BCrypt hash, keep it; otherwise hash it
                existingUser.PasswordHash = user.PasswordHash.StartsWith("$2")
                    ? user.PasswordHash
                    : BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            _context.Entry(existingUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/DeleteUser/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.AssignedAssets)
                .Include(u => u.RequestedServiceRequests)
                .Include(u => u.AssignedServiceRequests)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null || user.IsDeleted)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (user.AssignedAssets.Any(a => !a.IsDeleted))
            {
                return BadRequest($"Cannot delete user with {user.AssignedAssets.Count} assigned assets.");
            }

            if (user.RequestedServiceRequests.Any(sr => !sr.IsDeleted))
            {
                return BadRequest($"Cannot delete user with {user.RequestedServiceRequests.Count} active service requests.");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.Status = "Inactive";

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users/RestoreUser/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (!user.IsDeleted)
            {
                return BadRequest("User is not deleted.");
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await _context.Users
                .Where(u => !u.IsDeleted && u.Status == "Active" && u.Email == loginRequest.Email)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserSetting)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Verify password — supports both BCrypt hashes and legacy plain-text (rehashes on match)
            bool passwordValid;
            if (user.PasswordHash.StartsWith("$2"))
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            }
            else
            {
                passwordValid = user.PasswordHash == loginRequest.Password;
                if (passwordValid)
                {
                    // Migrate plain-text password to BCrypt hash
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password);
                }
            }

            if (!passwordValid)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var roleName = user.Role?.RoleName ?? "Requestor";
            var token = _jwtService.GenerateToken(user.UserId, user.Email, roleName);

            return Ok(new
            {
                token,
                user.UserId,
                user.EmployeeId,
                user.FullName,
                user.Email,
                user.RoleId,
                RoleName = roleName,
                user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName,
                user.Phone,
                user.Status,
                user.JoinedDate,
                user.LastLoginAt,
                user.CreatedAt
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
