using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("UserSettings")]
    public partial class UserSetting : IHasUpdatedAt
    {
        [Key]
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
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
