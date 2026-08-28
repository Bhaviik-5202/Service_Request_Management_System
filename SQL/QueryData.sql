-- ============================================================
-- Database: ServiceRequestSystemDB
-- Service Request Management System - Master Seed Data Script
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ServiceRequestSystemDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Disable FK constraints for clean data deletion
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT ALL";
GO

-- Clean up existing data in reverse dependency order
DELETE FROM dbo.AuditLogs;
DELETE FROM dbo.Notifications;
DELETE FROM dbo.Assets;
DELETE FROM dbo.Approvals;
DELETE FROM dbo.ServiceRequestAttachments;
DELETE FROM dbo.ServiceRequestTimeline;
DELETE FROM dbo.ServiceRequestReplies;
DELETE FROM dbo.ServiceRequests;
DELETE FROM dbo.ServiceRequestStatuses;
DELETE FROM dbo.RequestTypeTechnicianMappings;
DELETE FROM dbo.RequestTypes;
DELETE FROM dbo.ServiceTypes;
DELETE FROM dbo.DepartmentPersonnel;
DELETE FROM dbo.UserSettings;
DELETE FROM dbo.Users;
DELETE FROM dbo.Departments;
GO

-- Re-enable FK constraints
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL";
GO

-- ============================================================
-- 1. Departments
-- ============================================================
SET IDENTITY_INSERT dbo.Departments ON;
INSERT INTO dbo.Departments (DepartmentId, DepartmentName, DepartmentCode, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 'Information Technology', 'IT', 'IT Infrastructure, Hardware and Software Support', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 'Maintenance', 'MAINT', 'Facility, HVAC, Electrical and Equipment Maintenance', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 'Housekeeping', 'HK', 'Cleaning, Janitorial and Sanitation Services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (4, 'Human Resources', 'HR', 'Employee Relations and Onboarding Support', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (5, 'Finance & Accounting', 'FIN', 'Financial Operations and Asset Book Value Audits', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (6, 'Security & Access Control', 'SEC', 'Physical and Digital Access Management', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.Departments OFF;
GO

-- ============================================================
-- 2. Users
-- ============================================================
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserId, EmployeeId, FullName, Email, Role, DepartmentId, Phone, Status, JoinedDate, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 'EMP-0001', 'System Admin', 'admin@company.com', 'Admin', 1, '+91 9876543210', 'Active', '2024-01-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 'EMP-0002', 'Divya Nair', 'hod@company.com', 'HOD', 1, '+91 9876543211', 'Active', '2024-01-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 'EMP-0003', 'Ronak Verma', 'tech@company.com', 'Technician', 1, '+91 9876543212', 'Active', '2024-02-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (4, 'EMP-0004', 'Anita Desai', 'anita.desai@company.com', 'Technician', 1, '+91 9876543213', 'Active', '2024-02-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (5, 'EMP-0005', 'Suresh Kumar', 'suresh.kumar@company.com', 'Technician', 2, '+91 9876543214', 'Active', '2024-03-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (6, 'EMP-0006', 'Meena Joshi', 'meena.joshi@company.com', 'HOD', 2, '+91 9876543215', 'Active', '2024-03-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (7, 'EMP-0007', 'Rahul Sharma', 'requestor@company.com', 'Requestor', 4, '+91 9876543216', 'Active', '2024-04-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (8, 'EMP-0008', 'Neha Gupta', 'neha.gupta@company.com', 'Requestor', 5, '+91 9876543217', 'Active', '2024-04-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- ============================================================
-- 3. UserSettings
-- ============================================================
INSERT INTO dbo.UserSettings (UserId, Theme, TwoFactorEnabled, NotifyRequestUpdates, NotifyApprovalAlerts, NotifySLAWarnings, NotifyAssetEvents, NotifyEmailDigest, UpdatedAt)
VALUES 
    (1, 'light', 0, 1, 1, 1, 1, 1, SYSUTCDATETIME()),
    (2, 'light', 0, 1, 1, 1, 0, 0, SYSUTCDATETIME()),
    (3, 'dark',  0, 1, 1, 1, 1, 0, SYSUTCDATETIME()),
    (4, 'light', 0, 1, 1, 1, 0, 0, SYSUTCDATETIME()),
    (5, 'light', 0, 1, 1, 1, 0, 0, SYSUTCDATETIME()),
    (6, 'light', 0, 1, 1, 1, 0, 0, SYSUTCDATETIME()),
    (7, 'light', 0, 1, 0, 0, 0, 0, SYSUTCDATETIME()),
    (8, 'light', 0, 1, 0, 0, 0, 0, SYSUTCDATETIME());
GO

-- ============================================================
-- 4. DepartmentPersonnel
-- ============================================================
SET IDENTITY_INSERT dbo.DepartmentPersonnel ON;
INSERT INTO dbo.DepartmentPersonnel (DepartmentPersonnelId, UserId, DepartmentId, IsHOD, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 2, 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Divya (IT HOD)
    (2, 3, 1, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Ronak (IT Tech)
    (3, 4, 1, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Anita (IT Tech)
    (4, 6, 2, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Meena (Maint HOD)
    (5, 5, 2, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Suresh (Maint Tech)
SET IDENTITY_INSERT dbo.DepartmentPersonnel OFF;
GO

-- ============================================================
-- 5. ServiceTypes
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceTypes ON;
INSERT INTO dbo.ServiceTypes (ServiceTypeId, ServiceTypeName, ServiceTypeCode, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 'Technical Services', 'TECH', 'IT Equipment, Network, Software and Computer Systems', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 'Facility Services', 'FACILITY', 'Air Conditioning, Electrical, Plumbing and Janitorial', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 'Administrative Services', 'ADMIN', 'Access Management, Security, Furniture and Onboarding', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.ServiceTypes OFF;
GO

-- ============================================================
-- 6. RequestTypes
-- ============================================================
SET IDENTITY_INSERT dbo.RequestTypes ON;
INSERT INTO dbo.RequestTypes (RequestTypeId, ServiceTypeId, RequestTypeName, Description, RequiresApproval, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 1, 'Computer Issue', 'PC or Laptop hardware crash, blue screen, boot failure', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 1, 'Software Request', 'New software license, installation, Figma or IDE setup', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 1, 'Hardware Request', 'Secondary monitor, docking station or RAM upgrade', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (4, 1, 'Network Issue', 'Wi-Fi connection drop, slow VPN or IP conflict', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (5, 1, 'Email & Account Setup', 'Email password reset, distribution list access', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (6, 1, 'Printer Issue', 'Printer offline, paper jam or toner replacement', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (7, 2, 'AC Repair & Cooling', 'Office AC temperature adjustment or leak repair', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (8, 2, 'Plumbing & Water', 'Restroom water leak or tap repair', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (9, 2, 'Cleaning & Janitorial', 'Desk deep clean or floor spill cleanup', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (10, 3, 'Access Card Request', 'New keycard badge or server room access elevation', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.RequestTypes OFF;
GO

-- ============================================================
-- 7. RequestTypeTechnicianMappings
-- ============================================================
SET IDENTITY_INSERT dbo.RequestTypeTechnicianMappings ON;
INSERT INTO dbo.RequestTypeTechnicianMappings (MappingId, RequestTypeId, DepartmentPersonnelId, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 1, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Computer Issue -> Ronak
    (2, 2, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Software Request -> Anita
    (3, 3, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Hardware Request -> Anita
    (4, 4, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Network Issue -> Ronak
    (5, 7, 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- AC Repair -> Suresh
SET IDENTITY_INSERT dbo.RequestTypeTechnicianMappings OFF;
GO

-- ============================================================
-- 8. ServiceRequestStatuses
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceRequestStatuses ON;
INSERT INTO dbo.ServiceRequestStatuses (StatusId, StatusName, ColorCode, Description, IsActive, CreatedAt)
VALUES 
    (1, 'Open', 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300', 'Newly raised ticket awaiting technician assignment or action', 1, SYSUTCDATETIME()),
    (2, 'In Progress', 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-300', 'Technician actively investigating or resolving the ticket', 1, SYSUTCDATETIME()),
    (3, 'Pending Approval', 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300', 'Awaiting HOD sign-off before work can commence', 1, SYSUTCDATETIME()),
    (4, 'Resolved', 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900 dark:text-emerald-300', 'Issue fixed; awaiting requester confirmation', 1, SYSUTCDATETIME()),
    (5, 'Closed', 'bg-slate-100 text-slate-800 dark:bg-slate-800 dark:text-slate-300', 'Ticket verified and marked closed', 1, SYSUTCDATETIME()),
    (6, 'Rejected', 'bg-rose-100 text-rose-800 dark:bg-rose-900 dark:text-rose-300', 'Ticket rejected by HOD with remarks', 1, SYSUTCDATETIME()),
    (7, 'Cancelled', 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300', 'Ticket cancelled by requester', 1, SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.ServiceRequestStatuses OFF;
GO

-- ============================================================
-- 9. ServiceRequests
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceRequests ON;
INSERT INTO dbo.ServiceRequests (RequestId, RequestNumber, Title, Description, ServiceTypeId, RequestTypeId, DepartmentId, RequesterUserId, AssigneeUserId, StatusId, Priority, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 'SR-2026-1001', 'Laptop screen blue screen crash error', 'My Dell Latitude laptop crashed twice this morning displaying driver error BSOD when running dual external displays.', 1, 1, 1, 7, 3, 2, 'High', DATEADD(DAY, -3, SYSUTCDATETIME()), SYSUTCDATETIME(), 0),
    (2, 'SR-2026-1002', 'Figma Pro Software License Request', 'Need a Figma Pro editor seat license for Q3 UI design deliverables in the product squad.', 1, 2, 1, 8, NULL, 3, 'Medium', DATEADD(DAY, -2, SYSUTCDATETIME()), SYSUTCDATETIME(), 0),
    (3, 'SR-2026-1003', 'Conference Room AC Temperature Cooling Leak', 'The AC unit in Conference Room 4B is blowing warm air and dripping water onto the floor whiteboard.', 2, 7, 2, 7, 5, 2, 'Critical', DATEADD(DAY, -1, SYSUTCDATETIME()), SYSUTCDATETIME(), 0),
    (4, 'SR-2026-1004', 'Server Room Badge Access Elevation', 'Requesting elevated NFC badge access to Server Room B for scheduled network switch maintenance.', 3, 10, 6, 8, 3, 4, 'High', DATEADD(DAY, -5, SYSUTCDATETIME()), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.ServiceRequests OFF;
GO

-- ============================================================
-- 10. ServiceRequestReplies
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceRequestReplies ON;
INSERT INTO dbo.ServiceRequestReplies (ReplyId, RequestId, AuthorUserId, Message, StatusTransitionId, CreatedAt)
VALUES 
    (1, 1, 3, 'Hello Rahul, I have received your ticket. I will run hardware diagnostics on the graphics driver today.', 2, DATEADD(DAY, -2, SYSUTCDATETIME())),
    (2, 1, 7, 'Thanks Ronak! Let me know if you need physical access to my desk.', NULL, DATEADD(DAY, -1, SYSUTCDATETIME())),
    (3, 3, 5, 'Inspected the AC unit. Thermostat sensor replaced and drainage tray cleared.', 4, DATEADD(HOUR, -2, SYSUTCDATETIME()));
SET IDENTITY_INSERT dbo.ServiceRequestReplies OFF;
GO

-- ============================================================
-- 11. ServiceRequestTimeline
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceRequestTimeline ON;
INSERT INTO dbo.ServiceRequestTimeline (TimelineId, RequestId, StatusName, ChangedByUserId, ChangedAt, Note)
VALUES 
    (1, 1, 'Open', 7, DATEADD(DAY, -3, SYSUTCDATETIME()), 'Ticket raised by employee'),
    (2, 1, 'In Progress', 3, DATEADD(DAY, -2, SYSUTCDATETIME()), 'Assigned to technician Ronak Verma'),
    (3, 2, 'Pending Approval', 8, DATEADD(DAY, -2, SYSUTCDATETIME()), 'Software request submitted; sent to HOD Divya Nair for sign-off'),
    (4, 3, 'In Progress', 5, DATEADD(DAY, -1, SYSUTCDATETIME()), 'Technician Suresh Kumar assigned to facility ticket'),
    (5, 3, 'Resolved', 5, DATEADD(HOUR, -2, SYSUTCDATETIME()), 'AC cooling leak fixed and tested');
SET IDENTITY_INSERT dbo.ServiceRequestTimeline OFF;
GO

-- ============================================================
-- 12. ServiceRequestAttachments
-- ============================================================
SET IDENTITY_INSERT dbo.ServiceRequestAttachments ON;
INSERT INTO dbo.ServiceRequestAttachments (AttachmentId, RequestId, ReplyId, FileName, FileSizeKB, FileUrl, UploadedByUserId, UploadedAt)
VALUES 
    (1, 1, NULL, 'bsod_error_screenshot.png', 420, '/uploads/bsod_error_screenshot.png', 7, DATEADD(DAY, -3, SYSUTCDATETIME())),
    (2, 3, 3, 'ac_repair_completion_photo.jpg', 850, '/uploads/ac_repair_completion_photo.jpg', 5, DATEADD(HOUR, -2, SYSUTCDATETIME()));
SET IDENTITY_INSERT dbo.ServiceRequestAttachments OFF;
GO

-- ============================================================
-- 13. Approvals
-- ============================================================
SET IDENTITY_INSERT dbo.Approvals ON;
INSERT INTO dbo.Approvals (ApprovalId, RequestId, Status, DecidedByUserId, DecidedAt, Remarks, SubmittedAt)
VALUES 
    (1, 2, 'Pending', NULL, NULL, NULL, DATEADD(DAY, -2, SYSUTCDATETIME())),
    (2, 4, 'Approved', 2, DATEADD(DAY, -4, SYSUTCDATETIME()), 'Approved for scheduled server room maintenance window', DATEADD(DAY, -5, SYSUTCDATETIME()));
SET IDENTITY_INSERT dbo.Approvals OFF;
GO

-- ============================================================
-- 14. Assets
-- ============================================================
SET IDENTITY_INSERT dbo.Assets ON;
INSERT INTO dbo.Assets (AssetId, AssetTag, AssetName, Category, SerialNumber, AssignedToUserId, DepartmentId, Status, PurchaseDate, WarrantyUntil, BookValue, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (1, 'AST-IT-0142', 'Dell Latitude 7440 Laptop', 'Laptop', 'DL7440-98213', 7, 1, 'InUse', '2023-05-10', '2026-05-10', 85000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 'AST-IT-0143', 'Apple MacBook Pro 16"', 'Laptop', 'MBP16-55421', 8, 1, 'InUse', '2023-08-15', '2026-08-15', 195000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 'AST-IT-0201', 'Dell UltraSharp 27" Monitor', 'Monitor', 'MON27-11029', 7, 1, 'InUse', '2023-06-01', '2026-06-01', 32000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (4, 'AST-FAC-005', 'Daikin 2-Ton Inverter AC Unit', 'HVAC', 'AC-DK-44102', NULL, 2, 'Available', '2022-11-20', '2025-11-20', 48000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (5, 'AST-SEC-012', 'Cisco Catalyst 48-Port Switch', 'Network', 'SW-CS-99012', NULL, 6, 'InUse', '2023-02-10', '2028-02-10', 125000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.Assets OFF;
GO

-- ============================================================
-- 15. Notifications
-- ============================================================
SET IDENTITY_INSERT dbo.Notifications ON;
INSERT INTO dbo.Notifications (NotificationId, UserId, Title, Message, IsRead, NotificationType, CreatedAt)
VALUES 
    (1, 7, 'Ticket Status Updated', 'Your ticket SR-2026-1001 status changed to In Progress.', 0, 'request', DATEADD(DAY, -2, SYSUTCDATETIME())),
    (2, 2, 'Pending Approval Request', 'Ticket SR-2026-1002 requires your HOD approval.', 0, 'approval', DATEADD(DAY, -2, SYSUTCDATETIME())),
    (3, 3, 'New Ticket Assigned', 'You have been assigned to SR-2026-1001.', 1, 'request', DATEADD(DAY, -2, SYSUTCDATETIME())),
    (4, 8, 'Approval Approved', 'Your access card request SR-2026-1004 has been approved by Divya Nair.', 1, 'approval', DATEADD(DAY, -4, SYSUTCDATETIME()));
SET IDENTITY_INSERT dbo.Notifications OFF;
GO

-- ============================================================
-- 16. AuditLogs
-- ============================================================
SET IDENTITY_INSERT dbo.AuditLogs ON;
INSERT INTO dbo.AuditLogs (AuditLogId, ActorUserId, Action, TargetType, TargetId, TargetDisplay, Detail, IpAddress, CreatedAt)
VALUES 
    (1, 1, 'SYSTEM_INIT', 'System', '0', 'Database Initialization', 'Master seed database populated successfully.', '127.0.0.1', SYSUTCDATETIME()),
    (2, 7, 'CREATE_REQUEST', 'ServiceRequest', '1', 'SR-2026-1001', 'Raised ticket: Laptop screen blue screen crash error', '192.168.1.45', DATEADD(DAY, -3, SYSUTCDATETIME())),
    (3, 2, 'APPROVE_REQUEST', 'Approval', '4', 'SR-2026-1004', 'HOD Divya Nair approved access card request.', '192.168.1.12', DATEADD(DAY, -4, SYSUTCDATETIME()));
SET IDENTITY_INSERT dbo.AuditLogs OFF;
GO