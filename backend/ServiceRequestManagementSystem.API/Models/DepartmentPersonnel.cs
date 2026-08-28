namespace ServiceRequestManagementSystem.API.Models
{
    public class DepartmentPersonnel
    {
        public int DepartmentPersonnelId { get; set; }

        public int UserId { get; set; }

        public int DepartmentId { get; set; }

        public bool IsHOD { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual User? User { get; set; }

        public virtual Department? Department { get; set; }

        public virtual ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; }
            = new List<RequestTypeTechnicianMapping>();
    }
}
