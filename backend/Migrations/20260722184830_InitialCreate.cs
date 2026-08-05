using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Request_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                columns: table => new
                {
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServiceTypeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.ServiceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypes",
                columns: table => new
                {
                    RequestTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypes", x => x.RequestTypeId);
                    table.ForeignKey(
                        name: "FK_RequestTypes_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId");
                });

            migrationBuilder.CreateTable(
                name: "Approvals",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Pending"),
                    DecidedByUserId = table.Column<int>(type: "int", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvals", x => x.ApprovalId);
                    table.CheckConstraint("CK_Approvals_Status", "Status IN ('Pending', 'Approved', 'Rejected')");
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    PurchaseDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    WarrantyUntil = table.Column<DateTime>(type: "DATE", nullable: false),
                    BookValue = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false, defaultValue: 0.00m),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                    table.CheckConstraint("CK_Assets_Status", "Status IN ('In Use', 'Available', 'Under Repair', 'Retired')");
                    table.CheckConstraint("CK_Assets_Warranty", "WarrantyUntil >= PurchaseDate");
                    table.ForeignKey(
                        name: "FK_Assets_AssetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssetCategories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TargetDisplay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentPersonnel",
                columns: table => new
                {
                    DepartmentPersonnelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsHOD = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentPersonnel", x => x.DepartmentPersonnelId);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypeTechnicianMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestTypeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentPersonnelId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypeTechnicianMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_RequestTypeTechnicianMappings_DepartmentPersonnel_DepartmentPersonnelId",
                        column: x => x.DepartmentPersonnelId,
                        principalTable: "DepartmentPersonnel",
                        principalColumn: "DepartmentPersonnelId");
                    table.ForeignKey(
                        name: "FK_RequestTypeTechnicianMappings_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "RequestTypeId");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Active"),
                    JoinedDate = table.Column<DateTime>(type: "DATE", nullable: false, defaultValueSql: "CAST(GETDATE() AS DATE)"),
                    LastLoginAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.CheckConstraint("CK_Users_Status", "Status IN ('Active', 'Inactive')");
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NotificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.CheckConstraint("CK_Notifications_Type", "NotificationType IN ('request', 'approval', 'asset', 'system')");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    RequestTypeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    RequesterUserId = table.Column<int>(type: "int", nullable: false),
                    AssigneeUserId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Medium"),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestId);
                    table.CheckConstraint("CK_ServiceRequests_Desc_Len", "LEN(TRIM(Description)) >= 20");
                    table.CheckConstraint("CK_ServiceRequests_Priority", "Priority IN ('Critical', 'High', 'Medium', 'Low')");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "RequestTypeId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceRequestStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ServiceRequestStatuses",
                        principalColumn: "StatusId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "light"),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NotifyRequestUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotifyApprovalAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotifySLAWarnings = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotifyAssetEvents = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NotifyEmailDigest = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.CheckConstraint("CK_UserSettings_Theme", "Theme IN ('light', 'dark')");
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestReplies",
                columns: table => new
                {
                    ReplyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusTransitionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestReplies", x => x.ReplyId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestReplies_ServiceRequestStatuses_StatusTransitionId",
                        column: x => x.StatusTransitionId,
                        principalTable: "ServiceRequestStatuses",
                        principalColumn: "StatusId");
                    table.ForeignKey(
                        name: "FK_ServiceRequestReplies_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestReplies_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestTimeline",
                columns: table => new
                {
                    TimelineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestTimeline", x => x.TimelineId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestTimeline_ServiceRequestStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ServiceRequestStatuses",
                        principalColumn: "StatusId");
                    table.ForeignKey(
                        name: "FK_ServiceRequestTimeline_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestTimeline_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestAttachments",
                columns: table => new
                {
                    AttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ReplyId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FileSizeKB = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestAttachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_ServiceRequestReplies_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "ServiceRequestReplies",
                        principalColumn: "ReplyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId");
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_PendingHOD",
                table: "Approvals",
                column: "DecidedByUserId",
                filter: "Status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_RequestId",
                table: "Approvals",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_CategoryName",
                table: "AssetCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedTo",
                table: "Assets",
                column: "AssignedToUserId",
                filter: "AssignedToUserId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CategoryId",
                table: "Assets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SerialNumber",
                table: "Assets",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Tag",
                table: "Assets",
                column: "AssetTag",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Search",
                table: "AuditLogs",
                column: "CreatedAt")
                .Annotation("SqlServer:Include", new[] { "ActorUserId", "Action", "TargetDisplay" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPersonnel_DepartmentId",
                table: "DepartmentPersonnel",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "UIX_DeptPersonnel_UserDept",
                table: "DepartmentPersonnel",
                columns: new[] { "UserId", "DepartmentId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeletedByUserId",
                table: "Departments",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentName",
                table: "Departments",
                column: "DepartmentName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UnreadInbox",
                table: "Notifications",
                column: "UserId",
                filter: "IsRead = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionKey",
                table: "Permissions",
                column: "PermissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypes_RequestTypeName",
                table: "RequestTypes",
                column: "RequestTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypes_ServiceTypeId",
                table: "RequestTypes",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypeTechnicianMappings_DepartmentPersonnelId",
                table: "RequestTypeTechnicianMappings",
                column: "DepartmentPersonnelId");

            migrationBuilder.CreateIndex(
                name: "UIX_ReqTypeTech_Mapping",
                table: "RequestTypeTechnicianMappings",
                columns: new[] { "RequestTypeId", "DepartmentPersonnelId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ReplyId",
                table: "ServiceRequestAttachments",
                column: "ReplyId",
                filter: "ReplyId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_RequestId",
                table: "ServiceRequestAttachments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestAttachments_UploadedByUserId",
                table: "ServiceRequestAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Replies_RequestId",
                table: "ServiceRequestReplies",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestReplies_AuthorUserId",
                table: "ServiceRequestReplies",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestReplies_StatusTransitionId",
                table: "ServiceRequestReplies",
                column: "StatusTransitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Assignee",
                table: "ServiceRequests",
                column: "AssigneeUserId",
                filter: "AssigneeUserId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CreatedByUserId",
                table: "ServiceRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_DeletedByUserId",
                table: "ServiceRequests",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Dept",
                table: "ServiceRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Requester",
                table: "ServiceRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestNumber",
                table: "ServiceRequests",
                column: "RequestNumber",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestTypeId",
                table: "ServiceRequests",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceTypeId",
                table: "ServiceRequests",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Status",
                table: "ServiceRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_UpdatedByUserId",
                table: "ServiceRequests",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestStatuses_StatusName",
                table: "ServiceRequestStatuses",
                column: "StatusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestTimeline_ChangedByUserId",
                table: "ServiceRequestTimeline",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestTimeline_StatusId",
                table: "ServiceRequestTimeline",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeline_RequestId",
                table: "ServiceRequestTimeline",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ServiceTypeCode",
                table: "ServiceTypes",
                column: "ServiceTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ServiceTypeName",
                table: "ServiceTypes",
                column: "ServiceTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "IsDeleted = 0")
                .Annotation("SqlServer:Include", new[] { "PasswordHash", "RoleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_ServiceRequests_RequestId",
                table: "Approvals",
                column: "RequestId",
                principalTable: "ServiceRequests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_Users_DecidedByUserId",
                table: "Approvals",
                column: "DecidedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Departments_DepartmentId",
                table: "Assets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Users_AssignedToUserId",
                table: "Assets",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentPersonnel_Departments_DepartmentId",
                table: "DepartmentPersonnel",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentPersonnel_Users_UserId",
                table: "DepartmentPersonnel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DeletedByUserId",
                table: "Departments",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DeletedByUserId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "Approvals");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RequestTypeTechnicianMappings");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ServiceRequestAttachments");

            migrationBuilder.DropTable(
                name: "ServiceRequestTimeline");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "AssetCategories");

            migrationBuilder.DropTable(
                name: "DepartmentPersonnel");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ServiceRequestReplies");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "RequestTypes");

            migrationBuilder.DropTable(
                name: "ServiceRequestStatuses");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
