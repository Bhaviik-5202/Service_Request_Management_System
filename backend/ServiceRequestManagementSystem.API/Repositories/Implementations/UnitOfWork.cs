using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;

namespace ServiceRequestManagementSystem.API.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IGenericRepository<User>? _users;
        private IGenericRepository<UserSettings>? _userSettings;
        private IGenericRepository<Department>? _departments;
        private IGenericRepository<DepartmentPersonnel>? _departmentPersonnel;
        private IGenericRepository<ServiceType>? _serviceTypes;
        private IGenericRepository<RequestType>? _requestTypes;
        private IGenericRepository<RequestTypeTechnicianMapping>? _requestTypeTechnicianMappings;
        private IGenericRepository<ServiceRequestStatus>? _serviceRequestStatuses;
        private IGenericRepository<ServiceRequest>? _serviceRequests;
        private IGenericRepository<ServiceRequestReply>? _serviceRequestReplies;
        private IGenericRepository<ServiceRequestTimeline>? _serviceRequestTimeline;
        private IGenericRepository<ServiceRequestAttachment>? _serviceRequestAttachments;
        private IGenericRepository<Approval>? _approvals;
        private IGenericRepository<Asset>? _assets;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<AuditLog>? _auditLogs;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<User> Users
            => _users ??= new GenericRepository<User>(_context);

        public IGenericRepository<UserSettings> UserSettings
            => _userSettings ??= new GenericRepository<UserSettings>(_context);

        public IGenericRepository<Department> Departments
            => _departments ??= new GenericRepository<Department>(_context);

        public IGenericRepository<DepartmentPersonnel> DepartmentPersonnel
            => _departmentPersonnel ??= new GenericRepository<DepartmentPersonnel>(_context);

        public IGenericRepository<ServiceType> ServiceTypes
            => _serviceTypes ??= new GenericRepository<ServiceType>(_context);

        public IGenericRepository<RequestType> RequestTypes
            => _requestTypes ??= new GenericRepository<RequestType>(_context);

        public IGenericRepository<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings
            => _requestTypeTechnicianMappings ??= new GenericRepository<RequestTypeTechnicianMapping>(_context);

        public IGenericRepository<ServiceRequestStatus> ServiceRequestStatuses
            => _serviceRequestStatuses ??= new GenericRepository<ServiceRequestStatus>(_context);

        public IGenericRepository<ServiceRequest> ServiceRequests
            => _serviceRequests ??= new GenericRepository<ServiceRequest>(_context);

        public IGenericRepository<ServiceRequestReply> ServiceRequestReplies
            => _serviceRequestReplies ??= new GenericRepository<ServiceRequestReply>(_context);

        public IGenericRepository<ServiceRequestTimeline> ServiceRequestTimeline
            => _serviceRequestTimeline ??= new GenericRepository<ServiceRequestTimeline>(_context);

        public IGenericRepository<ServiceRequestAttachment> ServiceRequestAttachments
            => _serviceRequestAttachments ??= new GenericRepository<ServiceRequestAttachment>(_context);

        public IGenericRepository<Approval> Approvals
            => _approvals ??= new GenericRepository<Approval>(_context);

        public IGenericRepository<Asset> Assets
            => _assets ??= new GenericRepository<Asset>(_context);

        public IGenericRepository<Notification> Notifications
            => _notifications ??= new GenericRepository<Notification>(_context);

        public IGenericRepository<AuditLog> AuditLogs
            => _auditLogs ??= new GenericRepository<AuditLog>(_context);

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}
