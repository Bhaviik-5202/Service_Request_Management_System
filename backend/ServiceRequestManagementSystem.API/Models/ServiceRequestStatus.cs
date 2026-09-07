namespace ServiceRequestManagementSystem.API.Models
{
    public class ServiceRequestStatus
    {
        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? ColorCode { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
