using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.UserSettings;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> GetSettings()
        {
            var user = await _context.Users.FirstOrDefaultAsync() ?? new User { UserId = 1 };
            var settings = await _context.UserSettings.FindAsync(user.UserId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = user.UserId,
                    Theme = "light",
                    NotifyRequestUpdates = true,
                    NotifyApprovalAlerts = true,
                    NotifySLAWarnings = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var dto = new UserSettingsDto
            {
                UserId = settings.UserId,
                Theme = settings.Theme,
                TwoFactorEnabled = settings.TwoFactorEnabled,
                NotifyRequestUpdates = settings.NotifyRequestUpdates,
                NotifyApprovalAlerts = settings.NotifyApprovalAlerts,
                NotifySLAWarnings = settings.NotifySLAWarnings,
                NotifyAssetEvents = settings.NotifyAssetEvents,
                NotifyEmailDigest = settings.NotifyEmailDigest
            };

            return Ok(new ApiResponseDto<UserSettingsDto> { Success = true, Data = dto });
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> UpdateSettings([FromBody] UserSettingsDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync() ?? new User { UserId = 1 };
            var settings = await _context.UserSettings.FindAsync(user.UserId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = user.UserId };
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

            return Ok(new ApiResponseDto<UserSettingsDto> { Success = true, Message = "Settings saved successfully.", Data = dto });
        }
    }
}
