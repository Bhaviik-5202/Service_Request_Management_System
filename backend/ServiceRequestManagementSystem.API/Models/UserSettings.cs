namespace ServiceRequestManagementSystem.API.Models
{
    public class UserSettings
    {
        public int UserId { get; set; }

        public string Theme { get; set; } = "light";

        public bool TwoFactorEnabled { get; set; } = false;

        public bool NotifyRequestUpdates { get; set; } = true;

        public bool NotifyApprovalAlerts { get; set; } = true;

        public bool NotifySLAWarnings { get; set; } = true;

        public bool NotifyAssetEvents { get; set; } = false;

        public bool NotifyEmailDigest { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
