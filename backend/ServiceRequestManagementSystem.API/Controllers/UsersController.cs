using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserResponseDto>>>> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] int? departmentId = null)
        {
            var result = await _userService.GetUsersAsync(search, role, departmentId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetUser(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> CreateUser([FromBody] CreateUserDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _userService.CreateUserAsync(dto, ipAddress);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetUser), new { id = result.Data?.UserId }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _userService.UpdateUserAsync(id, dto, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "User not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteUser(int id)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _userService.DeleteUserAsync(id, ipAddress);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
