using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Services;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(
            AppDbContext context,
            TokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponse>>> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "Email and password are required.",
                    Data = null
                });
            }

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

            if (user == null || user.Status != UserStatus.Active)
            {
                return Unauthorized(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid credentials or inactive account.",
                    Data = null
                });
            }

            // Verify password using PBKDF2 hasher
            var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            // Backward compatibility: if salt is empty, the password was stored as plain text.
            // Verify by direct comparison, then upgrade to PBKDF2 on success.
            if (!isValid && string.IsNullOrEmpty(user.PasswordSalt))
            {
                if (user.PasswordHash == request.Password)
                {
                    var (newHash, newSalt) = _passwordHasher.HashPassword(request.Password);
                    user.PasswordHash = newHash;
                    user.PasswordSalt = newSalt;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    isValid = true;
                }
            }

            if (!isValid)
            {
                return Unauthorized(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid credentials or inactive account.",
                    Data = null
                });
            }

            var authResponse = _tokenService.GenerateToken(
                username: user.Email,
                role: user.Role.ToString(),
                email: user.Email,
                userId: user.UserId,
                fullName: user.FullName,
                employeeId: user.EmployeeId,
                departmentId: user.DepartmentId,
                departmentName: user.Department?.DepartmentName
            );

            // Log authentication event
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Login",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<AuthResponse>
            {
                Success = true,
                Message = "Login successful.",
                Data = authResponse
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponse>>> Register(
            [FromBody] RegisterRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "Full name, email, and password are required.",
                    Data = null
                });
            }

            var email = dto.Email.Trim().ToLowerInvariant();
            var employeeId = string.IsNullOrWhiteSpace(dto.EmployeeId)
                ? $"EMP-{DateTime.UtcNow.Ticks % 100000:D5}"
                : dto.EmployeeId.Trim();

            var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email || u.EmployeeId == employeeId);
            if (exists)
            {
                return BadRequest(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "User with this email or Employee ID already exists.",
                    Data = null
                });
            }

            var (hash, salt) = _passwordHasher.HashPassword(dto.Password);
            var now = DateTime.UtcNow;

            var user = new User
            {
                EmployeeId = employeeId,
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                Phone = dto.Phone?.Trim(),
                Status = UserStatus.Active,
                JoinedDate = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Default User Settings
            var userSettings = new UserSettings
            {
                UserId = user.UserId,
                Theme = "light",
                NotifyRequestUpdates = true,
                NotifyApprovalAlerts = true,
                NotifySLAWarnings = true,
                NotifyAssetEvents = false,
                NotifyEmailDigest = false,
                UpdatedAt = now
            };
            _context.UserSettings.Add(userSettings);

            // Audit log
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Registered",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            var department = dto.DepartmentId.HasValue
                ? await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == dto.DepartmentId.Value)
                : null;

            var authResponse = _tokenService.GenerateToken(
                username: user.Email,
                role: user.Role.ToString(),
                email: user.Email,
                userId: user.UserId,
                fullName: user.FullName,
                employeeId: user.EmployeeId,
                departmentId: user.DepartmentId,
                departmentName: department?.DepartmentName
            );

            return CreatedAtAction(nameof(Me), new ApiResponseDto<AuthResponse>
            {
                Success = true,
                Message = "Registration successful.",
                Data = authResponse
            });
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponseDto<object>>> Me()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Unauthorized request.",
                    Data = null
                });
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.UserSettings)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

            if (user == null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Current user retrieved successfully.",
                Data = new
                {
                    user.UserId,
                    user.EmployeeId,
                    user.FullName,
                    user.Email,
                    Role = user.Role.ToString(),
                    user.DepartmentId,
                    DepartmentName = user.Department?.DepartmentName,
                    user.Phone,
                    Status = user.Status.ToString(),
                    user.JoinedDate,
                    Settings = user.UserSettings == null ? null : new
                    {
                        user.UserSettings.Theme,
                        user.UserSettings.NotifyRequestUpdates,
                        user.UserSettings.NotifyApprovalAlerts,
                        user.UserSettings.NotifySLAWarnings,
                        user.UserSettings.NotifyAssetEvents,
                        user.UserSettings.NotifyEmailDigest
                    }
                }
            });
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponseDto<object>>> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Unauthorized request.",
                    Data = null
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                });
            }

            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Current password is incorrect.",
                    Data = null
                });
            }

            var (newHash, newSalt) = _passwordHasher.HashPassword(dto.NewPassword);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "Password Changed",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Password changed successfully.",
                Data = null
            });
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Public data accessed successfully.",
                Data = new
                {
                    Application = "Service Request Management System API",
                    Access = "Public",
                    Version = "1.0.0",
                    Status = "Online"
                }
            });
        }
    }
}
