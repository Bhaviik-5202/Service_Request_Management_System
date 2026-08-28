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
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserResponseDto>>>> GetUsers(
            [FromQuery] UserRole? role,
            [FromQuery] int? departmentId,
            [FromQuery] UserStatus? status,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Users.Include(u => u.Department).AsQueryable();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId.Value);

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s) || u.EmployeeId.ToLower().Contains(s));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    RequestsRaised = _context.ServiceRequests.Count(r => r.RequesterUserId == u.UserId),
                    RequestsResolved = _context.ServiceRequests.Count(r => r.AssigneeUserId == u.UserId && r.Status != null && r.Status.StatusName == "Resolved")
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Users fetched successfully.",
                Data = users,
                Pagination = new PaginationMetadataDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new ApiResponseDto<UserResponseDto> { Success = false, Message = "User not found." });

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
                JoinedDate = user.JoinedDate,
                RequestsRaised = await _context.ServiceRequests.CountAsync(r => r.RequesterUserId == user.UserId),
                RequestsResolved = await _context.ServiceRequests.CountAsync(r => r.AssigneeUserId == user.UserId && r.Status != null && r.Status.StatusName == "Resolved")
            };

            return Ok(new ApiResponseDto<UserResponseDto> { Success = true, Data = response });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<UserResponseDto> { Success = false, Message = "Invalid payload." });

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email || u.EmployeeId == dto.EmployeeId);
            if (exists)
                return BadRequest(new ApiResponseDto<UserResponseDto> { Success = false, Message = "User with this email or employee ID already exists." });

            var user = new User
            {
                EmployeeId = dto.EmployeeId,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                Phone = dto.Phone,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserSettings.Add(new UserSettings { UserId = user.UserId, Theme = "light", UpdatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                Phone = user.Phone,
                Status = user.Status,
                JoinedDate = user.JoinedDate
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new ApiResponseDto<UserResponseDto> { Success = true, Message = "User created successfully.", Data = response });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ApiResponseDto<UserResponseDto> { Success = false, Message = "User not found." });

            user.FullName = dto.FullName;
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.Phone = dto.Phone;
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                Phone = user.Phone,
                Status = user.Status,
                JoinedDate = user.JoinedDate
            };

            return Ok(new ApiResponseDto<UserResponseDto> { Success = true, Message = "User updated successfully.", Data = response });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "User not found." });

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool> { Success = true, Message = "User soft deleted successfully.", Data = true });
        }
    }
}
