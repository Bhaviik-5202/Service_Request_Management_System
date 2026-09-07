using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateUserDto> _createValidator;
        private readonly IValidator<UpdateUserDto> _updateValidator;

        public UsersController(
            AppDbContext context,
            IValidator<CreateUserDto> createValidator,
            IValidator<UpdateUserDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserResponseDto>>>> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.UserId)
                .Select(UserResponseSelector)
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Users retrieved successfully.",
                Data = users
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(UserResponseSelector)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<UserResponseDto>
            {
                Success = true,
                Message = "User retrieved successfully.",
                Data = user
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> CreateUser(
            CreateUserDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = validationResult.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var email = dto.Email.Trim().ToLowerInvariant();
            var employeeId = dto.EmployeeId.Trim();

            var exists = await _context.Users
                .AnyAsync(u =>
                    u.Email == email ||
                    u.EmployeeId == employeeId);

            if (exists)
            {
                return BadRequest(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = "Email or employee ID already exists.",
                    Data = null
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<UserResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department.",
                        Data = null
                    });
                }
            }

            var now = DateTime.UtcNow;

            var user = new User
            {
                EmployeeId = employeeId,
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                Phone = string.IsNullOrWhiteSpace(dto.Phone)
                    ? null
                    : dto.Phone.Trim(),
                Status = UserStatus.Active,
                JoinedDate = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == user.UserId)
                .Select(UserResponseSelector)
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                new ApiResponseDto<UserResponseDto>
                {
                    Success = true,
                    Message = "User created successfully.",
                    Data = response
                });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> UpdateUser(
            int id,
            UpdateUserDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = validationResult.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponseDto>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<UserResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department.",
                        Data = null
                    });
                }
            }

            user.FullName = dto.FullName.Trim();
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone)
                ? null
                : dto.Phone.Trim();
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(UserResponseSelector)
                .FirstAsync();

            return Ok(new ApiResponseDto<UserResponseDto>
            {
                Success = true,
                Message = "User updated successfully.",
                Data = response
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                });
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User deleted successfully.",
                Data = null
            });
        }

        private static readonly System.Linq.Expressions.Expression<
            Func<User, UserResponseDto>> UserResponseSelector = u =>
            new UserResponseDto
            {
                UserId = u.UserId,
                EmployeeId = u.EmployeeId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null
                    ? u.Department.DepartmentName
                    : null,
                Phone = u.Phone,
                Status = u.Status,
                JoinedDate = u.JoinedDate,

                RequestsRaised = u.RequestedRequests.Count(),

                RequestsResolved = u.AssignedRequests.Count(
                    r => r.Status != null &&
                         r.Status.StatusName == RequestStatusNames.Resolved)
            };
    }
}
