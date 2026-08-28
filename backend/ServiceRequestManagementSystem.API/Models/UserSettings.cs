using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceRequestManagementSystem.API.Models
{
    [Table("UserSettings")]
    public class UserSettings
    {
        [Key]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Theme { get; set; } = "light";

        [Required]
        public bool TwoFactorEnabled { get; set; } = false;

        [Required]
        public bool NotifyRequestUpdates { get; set; } = true;

        [Required]
        public bool NotifyApprovalAlerts { get; set; } = true;

        [Required]
        public bool NotifySLAWarnings { get; set; } = true;

        [Required]
        public bool NotifyAssetEvents { get; set; } = false;

        [Required]
        public bool NotifyEmailDigest { get; set; } = false;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
