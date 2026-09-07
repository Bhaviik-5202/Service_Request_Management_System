namespace ServiceRequestManagementSystem.API.Models
{
    public class RequestType
    {
        public int RequestTypeId { get; set; }

        public int ServiceTypeId { get; set; }

        public string RequestTypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool RequiresApproval { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ServiceType? ServiceType { get; set; }

        public ICollection<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = new List<RequestTypeTechnicianMapping>();

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
