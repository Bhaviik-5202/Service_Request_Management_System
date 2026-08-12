using System.ComponentModel.DataAnnotations;

namespace ServiceRequestManagementSystem.API.DTOs.UserSettings
{
    public class UserSettingsDto
    {
        public int UserId { get; set; }

        [Required]
        public string Theme { get; set; } = "light";

        public bool TwoFactorEnabled { get; set; }
        public bool NotifyRequestUpdates { get; set; }
        public bool NotifyApprovalAlerts { get; set; }
        public bool NotifySLAWarnings { get; set; }
        public bool NotifyAssetEvents { get; set; }
        public bool NotifyEmailDigest { get; set; }
    }
}
