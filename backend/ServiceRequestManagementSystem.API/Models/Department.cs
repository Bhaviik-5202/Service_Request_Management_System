namespace ServiceRequestManagementSystem.API.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<DepartmentPersonnel> DepartmentPersonnels { get; set; } = new List<DepartmentPersonnel>();

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
