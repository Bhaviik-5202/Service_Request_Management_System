using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUnitOfWork uow, AppDbContext context, IPasswordHasher passwordHasher)
        {
            _uow = uow;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponseDto<IEnumerable<UserResponseDto>>> GetUsersAsync(
            string? search, string? role, int? departmentId)
        {
            var query = _context.Users.AsNoTracking().Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    u.EmployeeId.ToLower().Contains(s) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
                query = query.Where(u => u.Role == userRole);

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId.Value);

            var users = await query
                .OrderBy(u => u.UserId)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                    Phone = u.Phone,
                    Status = u.Status,
                    JoinedDate = u.JoinedDate,
                    RequestsRaised = u.RequestedRequests.Count(),
                    RequestsResolved = u.AssignedRequests.Count(r =>
                        r.Status != null && r.Status.StatusName == RequestStatusNames.Resolved)
                })
                .ToListAsync();

            return Ok("Users retrieved successfully.", (IEnumerable<UserResponseDto>)users);
        }

        public async Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == id && !u.IsDeleted)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                    Phone = u.Phone,
                    Status = u.Status,
                    JoinedDate = u.JoinedDate,
                    RequestsRaised = u.RequestedRequests.Count(),
                    RequestsResolved = u.AssignedRequests.Count(r =>
                        r.Status != null && r.Status.StatusName == RequestStatusNames.Resolved)
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return Fail<UserResponseDto>("User not found.");

            return Ok("User retrieved successfully.", user);
        }

        public async Task<ApiResponseDto<UserResponseDto>> CreateUserAsync(CreateUserDto dto, string? ipAddress)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var employeeId = dto.EmployeeId.Trim();

            var exists = await _uow.Users.ExistsAsync(u =>
                (u.Email == email || u.EmployeeId == employeeId) && !u.IsDeleted);

            if (exists)
                return Fail<UserResponseDto>("Email or employee ID already exists.");

            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _uow.Departments.ExistsAsync(d =>
                    d.DepartmentId == dto.DepartmentId.Value && d.IsActive && !d.IsDeleted);
                if (!deptExists)
                    return Fail<UserResponseDto>("Invalid or inactive department.");
            }

            var now = DateTime.UtcNow;
            var (hash, salt) = _passwordHasher.HashPassword("Password@123");

            var user = new User
            {
                EmployeeId = employeeId,
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
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
                Action = "User Created",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = now
            });
            await _uow.SaveChangesAsync();

            return await GetUserByIdAsync(user.UserId);
        }

        public async Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(int id, UpdateUserDto dto, string? ipAddress)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);
            if (user == null)
                return Fail<UserResponseDto>("User not found.");

            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _uow.Departments.ExistsAsync(d =>
                    d.DepartmentId == dto.DepartmentId.Value && d.IsActive && !d.IsDeleted);
                if (!deptExists)
                    return Fail<UserResponseDto>("Invalid or inactive department.");
            }

            user.FullName = dto.FullName.Trim();
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Updated",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });
            await _uow.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<ApiResponseDto<object>> DeleteUserAsync(int id, string? ipAddress)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);
            if (user == null)
                return Fail<object>("User not found.");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = user.UserId,
                Action = "User Deleted",
                TargetType = "User",
                TargetId = user.UserId.ToString(),
                TargetDisplay = user.FullName,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });
            await _uow.SaveChangesAsync();

            return Ok<object>("User deleted successfully.", null!);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
