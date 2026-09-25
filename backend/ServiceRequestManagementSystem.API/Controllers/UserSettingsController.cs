using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSettingsController : ControllerBase
    {
        private readonly IUserSettingsService _userSettingsService;

        public UserSettingsController(IUserSettingsService userSettingsService)
        {
            _userSettingsService = userSettingsService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> GetSettings(int userId)
        {
            var result = await _userSettingsService.GetSettingsAsync(userId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> UpdateSettings(int userId, [FromBody] UserSettingsDto dto)
        {
            var result = await _userSettingsService.UpdateSettingsAsync(userId, dto);
            if (!result.Success)
            {
                if (result.Message == "User not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
