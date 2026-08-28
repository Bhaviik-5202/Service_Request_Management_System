namespace ServiceRequestManagementSystem.API.Models
{
    public class RequestTypeTechnicianMapping
    {
        public int MappingId { get; set; }

        public int RequestTypeId { get; set; }

        public int DepartmentPersonnelId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual RequestType? RequestType { get; set; }

        public virtual DepartmentPersonnel? DepartmentPersonnel { get; set; }
    }
}
