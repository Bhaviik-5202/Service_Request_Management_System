using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            const string email = "admin@gmail.com";
            const string password = "admin123";

            if ((request.Email == email) && request.Password == password)
            {
                var token = _tokenService.GenerateToken(
                    username: email,
                    email: email,
                    userId: 1,
                    role: "Admin",
                    fullName: "System Admin"
                );

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = token
                });
            }

            return Unauthorized(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Invalid username/email or password."
            });
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Current user retrieved successfully.",
                Data = new
                {
                    Email = User.FindFirst(ClaimTypes.Email)?.Value,
                    FullName = User.FindFirst("fullName")?.Value,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value,
                    UserId = User.FindFirst("userId")?.Value
                }
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Protected data accessed successfully.",
                Data = new
                {
                    Id = 1,
                    Name = "Admin",
                    Email = "admin@gmail.com",
                    Role = "Admin"
                }
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
                    Access = "Public"
                }
            });
        }
    }
}
