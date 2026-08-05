-- ============================================================
-- Database: ServiceRequestDB
-- Service Request Management System
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ServiceRequestDB')
BEGIN
    CREATE DATABASE ServiceRequestDB;
END
GO

USE ServiceRequestDB;
GO

-- ============================================================
-- TABLE: Roles
-- ============================================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================
-- TABLE: Permissions
-- ============================================================
CREATE TABLE Permissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    PermissionKey NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================
-- TABLE: RolePermissions
-- ============================================================
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE: Departments
-- ============================================================
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    DepartmentCode NVARCHAR(10) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedByUserId INT NULL,
    FOREIGN KEY (DeletedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- TABLE: Users
-- ============================================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    RoleId INT NOT NULL,
    DepartmentId INT NULL,
    Phone NVARCHAR(20) NULL,
    Status VARCHAR(15) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive')),
    JoinedDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE SET NULL
);
GO

-- Indexes for Users
CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = 0 INCLUDE (PasswordHash, RoleId, Status);
CREATE INDEX IX_Users_EmployeeId ON Users(EmployeeId);
GO

-- ============================================================
-- TABLE: UserSettings
-- ============================================================
CREATE TABLE UserSettings (
    UserId INT PRIMARY KEY,
    Theme VARCHAR(10) NOT NULL DEFAULT 'light' CHECK (Theme IN ('light', 'dark')),
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    NotifyRequestUpdates BIT NOT NULL DEFAULT 1,
    NotifyApprovalAlerts BIT NOT NULL DEFAULT 1,
    NotifySLAWarnings BIT NOT NULL DEFAULT 1,
    NotifyAssetEvents BIT NOT NULL DEFAULT 0,
    NotifyEmailDigest BIT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE: DepartmentPersonnel
-- ============================================================
CREATE TABLE DepartmentPersonnel (
    DepartmentPersonnelId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DepartmentId INT NOT NULL,
    IsHOD BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE NO ACTION
);
GO

-- Unique index for DepartmentPersonnel
CREATE UNIQUE INDEX UIX_DeptPersonnel_UserDept ON DepartmentPersonnel(UserId, DepartmentId) WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE: ServiceTypes
-- ============================================================
CREATE TABLE ServiceTypes (
    ServiceTypeId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceTypeName NVARCHAR(50) NOT NULL UNIQUE,
    ServiceTypeCode NVARCHAR(10) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

-- ============================================================
-- TABLE: RequestTypes
-- ============================================================
CREATE TABLE RequestTypes (
    RequestTypeId INT IDENTITY(1,1) PRIMARY KEY,
    RequestTypeName NVARCHAR(100) NOT NULL UNIQUE,
    ServiceTypeId INT NOT NULL,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(ServiceTypeId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_RequestTypes_ServiceTypeId ON RequestTypes(ServiceTypeId);
GO

-- ============================================================
-- TABLE: ServiceRequestStatuses
-- ============================================================
CREATE TABLE ServiceRequestStatuses (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE,
    ColorCode VARCHAR(100) NULL,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================
-- TABLE: AssetCategories
-- ============================================================
CREATE TABLE AssetCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================
-- TABLE: Assets
-- ============================================================
CREATE TABLE Assets (
    AssetId INT IDENTITY(1,1) PRIMARY KEY,
    AssetTag VARCHAR(30) NOT NULL UNIQUE,
    AssetName NVARCHAR(100) NOT NULL,
    CategoryId INT NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL UNIQUE,
    AssignedToUserId INT NULL,
    DepartmentId INT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Available' CHECK (Status IN ('In Use', 'Available', 'Under Repair', 'Retired')),
    PurchaseDate DATE NOT NULL,
    WarrantyUntil DATE NOT NULL CHECK (WarrantyUntil >= PurchaseDate),
    BookValue DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CategoryId) REFERENCES AssetCategories(CategoryId) ON DELETE NO ACTION,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId) ON DELETE SET NULL,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Assets_AssignedTo ON Assets(AssignedToUserId) WHERE AssignedToUserId IS NOT NULL;
CREATE UNIQUE INDEX IX_Assets_Tag ON Assets(AssetTag) WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE: RequestTypeTechnicianMappings
-- ============================================================
CREATE TABLE RequestTypeTechnicianMappings (
    MappingId INT IDENTITY(1,1) PRIMARY KEY,
    RequestTypeId INT NOT NULL,
    DepartmentPersonnelId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (RequestTypeId) REFERENCES RequestTypes(RequestTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentPersonnelId) REFERENCES DepartmentPersonnel(DepartmentPersonnelId) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX UIX_ReqTypeTech_Mapping ON RequestTypeTechnicianMappings(RequestTypeId, DepartmentPersonnelId) WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE: ServiceRequests
-- ============================================================
CREATE TABLE ServiceRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    RequestNumber VARCHAR(20) NOT NULL UNIQUE,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL CHECK (LEN(TRIM(Description)) >= 20),
    ServiceTypeId INT NOT NULL,
    RequestTypeId INT NOT NULL,
    DepartmentId INT NOT NULL,
    RequesterUserId INT NOT NULL,
    AssigneeUserId INT NULL,
    StatusId INT NOT NULL,
    Priority VARCHAR(15) NOT NULL DEFAULT 'Medium' CHECK (Priority IN ('Critical', 'High', 'Medium', 'Low')),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId INT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedByUserId INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedByUserId INT NULL,
    FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(ServiceTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (RequestTypeId) REFERENCES RequestTypes(RequestTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE NO ACTION,
    FOREIGN KEY (RequesterUserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (AssigneeUserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (StatusId) REFERENCES ServiceRequestStatuses(StatusId) ON DELETE NO ACTION,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (DeletedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX IX_ServiceRequests_RequestNumber ON ServiceRequests(RequestNumber) WHERE IsDeleted = 0;
CREATE INDEX IX_ServiceRequests_Requester ON ServiceRequests(RequesterUserId);
CREATE INDEX IX_ServiceRequests_Assignee ON ServiceRequests(AssigneeUserId) WHERE AssigneeUserId IS NOT NULL;
CREATE INDEX IX_ServiceRequests_Status ON ServiceRequests(StatusId);
CREATE INDEX IX_ServiceRequests_Dept ON ServiceRequests(DepartmentId);
GO

-- ============================================================
-- TABLE: ServiceRequestReplies
-- ============================================================
CREATE TABLE ServiceRequestReplies (
    ReplyId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    AuthorUserId INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    StatusTransitionId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (AuthorUserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (StatusTransitionId) REFERENCES ServiceRequestStatuses(StatusId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Replies_RequestId ON ServiceRequestReplies(RequestId);
GO

-- ============================================================
-- TABLE: ServiceRequestTimeline
-- ============================================================
CREATE TABLE ServiceRequestTimeline (
    TimelineId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    StatusId INT NOT NULL,
    ChangedByUserId INT NOT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Note NVARCHAR(500) NOT NULL,
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (StatusId) REFERENCES ServiceRequestStatuses(StatusId) ON DELETE NO ACTION,
    FOREIGN KEY (ChangedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Timeline_RequestId ON ServiceRequestTimeline(RequestId);
GO

-- ============================================================
-- TABLE: ServiceRequestAttachments
-- ============================================================
CREATE TABLE ServiceRequestAttachments (
    AttachmentId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    ReplyId INT NULL,
    FileName NVARCHAR(256) NOT NULL,
    FileSizeKB INT NOT NULL,
    FileUrl NVARCHAR(2048) NOT NULL,
    UploadedByUserId INT NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ReplyId) REFERENCES ServiceRequestReplies(ReplyId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Attachments_RequestId ON ServiceRequestAttachments(RequestId);
CREATE INDEX IX_Attachments_ReplyId ON ServiceRequestAttachments(ReplyId) WHERE ReplyId IS NOT NULL;
GO

-- ============================================================
-- TABLE: Approvals
-- ============================================================
CREATE TABLE Approvals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL UNIQUE,
    Status VARCHAR(15) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    DecidedByUserId INT NULL,
    DecidedAt DATETIME2 NULL,
    Remarks NVARCHAR(1000) NULL,
    SubmittedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (DecidedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Approvals_RequestId ON Approvals(RequestId);
CREATE INDEX IX_Approvals_PendingHOD ON Approvals(DecidedByUserId) WHERE Status = 'Pending';
GO

-- ============================================================
-- TABLE: Notifications
-- ============================================================
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    NotificationType VARCHAR(20) NOT NULL CHECK (NotificationType IN ('request', 'approval', 'asset', 'system')),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Notifications_UnreadInbox ON Notifications(UserId) WHERE IsRead = 0;
GO

-- ============================================================
-- TABLE: AuditLogs
-- ============================================================
CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ActorUserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    TargetType VARCHAR(50) NOT NULL,
    TargetId VARCHAR(50) NULL,
    TargetDisplay NVARCHAR(100) NULL,
    Detail NVARCHAR(MAX) NULL,
    IpAddress VARCHAR(45) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (ActorUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_AuditLogs_Search ON AuditLogs(CreatedAt) INCLUDE (ActorUserId, Action, TargetDisplay);
GO