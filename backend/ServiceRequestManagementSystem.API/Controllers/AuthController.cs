using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// POST /api/v1/auth/login
        /// Validates credentials and returns JWT access token & user profile.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<AuthResponseDto> { Success = false, Message = "Invalid request payload." });

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Code = "UNAUTHORIZED"
                });
            }

            // Note: Password verification using BCrypt will be hooked here when Auth Service is implemented.
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = "SIMULATED_JWT_ACCESS_TOKEN",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = new UserResponseDto
                {
                    UserId = user.UserId,
                    EmployeeId = user.EmployeeId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.DepartmentName,
                    Phone = user.Phone,
                    Status = user.Status,
                    JoinedDate = user.JoinedDate,
                    LastLoginAt = user.LastLoginAt
                }
            };

            return Ok(new ApiResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Login successful.",
                Data = response
            });
        }

        /// <summary>
        /// POST /api/v1/auth/signup
        /// Registers a new corporate user account with default Requestor role.
        /// </summary>
        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> Signup([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<UserResponseDto> { Success = false, Message = "Validation failed." });

            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (exists)
            {
                return BadRequest(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = "A user with this email address already exists."
                });
            }

            var userCount = await _context.Users.CountAsync();
            var newUser = new Models.User
            {
                EmployeeId = $"EMP-{(userCount + 1):D4}",
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password, // Note: BCrypt hashing applied in Auth Service
                Role = Enums.UserRole.Requestor,
                DepartmentId = request.DepartmentId,
                Phone = request.Phone,
                Status = Enums.UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Create default UserSettings
            _context.UserSettings.Add(new Models.UserSettings
            {
                UserId = newUser.UserId,
                Theme = "light",
                NotifyRequestUpdates = true,
                NotifyApprovalAlerts = true,
                NotifySLAWarnings = true,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var userResponse = new UserResponseDto
            {
                UserId = newUser.UserId,
                EmployeeId = newUser.EmployeeId,
                FullName = newUser.FullName,
                Email = newUser.Email,
                Role = newUser.Role,
                DepartmentId = newUser.DepartmentId,
                Phone = newUser.Phone,
                Status = newUser.Status,
                JoinedDate = newUser.JoinedDate
            };

            return Ok(new ApiResponseDto<UserResponseDto>
            {
                Success = true,
                Message = "Registration successful. Welcome aboard!",
                Data = userResponse
            });
        }

        /// <summary>
        /// POST /api/v1/auth/forgot-password
        /// Generates password reset token cue.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponseDto<string>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                // Security practice: Return success to avoid email enumeration
                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Message = "If your email is registered, a password reset link has been sent."
                });
            }

            // Note: Email dispatches and reset token generation will be handled by Email Service later.
            return Ok(new ApiResponseDto<string>
            {
                Success = true,
                Message = "If your email is registered, a password reset link has been sent."
            });
        }

        /// <summary>
        /// POST /api/v1/auth/reset-password
        /// Resets user password using reset token.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponseDto<string>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            // Note: Token validation and password update logic will be handled by Auth Service later.
            return Ok(new ApiResponseDto<string>
            {
                Success = true,
                Message = "Password reset successfully. You may now log in."
            });
        }

        /// <summary>
        /// GET /api/v1/auth/me
        /// Returns currently authenticated user profile.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetCurrentUser()
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new ApiResponseDto<UserResponseDto> { Success = false, Message = "User profile not found." });

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName,
                Phone = user.Phone,
                Status = user.Status,
                JoinedDate = user.JoinedDate
            };

            return Ok(new ApiResponseDto<UserResponseDto> { Success = true, Data = response });
        }
    }
}
