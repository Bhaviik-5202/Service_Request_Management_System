namespace ServiceRequestManagementSystem.API.DTOs.AuditLogs
{
    public class AuditLogResponseDto
    {
        public long AuditLogId { get; set; }
        public int? ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string? TargetId { get; set; }
        public string? TargetDisplay { get; set; }
        public string? Detail { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
