using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUnitOfWork uow,
            AppDbContext context,
            TokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _uow = uow;
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponseDto<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Fail<AuthResponse>("Email and password are required.");

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

            if (user == null || user.Status != UserStatus.Active)
                return Fail<AuthResponse>("Invalid credentials or inactive account.");

            var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            if (!isValid && string.IsNullOrEmpty(user.PasswordSalt))
            {
                if (user.PasswordHash == request.Password)
                {
                    var (newHash, newSalt) = _passwordHasher.HashPassword(request.Password);
                    user.PasswordHash = newHash;
                    user.PasswordSalt = newSalt;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _uow.SaveChangesAsync();
                    isValid = true;
                }
            }

            if (!isValid)
                return Fail<AuthResponse>("Invalid credentials or inactive account.");

            var authResponse = _tokenService.GenerateToken(
                username: user.Email,
                role: user.Role.ToString(),
                email: user.Email,
                userId: user.UserId,
                fullName: user.FullName,
                employeeId: user.EmployeeId,
                departmentId: user.DepartmentId,
                departmentName: user.Department?.DepartmentName);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Login",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });
            await _uow.SaveChangesAsync();

            return Ok("Login successful.", authResponse);
        }

        public async Task<ApiResponseDto<AuthResponse>> RegisterAsync(RegisterRequestDto dto, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
                return Fail<AuthResponse>("Full name, email, and password are required.");

            var email = dto.Email.Trim().ToLowerInvariant();
            var employeeId = string.IsNullOrWhiteSpace(dto.EmployeeId)
                ? $"EMP-{DateTime.UtcNow.Ticks % 100000:D5}"
                : dto.EmployeeId.Trim();

            var exists = await _uow.Users.ExistsAsync(u => u.Email.ToLower() == email || u.EmployeeId == employeeId);
            if (exists)
                return Fail<AuthResponse>("User with this email or Employee ID already exists.");

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

            await _uow.Users.AddAsync(user);
            await _uow.SaveChangesAsync();

            await _uow.UserSettings.AddAsync(new UserSettings
            {
                UserId = user.UserId,
                Theme = "light",
                NotifyRequestUpdates = true,
                NotifyApprovalAlerts = true,
                NotifySLAWarnings = true,
                NotifyAssetEvents = false,
                NotifyEmailDigest = false,
                UpdatedAt = now
            });

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Registered",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = now
            });
            await _uow.SaveChangesAsync();

            var department = dto.DepartmentId.HasValue
                ? await _uow.Departments.GetByIdAsync(dto.DepartmentId.Value)
                : null;

            var authResponse = _tokenService.GenerateToken(
                username: user.Email,
                role: user.Role.ToString(),
                email: user.Email,
                userId: user.UserId,
                fullName: user.FullName,
                employeeId: user.EmployeeId,
                departmentId: user.DepartmentId,
                departmentName: department?.DepartmentName);

            return Ok("Registration successful.", authResponse);
        }

        public async Task<ApiResponseDto<object>> MeAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.UserSettings)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

            if (user == null)
                return Fail<object>("User not found.");

            object data = new
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
            };

            return Ok("Current user retrieved successfully.", data);
        }

        public async Task<ApiResponseDto<object>> ChangePasswordAsync(int userId, ChangePasswordDto dto, string? ipAddress)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null)
                return Fail<object>("User not found.");

            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                return Fail<object>("Current password is incorrect.");

            var (newHash, newSalt) = _passwordHasher.HashPassword(dto.NewPassword);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "Password Changed",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });
            await _uow.SaveChangesAsync();

            return Ok<object>("Password changed successfully.", null!);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
