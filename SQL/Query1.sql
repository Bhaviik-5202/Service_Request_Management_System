-- ============================================================
-- Database: ServiceRequestSystemDB
-- Service Request Management System - Database Schema Script
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ServiceRequestSystemDB')
BEGIN
    CREATE DATABASE ServiceRequestSystemDB;
END
GO

USE ServiceRequestSystemDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Drop tables if they exist in reverse dependency order
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.Assets', 'U') IS NOT NULL DROP TABLE dbo.Assets;
IF OBJECT_ID('dbo.Approvals', 'U') IS NOT NULL DROP TABLE dbo.Approvals;
IF OBJECT_ID('dbo.ServiceRequestAttachments', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequestAttachments;
IF OBJECT_ID('dbo.ServiceRequestTimeline', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequestTimeline;
IF OBJECT_ID('dbo.ServiceRequestReplies', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequestReplies;
IF OBJECT_ID('dbo.ServiceRequests', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequests;
IF OBJECT_ID('dbo.ServiceRequestStatuses', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequestStatuses;
IF OBJECT_ID('dbo.RequestTypeTechnicianMappings', 'U') IS NOT NULL DROP TABLE dbo.RequestTypeTechnicianMappings;
IF OBJECT_ID('dbo.RequestTypes', 'U') IS NOT NULL DROP TABLE dbo.RequestTypes;
IF OBJECT_ID('dbo.ServiceTypes', 'U') IS NOT NULL DROP TABLE dbo.ServiceTypes;
IF OBJECT_ID('dbo.DepartmentPersonnel', 'U') IS NOT NULL DROP TABLE dbo.DepartmentPersonnel;
IF OBJECT_ID('dbo.UserSettings', 'U') IS NOT NULL DROP TABLE dbo.UserSettings;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

-- ============================================================
-- 1. TABLE: Departments
-- ============================================================
CREATE TABLE dbo.Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    DepartmentCode VARCHAR(10) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL
);
GO

-- ============================================================
-- 2. TABLE: Users
-- ============================================================
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'Requestor' CHECK (Role IN ('Admin', 'HOD', 'Technician', 'Requestor')),
    DepartmentId INT NULL,
    Phone NVARCHAR(20) NULL,
    Status VARCHAR(15) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive')),
    JoinedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId) ON DELETE NO ACTION
);
GO

-- Indexes for Users
CREATE INDEX IX_Users_Email ON dbo.Users(Email) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_EmployeeId ON dbo.Users(EmployeeId);
GO

-- ============================================================
-- 3. TABLE: UserSettings
-- ============================================================
CREATE TABLE dbo.UserSettings (
    UserId INT PRIMARY KEY,
    Theme NVARCHAR(10) NOT NULL DEFAULT 'light',
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    NotifyRequestUpdates BIT NOT NULL DEFAULT 1,
    NotifyApprovalAlerts BIT NOT NULL DEFAULT 1,
    NotifySLAWarnings BIT NOT NULL DEFAULT 1,
    NotifyAssetEvents BIT NOT NULL DEFAULT 0,
    NotifyEmailDigest BIT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);
GO

-- ============================================================
-- 4. TABLE: DepartmentPersonnel
-- ============================================================
CREATE TABLE dbo.DepartmentPersonnel (
    DepartmentPersonnelId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DepartmentId INT NOT NULL,
    IsHOD BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 5. TABLE: ServiceTypes
-- ============================================================
CREATE TABLE dbo.ServiceTypes (
    ServiceTypeId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceTypeName NVARCHAR(50) NOT NULL UNIQUE,
    ServiceTypeCode VARCHAR(10) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL
);
GO

-- ============================================================
-- 6. TABLE: RequestTypes
-- ============================================================
CREATE TABLE dbo.RequestTypes (
    RequestTypeId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceTypeId INT NOT NULL,
    RequestTypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    RequiresApproval BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (ServiceTypeId) REFERENCES dbo.ServiceTypes(ServiceTypeId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 7. TABLE: RequestTypeTechnicianMappings
-- ============================================================
CREATE TABLE dbo.RequestTypeTechnicianMappings (
    MappingId INT IDENTITY(1,1) PRIMARY KEY,
    RequestTypeId INT NOT NULL,
    DepartmentPersonnelId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (RequestTypeId) REFERENCES dbo.RequestTypes(RequestTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentPersonnelId) REFERENCES dbo.DepartmentPersonnel(DepartmentPersonnelId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 8. TABLE: ServiceRequestStatuses
-- ============================================================
CREATE TABLE dbo.ServiceRequestStatuses (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE,
    ColorCode NVARCHAR(100) NULL,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================
-- 9. TABLE: ServiceRequests
-- ============================================================
CREATE TABLE dbo.ServiceRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    RequestNumber VARCHAR(20) NOT NULL UNIQUE,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ServiceTypeId INT NOT NULL,
    RequestTypeId INT NOT NULL,
    DepartmentId INT NOT NULL,
    RequesterUserId INT NOT NULL,
    AssigneeUserId INT NULL,
    StatusId INT NOT NULL,
    Priority VARCHAR(10) NOT NULL DEFAULT 'Medium' CHECK (Priority IN ('Critical', 'High', 'Medium', 'Low')),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (ServiceTypeId) REFERENCES dbo.ServiceTypes(ServiceTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (RequestTypeId) REFERENCES dbo.RequestTypes(RequestTypeId) ON DELETE NO ACTION,
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId) ON DELETE NO ACTION,
    FOREIGN KEY (RequesterUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (AssigneeUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (StatusId) REFERENCES dbo.ServiceRequestStatuses(StatusId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_ServiceRequests_RequestNumber ON dbo.ServiceRequests(RequestNumber) WHERE IsDeleted = 0;
CREATE INDEX IX_ServiceRequests_Requester ON dbo.ServiceRequests(RequesterUserId);
CREATE INDEX IX_ServiceRequests_Assignee ON dbo.ServiceRequests(AssigneeUserId);
GO

-- ============================================================
-- 10. TABLE: ServiceRequestReplies
-- ============================================================
CREATE TABLE dbo.ServiceRequestReplies (
    ReplyId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    AuthorUserId INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    StatusTransitionId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES dbo.ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (AuthorUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (StatusTransitionId) REFERENCES dbo.ServiceRequestStatuses(StatusId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 11. TABLE: ServiceRequestTimeline
-- ============================================================
CREATE TABLE dbo.ServiceRequestTimeline (
    TimelineId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    StatusName NVARCHAR(50) NOT NULL,
    ChangedByUserId INT NOT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Note NVARCHAR(500) NOT NULL,
    FOREIGN KEY (RequestId) REFERENCES dbo.ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 12. TABLE: ServiceRequestAttachments
-- ============================================================
CREATE TABLE dbo.ServiceRequestAttachments (
    AttachmentId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    ReplyId INT NULL,
    FileName NVARCHAR(256) NOT NULL,
    FileSizeKB INT NOT NULL,
    FileUrl NVARCHAR(2048) NOT NULL,
    UploadedByUserId INT NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES dbo.ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ReplyId) REFERENCES dbo.ServiceRequestReplies(ReplyId) ON DELETE NO ACTION,
    FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 13. TABLE: Approvals
-- ============================================================
CREATE TABLE dbo.Approvals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL UNIQUE,
    Status VARCHAR(15) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    DecidedByUserId INT NULL,
    DecidedAt DATETIME2 NULL,
    Remarks NVARCHAR(1000) NULL,
    SubmittedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RequestId) REFERENCES dbo.ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (DecidedByUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION
);
GO

-- ============================================================
-- 14. TABLE: Assets
-- ============================================================
CREATE TABLE dbo.Assets (
    AssetId INT IDENTITY(1,1) PRIMARY KEY,
    AssetTag VARCHAR(30) NOT NULL UNIQUE,
    AssetName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL UNIQUE,
    AssignedToUserId INT NULL,
    DepartmentId INT NULL,
    Status VARCHAR(15) NOT NULL DEFAULT 'Available' CHECK (Status IN ('InUse', 'Available', 'UnderRepair', 'Retired')),
    PurchaseDate DATE NOT NULL,
    WarrantyUntil DATE NOT NULL,
    BookValue DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES dbo.Users(UserId) ON DELETE SET NULL,
    FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Assets_AssetTag ON dbo.Assets(AssetTag) WHERE IsDeleted = 0;
CREATE INDEX IX_Assets_SerialNumber ON dbo.Assets(SerialNumber) WHERE IsDeleted = 0;
GO

-- ============================================================
-- 15. TABLE: Notifications
-- ============================================================
CREATE TABLE dbo.Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    NotificationType VARCHAR(15) NOT NULL DEFAULT 'system' CHECK (NotificationType IN ('request', 'approval', 'asset', 'system')),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);
GO

-- ============================================================
-- 16. TABLE: AuditLogs
-- ============================================================
CREATE TABLE dbo.AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ActorUserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    TargetType NVARCHAR(50) NOT NULL,
    TargetId NVARCHAR(50) NULL,
    TargetDisplay NVARCHAR(100) NULL,
    Detail NVARCHAR(MAX) NULL,
    IpAddress VARCHAR(45) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION
);
GO