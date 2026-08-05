using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        [Column(Order = 0)]
        public int RoleId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int PermissionId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        [JsonIgnore]
        public virtual Permission Permission { get; set; } = null!;
    }
}
