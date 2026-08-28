using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserSettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> GetSettings(int userId)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                return NotFound(new ApiResponseDto<UserSettingsDto>
                {
                    Success = false,
                    Message = "User settings not found."
                });
            }

            return Ok(new ApiResponseDto<UserSettingsDto>
            {
                Success = true,
                Message = "User settings fetched successfully.",
                Data = new UserSettingsDto
                {
                    UserId = settings.UserId,
                    Theme = settings.Theme,
                    TwoFactorEnabled = settings.TwoFactorEnabled,
                    NotifyRequestUpdates = settings.NotifyRequestUpdates,
                    NotifyApprovalAlerts = settings.NotifyApprovalAlerts,
                    NotifySLAWarnings = settings.NotifySLAWarnings,
                    NotifyAssetEvents = settings.NotifyAssetEvents,
                    NotifyEmailDigest = settings.NotifyEmailDigest
                }
            });
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> UpdateSettings(
            int userId,
            [FromBody] UserSettingsDto dto)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId && !u.IsDeleted);

            if (!userExists)
            {
                return NotFound(new ApiResponseDto<UserSettingsDto>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId
                };

                _context.UserSettings.Add(settings);
            }

            settings.Theme = dto.Theme;
            settings.TwoFactorEnabled = dto.TwoFactorEnabled;
            settings.NotifyRequestUpdates = dto.NotifyRequestUpdates;
            settings.NotifyApprovalAlerts = dto.NotifyApprovalAlerts;
            settings.NotifySLAWarnings = dto.NotifySLAWarnings;
            settings.NotifyAssetEvents = dto.NotifyAssetEvents;
            settings.NotifyEmailDigest = dto.NotifyEmailDigest;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            dto.UserId = userId;

            return Ok(new ApiResponseDto<UserSettingsDto>
            {
                Success = true,
                Message = "User settings updated successfully.",
                Data = dto
            });
        }
    }
}
