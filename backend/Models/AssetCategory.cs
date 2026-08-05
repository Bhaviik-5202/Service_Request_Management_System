using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Request_Management_System.Models
{
    [Table("AssetCategories")]
    public class AssetCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column(TypeName = "DATETIME2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
