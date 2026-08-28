namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceType
    {
        public int ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; } = string.Empty;

        public string ServiceTypeCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<RequestType> RequestTypes { get; set; }
            = new List<RequestType>();

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
            = new List<ServiceRequest>();
    }
}
