namespace ServiceRequestManagementSystem.API.Models
{
    public class DepartmentPersonnel
    {
        public int DepartmentPersonnelId { get; set; }

        public int UserId { get; set; }

        public int DepartmentId { get; set; }

        public bool IsHOD { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }

        public Department? Department { get; set; }

        public ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = new List<RequestTypeTechnicianMapping>();
    }
}
