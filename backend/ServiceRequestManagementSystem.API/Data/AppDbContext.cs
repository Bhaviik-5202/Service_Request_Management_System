using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
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

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(u => u.Phone)
                    .HasMaxLength(20);

                entity.Property(u => u.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(u => u.JoinedDate)
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .IsRequired();

                entity.Property(u => u.UpdatedAt)
                    .IsRequired();

                entity.Property(u => u.IsDeleted)
                    .IsRequired();

                entity.HasIndex(u => u.EmployeeId)
                    .IsUnique();

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(us => us.UserId);

                entity.Property(us => us.Theme)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(us => us.TwoFactorEnabled)
                    .IsRequired();

                entity.Property(us => us.NotifyRequestUpdates)
                    .IsRequired();

                entity.Property(us => us.NotifyApprovalAlerts)
                    .IsRequired();

                entity.Property(us => us.NotifySLAWarnings)
                    .IsRequired();

                entity.Property(us => us.NotifyAssetEvents)
                    .IsRequired();

                entity.Property(us => us.NotifyEmailDigest)
                    .IsRequired();

                entity.Property(us => us.UpdatedAt)
                    .IsRequired();

                entity.HasOne(us => us.User)
                    .WithOne(u => u.UserSettings)
                    .HasForeignKey<UserSettings>(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.DepartmentId);

                entity.Property(d => d.DepartmentName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(d => d.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(d => d.Description)
                    .HasMaxLength(250);

                entity.Property(d => d.IsActive)
                    .IsRequired();

                entity.Property(d => d.CreatedAt)
                    .IsRequired();

                entity.Property(d => d.UpdatedAt)
                    .IsRequired();

                entity.Property(d => d.IsDeleted)
                    .IsRequired();

                entity.HasIndex(d => d.DepartmentCode)
                    .IsUnique();

                entity.HasMany(d => d.Users)
                    .WithOne(u => u.Department)
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(d => !d.IsDeleted);
            });

            modelBuilder.Entity<DepartmentPersonnel>(entity =>
            {
                entity.HasKey(dp => dp.DepartmentPersonnelId);

                entity.Property(dp => dp.UserId)
                    .IsRequired();

                entity.Property(dp => dp.DepartmentId)
                    .IsRequired();

                entity.Property(dp => dp.IsHOD)
                    .IsRequired();

                entity.Property(dp => dp.IsActive)
                    .IsRequired();

                entity.Property(dp => dp.CreatedAt)
                    .IsRequired();

                entity.Property(dp => dp.UpdatedAt)
                    .IsRequired();

                entity.Property(dp => dp.IsDeleted)
                    .IsRequired();

                entity.HasOne(dp => dp.User)
                    .WithMany(u => u.DepartmentPersonnels)
                    .HasForeignKey(dp => dp.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dp => dp.Department)
                    .WithMany(d => d.DepartmentPersonnels)
                    .HasForeignKey(dp => dp.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(dp => new { dp.UserId, dp.DepartmentId })
                    .IsUnique();

                entity.HasQueryFilter(dp => !dp.IsDeleted);
            });

            modelBuilder.Entity<ServiceType>(entity =>
            {
                entity.HasKey(st => st.ServiceTypeId);

                entity.Property(st => st.ServiceTypeName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(st => st.ServiceTypeCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(st => st.Description)
                    .HasMaxLength(250);

                entity.Property(st => st.IsActive)
                    .IsRequired();

                entity.Property(st => st.CreatedAt)
                    .IsRequired();

                entity.Property(st => st.UpdatedAt)
                    .IsRequired();

                entity.Property(st => st.IsDeleted)
                    .IsRequired();

                entity.HasIndex(st => st.ServiceTypeCode)
                    .IsUnique();

                entity.HasQueryFilter(st => !st.IsDeleted);
            });

            modelBuilder.Entity<RequestType>(entity =>
            {
                entity.HasKey(rt => rt.RequestTypeId);

                entity.Property(rt => rt.ServiceTypeId)
                    .IsRequired();

                entity.Property(rt => rt.RequestTypeName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(rt => rt.Description)
                    .HasMaxLength(250);

                entity.Property(rt => rt.RequiresApproval)
                    .IsRequired();

                entity.Property(rt => rt.IsActive)
                    .IsRequired();

                entity.Property(rt => rt.CreatedAt)
                    .IsRequired();

                entity.Property(rt => rt.UpdatedAt)
                    .IsRequired();

                entity.Property(rt => rt.IsDeleted)
                    .IsRequired();

                entity.HasOne(rt => rt.ServiceType)
                    .WithMany(st => st.RequestTypes)
                    .HasForeignKey(rt => rt.ServiceTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(rt => new { rt.ServiceTypeId, rt.RequestTypeName })
                    .IsUnique();

                entity.HasQueryFilter(rt => !rt.IsDeleted);
            });

            modelBuilder.Entity<RequestTypeTechnicianMapping>(entity =>
            {
                entity.HasKey(m => m.MappingId);

                entity.Property(m => m.RequestTypeId)
                    .IsRequired();

                entity.Property(m => m.DepartmentPersonnelId)
                    .IsRequired();

                entity.Property(m => m.IsActive)
                    .IsRequired();

                entity.Property(m => m.CreatedAt)
                    .IsRequired();

                entity.Property(m => m.UpdatedAt)
                    .IsRequired();

                entity.Property(m => m.IsDeleted)
                    .IsRequired();

                entity.HasOne(m => m.RequestType)
                    .WithMany(rt => rt.RequestTypeTechnicianMappings)
                    .HasForeignKey(m => m.RequestTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.DepartmentPersonnel)
                    .WithMany(dp => dp.RequestTypeTechnicianMappings)
                    .HasForeignKey(m => m.DepartmentPersonnelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => new { m.RequestTypeId, m.DepartmentPersonnelId })
                    .IsUnique();

                entity.HasQueryFilter(m => !m.IsDeleted);
            });

            modelBuilder.Entity<ServiceRequestStatus>(entity =>
            {
                entity.HasKey(s => s.StatusId);

                entity.Property(s => s.StatusName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.ColorCode)
                    .HasMaxLength(100);

                entity.Property(s => s.Description)
                    .HasMaxLength(250);

                entity.Property(s => s.IsActive)
                    .IsRequired();

                entity.Property(s => s.CreatedAt)
                    .IsRequired();

                entity.HasIndex(s => s.StatusName)
                    .IsUnique();
            });

            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(sr => sr.RequestId);

                entity.Property(sr => sr.RequestNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(sr => sr.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(sr => sr.Description)
                    .IsRequired();

                entity.Property(sr => sr.ServiceTypeId)
                    .IsRequired();

                entity.Property(sr => sr.RequestTypeId)
                    .IsRequired();

                entity.Property(sr => sr.DepartmentId)
                    .IsRequired();

                entity.Property(sr => sr.RequesterUserId)
                    .IsRequired();

                entity.Property(sr => sr.StatusId)
                    .IsRequired();

                entity.Property(sr => sr.Priority)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(sr => sr.CreatedAt)
                    .IsRequired();

                entity.Property(sr => sr.UpdatedAt)
                    .IsRequired();

                entity.Property(sr => sr.IsDeleted)
                    .IsRequired();

                entity.HasOne(sr => sr.Requester)
                    .WithMany(u => u.RequestedRequests)
                    .HasForeignKey(sr => sr.RequesterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Assignee)
                    .WithMany(u => u.AssignedRequests)
                    .HasForeignKey(sr => sr.AssigneeUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.ServiceType)
                    .WithMany(st => st.ServiceRequests)
                    .HasForeignKey(sr => sr.ServiceTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.RequestType)
                    .WithMany(rt => rt.ServiceRequests)
                    .HasForeignKey(sr => sr.RequestTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Department)
                    .WithMany(d => d.ServiceRequests)
                    .HasForeignKey(sr => sr.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Status)
                    .WithMany(s => s.ServiceRequests)
                    .HasForeignKey(sr => sr.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(sr => sr.RequestNumber)
                    .IsUnique();

                entity.HasQueryFilter(sr => !sr.IsDeleted);
            });

            modelBuilder.Entity<ServiceRequestReply>(entity =>
            {
                entity.HasKey(r => r.ReplyId);

                entity.Property(r => r.RequestId)
                    .IsRequired();

                entity.Property(r => r.AuthorUserId)
                    .IsRequired();

                entity.Property(r => r.Message)
                    .IsRequired();

                entity.Property(r => r.CreatedAt)
                    .IsRequired();

                entity.HasOne(r => r.ServiceRequest)
                    .WithMany(sr => sr.Replies)
                    .HasForeignKey(r => r.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Author)
                    .WithMany(u => u.Replies)
                    .HasForeignKey(r => r.AuthorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.StatusTransition)
                    .WithMany()
                    .HasForeignKey(r => r.StatusTransitionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ServiceRequestTimeline>(entity =>
            {
                entity.HasKey(t => t.TimelineId);

                entity.Property(t => t.RequestId)
                    .IsRequired();

                entity.Property(t => t.StatusName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.ChangedByUserId)
                    .IsRequired();

                entity.Property(t => t.ChangedAt)
                    .IsRequired();

                entity.Property(t => t.Note)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(t => t.ServiceRequest)
                    .WithMany(sr => sr.TimelineEntries)
                    .HasForeignKey(t => t.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.ChangedBy)
                    .WithMany(u => u.TimelineEntries)
                    .HasForeignKey(t => t.ChangedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ServiceRequestAttachment>(entity =>
            {
                entity.HasKey(a => a.AttachmentId);

                entity.Property(a => a.RequestId)
                    .IsRequired();

                entity.Property(a => a.FileName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(a => a.FileSizeKB)
                    .IsRequired();

                entity.Property(a => a.FileUrl)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.Property(a => a.UploadedByUserId)
                    .IsRequired();

                entity.Property(a => a.UploadedAt)
                    .IsRequired();

                entity.HasOne(a => a.ServiceRequest)
                    .WithMany(sr => sr.Attachments)
                    .HasForeignKey(a => a.RequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Reply)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(a => a.ReplyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.UploadedBy)
                    .WithMany(u => u.UploadedAttachments)
                    .HasForeignKey(a => a.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(a => a.ApprovalId);

                entity.Property(a => a.RequestId)
                    .IsRequired();

                entity.Property(a => a.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(a => a.Remarks)
                    .HasMaxLength(1000);

                entity.Property(a => a.SubmittedAt)
                    .IsRequired();

                entity.HasOne(a => a.ServiceRequest)
                    .WithOne(sr => sr.Approval)
                    .HasForeignKey<Approval>(a => a.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.DecidedBy)
                    .WithMany(u => u.DecidedApprovals)
                    .HasForeignKey(a => a.DecidedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(a => a.AssetId);

                entity.Property(a => a.AssetTag)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(a => a.AssetName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.Category)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(a => a.SerialNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(a => a.PurchaseDate)
                    .IsRequired();

                entity.Property(a => a.WarrantyUntil)
                    .IsRequired();

                entity.Property(a => a.BookValue)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(a => a.CreatedAt)
                    .IsRequired();

                entity.Property(a => a.UpdatedAt)
                    .IsRequired();

                entity.Property(a => a.IsDeleted)
                    .IsRequired();

                entity.HasOne(a => a.AssignedTo)
                    .WithMany(u => u.AssignedAssets)
                    .HasForeignKey(a => a.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Department)
                    .WithMany(d => d.Assets)
                    .HasForeignKey(a => a.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(a => a.AssetTag)
                    .IsUnique();

                entity.HasIndex(a => a.SerialNumber)
                    .IsUnique();

                entity.HasQueryFilter(a => !a.IsDeleted);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);

                entity.Property(n => n.UserId)
                    .IsRequired();

                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(n => n.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(n => n.IsRead)
                    .IsRequired();

                entity.Property(n => n.NotificationType)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(n => n.CreatedAt)
                    .IsRequired();

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(al => al.AuditLogId);

                entity.Property(al => al.Action)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(al => al.TargetType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(al => al.TargetId)
                    .HasMaxLength(50);

                entity.Property(al => al.TargetDisplay)
                    .HasMaxLength(100);

                entity.Property(al => al.IpAddress)
                    .HasMaxLength(45);

                entity.Property(al => al.CreatedAt)
                    .IsRequired();

                entity.HasOne(al => al.Actor)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(al => al.ActorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
