using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceRequestManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.ServiceTypeId);
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
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypes",
                columns: table => new
                {
                    RequestTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    RequestTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypes", x => x.RequestTypeId);
                    table.ForeignKey(
                        name: "FK_RequestTypes_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WarrantyUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Assets_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Assets_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentPersonnel",
                columns: table => new
                {
                    DepartmentPersonnelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsHOD = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentPersonnel", x => x.DepartmentPersonnelId);
                    table.ForeignKey(
                        name: "FK_DepartmentPersonnel_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentPersonnel_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
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
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifyRequestUpdates = table.Column<bool>(type: "bit", nullable: false),
                    NotifyApprovalAlerts = table.Column<bool>(type: "bit", nullable: false),
                    NotifySLAWarnings = table.Column<bool>(type: "bit", nullable: false),
                    NotifyAssetEvents = table.Column<bool>(type: "bit", nullable: false),
                    NotifyEmailDigest = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
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
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "RequestTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceRequestStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ServiceRequestStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypeTechnicianMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestTypeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentPersonnelId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypeTechnicianMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_RequestTypeTechnicianMappings_DepartmentPersonnel_DepartmentPersonnelId",
                        column: x => x.DepartmentPersonnelId,
                        principalTable: "DepartmentPersonnel",
                        principalColumn: "DepartmentPersonnelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestTypeTechnicianMappings_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "RequestTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Approvals",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecidedByUserId = table.Column<int>(type: "int", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvals", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_Approvals_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approvals_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestTimeline",
                columns: table => new
                {
                    TimelineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestTimeline", x => x.TimelineId);
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
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestAttachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_ServiceRequestReplies_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "ServiceRequestReplies",
                        principalColumn: "ReplyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_DecidedByUserId",
                table: "Approvals",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_RequestId",
                table: "Approvals",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTag",
                table: "Assets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedToUserId",
                table: "Assets",
                column: "AssignedToUserId");

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
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPersonnel_DepartmentId",
                table: "DepartmentPersonnel",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPersonnel_UserId",
                table: "DepartmentPersonnel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

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
                name: "IX_RequestTypeTechnicianMappings_RequestTypeId",
                table: "RequestTypeTechnicianMappings",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestAttachments_ReplyId",
                table: "ServiceRequestAttachments",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestAttachments_RequestId",
                table: "ServiceRequestAttachments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestAttachments_UploadedByUserId",
                table: "ServiceRequestAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestReplies_AuthorUserId",
                table: "ServiceRequestReplies",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestReplies_RequestId",
                table: "ServiceRequestReplies",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestReplies_StatusTransitionId",
                table: "ServiceRequestReplies",
                column: "StatusTransitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_AssigneeUserId",
                table: "ServiceRequests",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_DepartmentId",
                table: "ServiceRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequesterUserId",
                table: "ServiceRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestNumber",
                table: "ServiceRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestTypeId",
                table: "ServiceRequests",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceTypeId",
                table: "ServiceRequests",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_StatusId",
                table: "ServiceRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestTimeline_ChangedByUserId",
                table: "ServiceRequestTimeline",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestTimeline_RequestId",
                table: "ServiceRequestTimeline",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ServiceTypeCode",
                table: "ServiceTypes",
                column: "ServiceTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "ServiceRequestAttachments");

            migrationBuilder.DropTable(
                name: "ServiceRequestTimeline");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "DepartmentPersonnel");

            migrationBuilder.DropTable(
                name: "ServiceRequestReplies");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "RequestTypes");

            migrationBuilder.DropTable(
                name: "ServiceRequestStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
