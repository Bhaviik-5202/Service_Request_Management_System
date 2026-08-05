using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        [MaxLength(20)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
