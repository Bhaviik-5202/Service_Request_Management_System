using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Roles")]
    public partial class Role : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        
    }
}
