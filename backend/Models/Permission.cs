using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("Permissions")]
    public partial class Permission : IHasCreatedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionKey { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
