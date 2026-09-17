using FluentValidation;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UserSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<UserSettingsDto> _validator;

        public UserSettingsController(
            AppDbContext context,
            IValidator<UserSettingsDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> GetSettings(
            int userId)
        {
            var settings = await _context.UserSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                return NotFound(new ApiResponseDto<UserSettingsDto>
                {
                    Success = false,
                    Message = "User settings not found.",
                    Data = null
                });
            }

            var data = new UserSettingsDto
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

            return Ok(new ApiResponseDto<UserSettingsDto>
            {
                Success = true,
                Message = "User settings fetched successfully.",
                Data = data
            });
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserSettingsDto>>> UpdateSettings(
            int userId,
            UserSettingsDto dto)
        {
            dto.UserId = userId;

            var validation = await _validator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<UserSettingsDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return NotFound(new ApiResponseDto<UserSettingsDto>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
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

            settings.Theme = dto.Theme.Trim().ToLowerInvariant();
            settings.TwoFactorEnabled = dto.TwoFactorEnabled;
            settings.NotifyRequestUpdates = dto.NotifyRequestUpdates;
            settings.NotifyApprovalAlerts = dto.NotifyApprovalAlerts;
            settings.NotifySLAWarnings = dto.NotifySLAWarnings;
            settings.NotifyAssetEvents = dto.NotifyAssetEvents;
            settings.NotifyEmailDigest = dto.NotifyEmailDigest;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            dto.Theme = settings.Theme;

            return Ok(new ApiResponseDto<UserSettingsDto>
            {
                Success = true,
                Message = "User settings updated successfully.",
                Data = dto
            });
        }
    }
}
