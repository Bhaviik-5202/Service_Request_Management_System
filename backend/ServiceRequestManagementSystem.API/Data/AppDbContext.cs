using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DepartmentPersonnel> DepartmentPersonnel { get; set; } = null!;
        public DbSet<ServiceType> ServiceTypes { get; set; } = null!;
        public DbSet<RequestType> RequestTypes { get; set; } = null!;
        public DbSet<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; } = null!;
        public DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
        public DbSet<ServiceRequestReply> ServiceRequestReplies { get; set; } = null!;
        public DbSet<ServiceRequestTimeline> ServiceRequestTimeline { get; set; } = null!;
        public DbSet<ServiceRequestAttachment> ServiceRequestAttachments { get; set; } = null!;
        public DbSet<Approval> Approvals { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum -> String Conversion

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Asset>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Approval>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationType)
                .HasConversion<string>();


            // Decimal Precision

            modelBuilder.Entity<Asset>()
                .Property(a => a.BookValue)
                .HasPrecision(18, 2);


            // User <-> UserSettings : One-to-One

            modelBuilder.Entity<UserSettings>()
                .HasKey(s => s.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserSettings)
                .WithOne(s => s.User)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceRequest <-> User : Requester

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Requester)
                .WithMany(u => u.RequestedRequests)
                .HasForeignKey(sr => sr.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> User : Assignee

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Assignee)
                .WithMany(u => u.AssignedRequests)
                .HasForeignKey(sr => sr.AssigneeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> Approval : One-to-One

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.ServiceRequest)
                .WithOne(sr => sr.Approval)
                .HasForeignKey<Approval>(a => a.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Approval <-> User : DecidedBy

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.DecidedBy)
                .WithMany(u => u.DecidedApprovals)
                .HasForeignKey(a => a.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> Replies

            modelBuilder.Entity<ServiceRequestReply>()
                .HasOne(r => r.ServiceRequest)
                .WithMany(sr => sr.Replies)
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceRequest <-> Timeline

            modelBuilder.Entity<ServiceRequestTimeline>()
                .HasOne(t => t.ServiceRequest)
                .WithMany(sr => sr.TimelineEntries)
                .HasForeignKey(t => t.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceRequest <-> Attachments

            modelBuilder.Entity<ServiceRequestAttachment>()
                .HasOne(a => a.ServiceRequest)
                .WithMany(sr => sr.Attachments)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reply <-> Attachments

            modelBuilder.Entity<ServiceRequestAttachment>()
                .HasOne(a => a.Reply)
                .WithMany(r => r.Attachments)
                .HasForeignKey(a => a.ReplyId)
                .OnDelete(DeleteBehavior.Restrict);

            // User <-> Attachments : UploadedBy

            modelBuilder.Entity<ServiceRequestAttachment>()
                .HasOne(a => a.UploadedBy)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User <-> Department

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // DepartmentPersonnel <-> User

            modelBuilder.Entity<DepartmentPersonnel>()
                .HasOne(dp => dp.User)
                .WithMany(u => u.DepartmentPersonnels)
                .HasForeignKey(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DepartmentPersonnel <-> Department

            modelBuilder.Entity<DepartmentPersonnel>()
                .HasOne(dp => dp.Department)
                .WithMany(d => d.DepartmentPersonnels)
                .HasForeignKey(dp => dp.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // RequestType <-> ServiceType

            modelBuilder.Entity<RequestType>()
                .HasOne(rt => rt.ServiceType)
                .WithMany(st => st.RequestTypes)
                .HasForeignKey(rt => rt.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Technician Mapping <-> RequestType

            modelBuilder.Entity<RequestTypeTechnicianMapping>()
                .HasOne(m => m.RequestType)
                .WithMany(rt => rt.RequestTypeTechnicianMappings)
                .HasForeignKey(m => m.RequestTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Technician Mapping <-> DepartmentPersonnel

            modelBuilder.Entity<RequestTypeTechnicianMapping>()
                .HasOne(m => m.DepartmentPersonnel)
                .WithMany(dp => dp.RequestTypeTechnicianMappings)
                .HasForeignKey(m => m.DepartmentPersonnelId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> ServiceType

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.ServiceType)
                .WithMany(st => st.ServiceRequests)
                .HasForeignKey(sr => sr.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> RequestType

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.RequestType)
                .WithMany(rt => rt.ServiceRequests)
                .HasForeignKey(sr => sr.RequestTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> Department

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Department)
                .WithMany(d => d.ServiceRequests)
                .HasForeignKey(sr => sr.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest <-> Status

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Status)
                .WithMany(st => st.ServiceRequests)
                .HasForeignKey(sr => sr.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Asset <-> User

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.AssignedTo)
                .WithMany(u => u.AssignedAssets)
                .HasForeignKey(a => a.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Asset <-> Department

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Department)
                .WithMany(d => d.Assets)
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // User <-> Notifications

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User <-> AuditLogs

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.Actor)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Unique Indexes

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DepartmentCode)
                .IsUnique();

            modelBuilder.Entity<ServiceType>()
                .HasIndex(st => st.ServiceTypeCode)
                .IsUnique();

            modelBuilder.Entity<RequestType>()
                .HasIndex(rt => new { rt.ServiceTypeId, rt.RequestTypeName })
                .IsUnique();

            modelBuilder.Entity<DepartmentPersonnel>()
                .HasIndex(dp => new { dp.UserId, dp.DepartmentId })
                .IsUnique();

            modelBuilder.Entity<RequestTypeTechnicianMapping>()
                .HasIndex(m => new { m.RequestTypeId, m.DepartmentPersonnelId })
                .IsUnique();

            modelBuilder.Entity<ServiceRequestStatus>()
                .HasIndex(s => s.StatusName)
                .IsUnique();

            modelBuilder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.RequestNumber)
                .IsUnique();

            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.AssetTag)
                .IsUnique();

            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.SerialNumber)
                .IsUnique();

            // Global Query Filters for Soft Delete

            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<Department>()
                .HasQueryFilter(d => !d.IsDeleted);

            modelBuilder.Entity<DepartmentPersonnel>()
                .HasQueryFilter(dp => !dp.IsDeleted);

            modelBuilder.Entity<ServiceType>()
                .HasQueryFilter(st => !st.IsDeleted);

            modelBuilder.Entity<RequestType>()
                .HasQueryFilter(rt => !rt.IsDeleted);

            modelBuilder.Entity<RequestTypeTechnicianMapping>()
                .HasQueryFilter(m => !m.IsDeleted);

            modelBuilder.Entity<ServiceRequest>()
                .HasQueryFilter(sr => !sr.IsDeleted);

            modelBuilder.Entity<Asset>()
                .HasQueryFilter(a => !a.IsDeleted);
        }
    }
}

/*
OnDelete() has 4 Types — Must Remember


Behavior	        Meaning
Cascade	            Parent delete → child automatically delete
Restrict	        Parent delete prevent/restrict if related records exist
SetNull	            Parent delete → FK becomes NULL
Default/NoAction	Database/provider default behavior


Cascade
→ UserSettings
→ Approval
→ Replies
→ Timeline
→ Notifications

Restrict
→ Most important references

SetNull
→ Asset AssignedTo
→ Asset Department
*/