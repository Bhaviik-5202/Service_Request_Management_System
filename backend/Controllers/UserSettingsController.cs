using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class UserSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserSettingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSetting>>> GetUserSettings()
        {
            try
            {
                return await _context.UserSettings
                    .Include(us => us.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/UserSettings/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserSetting>> GetUserSetting(int userId)
        {
            try
            {
                var userSetting = await _context.UserSettings
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.UserId == userId);

                if (userSetting == null)
                {
                    return NotFound($"User settings for User ID {userId} not found.");
                }

                return Ok(userSetting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/UserSettings/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserSetting>> GetUserSettingsByUserId(int userId)
        {
            try
            {
                var userSetting = await _context.UserSettings
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.UserId == userId);

                if (userSetting == null)
                {
                    return NotFound($"User settings for User ID {userId} not found.");
                }

                return Ok(userSetting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/UserSettings/5
        [HttpPut("{userId}")]
        public async Task<IActionResult> PutUserSetting(int userId, UserSetting userSetting)
        {
            if (userId != userSetting.UserId)
            {
                return BadRequest("User ID mismatch.");
            }

            try
            {
                var existingSetting = await _context.UserSettings.FindAsync(userId);
                if (existingSetting == null)
                {
                    return NotFound($"User settings for User ID {userId} not found.");
                }

                existingSetting.Theme = userSetting.Theme;
                existingSetting.TwoFactorEnabled = userSetting.TwoFactorEnabled;
                existingSetting.NotifyRequestUpdates = userSetting.NotifyRequestUpdates;
                existingSetting.NotifyApprovalAlerts = userSetting.NotifyApprovalAlerts;
                existingSetting.NotifySLAWarnings = userSetting.NotifySLAWarnings;
                existingSetting.NotifyAssetEvents = userSetting.NotifyAssetEvents;
                existingSetting.NotifyEmailDigest = userSetting.NotifyEmailDigest;
                existingSetting.UpdatedAt = DateTime.UtcNow;

                _context.Entry(existingSetting).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserSettingExistsAsync(userId))
                {
                    return NotFound($"User settings for User ID {userId} not found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<bool> UserSettingExistsAsync(int userId)
        {
            return await _context.UserSettings.AnyAsync(us => us.UserId == userId);
        }
    }
}