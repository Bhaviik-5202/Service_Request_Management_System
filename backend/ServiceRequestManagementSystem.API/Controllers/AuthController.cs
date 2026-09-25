using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(request, ipAddress);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponse>>> Register([FromBody] RegisterRequestDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RegisterAsync(dto, ipAddress);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(Me), result);
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

            var result = await _authService.MeAsync(userId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponseDto<object>>> ChangePassword([FromBody] ChangePasswordDto dto)
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

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.ChangePasswordAsync(userId, dto, ipAddress);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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
