using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;

namespace ServiceRequestManagementSystem.API.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<UserSettings> UserSettings { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<DepartmentPersonnel> DepartmentPersonnel { get; }
        IGenericRepository<ServiceType> ServiceTypes { get; }
        IGenericRepository<RequestType> RequestTypes { get; }
        IGenericRepository<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; }
        IGenericRepository<ServiceRequestStatus> ServiceRequestStatuses { get; }
        IGenericRepository<ServiceRequest> ServiceRequests { get; }
        IGenericRepository<ServiceRequestReply> ServiceRequestReplies { get; }
        IGenericRepository<ServiceRequestTimeline> ServiceRequestTimeline { get; }
        IGenericRepository<ServiceRequestAttachment> ServiceRequestAttachments { get; }
        IGenericRepository<Approval> Approvals { get; }
        IGenericRepository<Asset> Assets { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync();
    }
}
