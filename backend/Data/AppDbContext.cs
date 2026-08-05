using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<DepartmentPersonnel> DepartmentPersonnel { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; }
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<RequestTypeTechnicianMapping> RequestTypeTechnicianMappings { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestReply> ServiceRequestReplies { get; set; }
        public DbSet<ServiceRequestTimeline> ServiceRequestTimelines { get; set; }
        public DbSet<ServiceRequestAttachment> ServiceRequestAttachments { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ ROLES ============
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).UseIdentityColumn();

                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ============ PERMISSIONS ============
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
                entity.Property(e => e.PermissionId).UseIdentityColumn();

                entity.HasIndex(e => e.PermissionKey).IsUnique();
                entity.Property(e => e.PermissionKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ============ ROLE PERMISSIONS ============
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============ DEPARTMENTS ============
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);
                entity.Property(e => e.DepartmentId).UseIdentityColumn();

                entity.HasIndex(e => e.DepartmentName).IsUnique();
                entity.HasIndex(e => e.DepartmentCode).IsUnique()
                    .HasFilter("IsDeleted = 0");

                entity.Property(e => e.DepartmentName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DepartmentCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany(u => u.DeletedDepartments)
                    .HasForeignKey(e => e.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ USERS ============
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).UseIdentityColumn();

                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique()
                    .HasFilter("IsDeleted = 0");

                // Index for authentication
                entity.HasIndex(e => e.Email)
                    .HasDatabaseName("IX_Users_Email")
                    .HasFilter("IsDeleted = 0")
                    .IncludeProperties(p => new { p.PasswordHash, p.RoleId, p.Status });

                entity.HasIndex(e => e.EmployeeId).IsUnique();

                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(15).HasDefaultValue("Active");
                entity.Property(e => e.JoinedDate).HasDefaultValueSql("CAST(GETDATE() AS DATE)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.ToTable(t => t.HasCheckConstraint("CK_Users_Status", "Status IN ('Active', 'Inactive')"));

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Users)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ============ USER SETTINGS ============
            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Theme).IsRequired().HasMaxLength(10).HasDefaultValue("light");
                entity.Property(e => e.TwoFactorEnabled).HasDefaultValue(false);
                entity.Property(e => e.NotifyRequestUpdates).HasDefaultValue(true);
                entity.Property(e => e.NotifyApprovalAlerts).HasDefaultValue(true);
                entity.Property(e => e.NotifySLAWarnings).HasDefaultValue(true);
                entity.Property(e => e.NotifyAssetEvents).HasDefaultValue(false);
                entity.Property(e => e.NotifyEmailDigest).HasDefaultValue(false);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.ToTable(t => t.HasCheckConstraint("CK_UserSettings_Theme", "Theme IN ('light', 'dark')"));

                entity.HasOne(e => e.User)
                    .WithOne(u => u.UserSetting)
                    .HasForeignKey<UserSetting>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============ DEPARTMENT PERSONNEL ============
            modelBuilder.Entity<DepartmentPersonnel>(entity =>
            {
                entity.HasKey(e => e.DepartmentPersonnelId);
                entity.Property(e => e.DepartmentPersonnelId).UseIdentityColumn();

                entity.HasIndex(e => new { e.UserId, e.DepartmentId })
                    .IsUnique()
                    .HasFilter("IsDeleted = 0")
                    .HasDatabaseName("UIX_DeptPersonnel_UserDept");

                entity.Property(e => e.IsHOD).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.DepartmentPersonnel)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.DepartmentPersonnel)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE TYPES ============
            modelBuilder.Entity<ServiceType>(entity =>
            {
                entity.HasKey(e => e.ServiceTypeId);
                entity.Property(e => e.ServiceTypeId).UseIdentityColumn();

                entity.HasIndex(e => e.ServiceTypeName).IsUnique();
                entity.HasIndex(e => e.ServiceTypeCode).IsUnique();

                entity.Property(e => e.ServiceTypeName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ServiceTypeCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // ============ REQUEST TYPES ============
            modelBuilder.Entity<RequestType>(entity =>
            {
                entity.HasKey(e => e.RequestTypeId);
                entity.Property(e => e.RequestTypeId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestTypeName).IsUnique();
                entity.HasIndex(e => e.ServiceTypeId).HasDatabaseName("IX_RequestTypes_ServiceTypeId");

                entity.Property(e => e.RequestTypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.ServiceType)
                    .WithMany(st => st.RequestTypes)
                    .HasForeignKey(e => e.ServiceTypeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE REQUEST STATUSES ============
            modelBuilder.Entity<ServiceRequestStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);
                entity.Property(e => e.StatusId).UseIdentityColumn();

                entity.HasIndex(e => e.StatusName).IsUnique();

                entity.Property(e => e.StatusName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ColorCode).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ============ ASSET CATEGORIES ============
            modelBuilder.Entity<AssetCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).UseIdentityColumn();

                entity.HasIndex(e => e.CategoryName).IsUnique();

                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ============ ASSETS ============
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.AssetId);
                entity.Property(e => e.AssetId).UseIdentityColumn();

                entity.HasIndex(e => e.AssetTag).IsUnique()
                    .HasFilter("IsDeleted = 0")
                    .HasDatabaseName("IX_Assets_Tag");

                entity.HasIndex(e => e.SerialNumber).IsUnique();

                entity.HasIndex(e => e.AssignedToUserId)
                    .HasDatabaseName("IX_Assets_AssignedTo")
                    .HasFilter("AssignedToUserId IS NOT NULL");

                entity.Property(e => e.AssetTag).IsRequired().HasMaxLength(30);
                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Available");
                entity.Property(e => e.PurchaseDate).IsRequired();
                entity.Property(e => e.WarrantyUntil).IsRequired();
                entity.Property(e => e.BookValue).HasColumnType("DECIMAL(18,2)").HasDefaultValue(0.00m);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.ToTable(t => t.HasCheckConstraint("CK_Assets_Status", "Status IN ('In Use', 'Available', 'Under Repair', 'Retired')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Assets_Warranty", "WarrantyUntil >= PurchaseDate"));

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Assets)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.AssignedToUser)
                    .WithMany(u => u.AssignedAssets)
                    .HasForeignKey(e => e.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Assets)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ============ REQUEST TYPE TECHNICIAN MAPPINGS ============
            modelBuilder.Entity<RequestTypeTechnicianMapping>(entity =>
            {
                entity.HasKey(e => e.MappingId);
                entity.Property(e => e.MappingId).UseIdentityColumn();

                entity.HasIndex(e => new { e.RequestTypeId, e.DepartmentPersonnelId })
                    .IsUnique()
                    .HasFilter("IsDeleted = 0")
                    .HasDatabaseName("UIX_ReqTypeTech_Mapping");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.RequestType)
                    .WithMany(rt => rt.RequestTypeTechnicianMappings)
                    .HasForeignKey(e => e.RequestTypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.DepartmentPersonnel)
                    .WithMany(dp => dp.RequestTypeTechnicianMappings)
                    .HasForeignKey(e => e.DepartmentPersonnelId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE REQUESTS ============
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.RequestId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestNumber)
                    .IsUnique()
                    .HasFilter("IsDeleted = 0")
                    .HasDatabaseName("IX_ServiceRequests_RequestNumber");

                entity.HasIndex(e => e.RequesterUserId).HasDatabaseName("IX_ServiceRequests_Requester");
                entity.HasIndex(e => e.AssigneeUserId)
                    .HasDatabaseName("IX_ServiceRequests_Assignee")
                    .HasFilter("AssigneeUserId IS NOT NULL");
                entity.HasIndex(e => e.StatusId).HasDatabaseName("IX_ServiceRequests_Status");
                entity.HasIndex(e => e.DepartmentId).HasDatabaseName("IX_ServiceRequests_Dept");

                entity.Property(e => e.RequestNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(15).HasDefaultValue("Medium");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.ToTable(t => t.HasCheckConstraint("CK_ServiceRequests_Priority", "Priority IN ('Critical', 'High', 'Medium', 'Low')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_ServiceRequests_Desc_Len", "LEN(TRIM(Description)) >= 20"));

                entity.HasOne(e => e.ServiceType)
                    .WithMany(st => st.ServiceRequests)
                    .HasForeignKey(e => e.ServiceTypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.RequestType)
                    .WithMany(rt => rt.ServiceRequests)
                    .HasForeignKey(e => e.RequestTypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.ServiceRequests)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.RequesterUser)
                    .WithMany(u => u.RequestedServiceRequests)
                    .HasForeignKey(e => e.RequesterUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.AssigneeUser)
                    .WithMany(u => u.AssignedServiceRequests)
                    .HasForeignKey(e => e.AssigneeUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Status)
                    .WithMany(s => s.ServiceRequests)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedServiceRequests)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany(u => u.UpdatedServiceRequests)
                    .HasForeignKey(e => e.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany(u => u.DeletedServiceRequests)
                    .HasForeignKey(e => e.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE REQUEST REPLIES ============
            modelBuilder.Entity<ServiceRequestReply>(entity =>
            {
                entity.HasKey(e => e.ReplyId);
                entity.Property(e => e.ReplyId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_Replies_RequestId");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(e => e.ServiceRequest)
                    .WithMany(sr => sr.Replies)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AuthorUser)
                    .WithMany(u => u.ServiceRequestReplies)
                    .HasForeignKey(e => e.AuthorUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.StatusTransition)
                    .WithMany(s => s.ServiceRequestReplies)
                    .HasForeignKey(e => e.StatusTransitionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE REQUEST TIMELINE ============
            modelBuilder.Entity<ServiceRequestTimeline>(entity =>
            {
                entity.HasKey(e => e.TimelineId);
                entity.Property(e => e.TimelineId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_Timeline_RequestId");

                entity.Property(e => e.Note).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(e => e.ServiceRequest)
                    .WithMany(sr => sr.Timelines)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Status)
                    .WithMany(s => s.ServiceRequestTimelines)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ChangedByUser)
                    .WithMany(u => u.ServiceRequestTimelines)
                    .HasForeignKey(e => e.ChangedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ SERVICE REQUEST ATTACHMENTS ============
            modelBuilder.Entity<ServiceRequestAttachment>(entity =>
            {
                entity.HasKey(e => e.AttachmentId);
                entity.Property(e => e.AttachmentId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_Attachments_RequestId");
                entity.HasIndex(e => e.ReplyId)
                    .HasDatabaseName("IX_Attachments_ReplyId")
                    .HasFilter("ReplyId IS NOT NULL");

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FileSizeKB).IsRequired();
                entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(2048);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(e => e.ServiceRequest)
                    .WithMany(sr => sr.Attachments)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Reply)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(e => e.ReplyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedByUser)
                    .WithMany(u => u.ServiceRequestAttachments)
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ APPROVALS ============
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);
                entity.Property(e => e.ApprovalId).UseIdentityColumn();

                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_Approvals_RequestId");
                entity.HasIndex(e => e.DecidedByUserId)
                    .HasDatabaseName("IX_Approvals_PendingHOD")
                    .HasFilter("Status = 'Pending'");

                entity.Property(e => e.Status).IsRequired().HasMaxLength(15).HasDefaultValue("Pending");
                entity.Property(e => e.Remarks).HasMaxLength(1000);
                entity.Property(e => e.SubmittedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.ToTable(t => t.HasCheckConstraint("CK_Approvals_Status", "Status IN ('Pending', 'Approved', 'Rejected')"));

                entity.HasOne(e => e.ServiceRequest)
                    .WithOne(sr => sr.Approval)
                    .HasForeignKey<Approval>(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DecidedByUser)
                    .WithMany(u => u.Approvals)
                    .HasForeignKey(e => e.DecidedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============ NOTIFICATIONS ============
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.NotificationId).UseIdentityColumn();

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Notifications_UnreadInbox")
                    .HasFilter("IsRead = 0");

                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.ToTable(t => t.HasCheckConstraint("CK_Notifications_Type", "NotificationType IN ('request', 'approval', 'asset', 'system')"));

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============ AUDIT LOGS ============
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.AuditLogId).UseIdentityColumn();

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_AuditLogs_Search")
                    .IncludeProperties(p => new { p.ActorUserId, p.Action, p.TargetDisplay });

                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TargetType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TargetId).HasMaxLength(50);
                entity.Property(e => e.TargetDisplay).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(e => e.ActorUser)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.ActorUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        // Override SaveChanges to automatically update UpdatedAt timestamps
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not null &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                // Update UpdatedAt for entities that have it
                if (entityEntry.Entity is IHasUpdatedAt updatedEntity)
                {
                    updatedEntity.UpdatedAt = DateTime.UtcNow;
                }

                // Set CreatedAt for new entities
                if (entityEntry.State == EntityState.Added && entityEntry.Entity is IHasCreatedAt createdEntity)
                {
                    createdEntity.CreatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }
    }

    // Interfaces for automatic timestamp management
    public interface IHasUpdatedAt
    {
        DateTime UpdatedAt { get; set; }
    }

    public interface IHasCreatedAt
    {
        DateTime CreatedAt { get; set; }
    }
}