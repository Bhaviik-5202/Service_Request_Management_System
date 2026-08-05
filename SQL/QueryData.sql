-- ============================================================
-- INSERT DUMMY DATA
-- ============================================================

-- ============================================================
-- 1. Roles
-- ============================================================
INSERT INTO Roles (RoleName, Description, CreatedAt, UpdatedAt)
VALUES 
    ('Admin', 'System Administrator with full access', SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('HOD', 'Head of Department - Can approve requests', SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Technician', 'Technical staff who resolves tickets', SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Requestor', 'Regular employee who creates requests', SYSUTCDATETIME(), SYSUTCDATETIME());
GO

-- ============================================================
-- 2. Permissions
-- ============================================================
INSERT INTO Permissions (PermissionKey, Description, CreatedAt)
VALUES 
    ('users.create', 'Can create new users', SYSUTCDATETIME()),
    ('users.edit', 'Can edit user details', SYSUTCDATETIME()),
    ('users.delete', 'Can delete users', SYSUTCDATETIME()),
    ('users.view', 'Can view user details', SYSUTCDATETIME()),
    ('requests.create', 'Can create service requests', SYSUTCDATETIME()),
    ('requests.edit', 'Can edit service requests', SYSUTCDATETIME()),
    ('requests.delete', 'Can delete service requests', SYSUTCDATETIME()),
    ('requests.view', 'Can view service requests', SYSUTCDATETIME()),
    ('requests.assign', 'Can assign technicians to requests', SYSUTCDATETIME()),
    ('approvals.decide', 'Can approve or reject requests', SYSUTCDATETIME()),
    ('approvals.view', 'Can view approvals', SYSUTCDATETIME()),
    ('assets.create', 'Can create assets', SYSUTCDATETIME()),
    ('assets.edit', 'Can edit assets', SYSUTCDATETIME()),
    ('assets.delete', 'Can delete assets', SYSUTCDATETIME()),
    ('assets.view', 'Can view assets', SYSUTCDATETIME()),
    ('assets.assign', 'Can assign assets to users', SYSUTCDATETIME()),
    ('departments.create', 'Can create departments', SYSUTCDATETIME()),
    ('departments.edit', 'Can edit departments', SYSUTCDATETIME()),
    ('departments.delete', 'Can delete departments', SYSUTCDATETIME()),
    ('departments.view', 'Can view departments', SYSUTCDATETIME()),
    ('reports.view', 'Can view reports', SYSUTCDATETIME()),
    ('audit.view', 'Can view audit logs', SYSUTCDATETIME()),
    ('settings.edit', 'Can edit system settings', SYSUTCDATETIME());
GO

-- ============================================================
-- 3. RolePermissions (Admin gets all permissions)
-- ============================================================
-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, PermissionId FROM Permissions;
GO

-- HOD gets specific permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, PermissionId FROM Permissions 
WHERE PermissionKey IN ('requests.view', 'approvals.decide', 'approvals.view', 'departments.view', 'requests.create');
GO

-- Technician permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, PermissionId FROM Permissions 
WHERE PermissionKey IN ('requests.view', 'requests.create', 'requests.assign', 'departments.view');
GO

-- Requestor permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 4, PermissionId FROM Permissions 
WHERE PermissionKey IN ('requests.create', 'requests.view', 'departments.view');
GO

-- ============================================================
-- 4. Departments
-- ============================================================
INSERT INTO Departments (DepartmentName, DepartmentCode, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    ('Information Technology', 'IT', 'IT Infrastructure and Support', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Maintenance', 'MAINT', 'Facility and Equipment Maintenance', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Housekeeping', 'HK', 'Cleaning and Janitorial Services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Human Resources', 'HR', 'Employee Relations and Benefits', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Sales', 'SALES', 'Sales and Business Development', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Finance', 'FIN', 'Financial Operations', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO

-- ============================================================
-- 5. Users
-- ============================================================
INSERT INTO Users (EmployeeId, FullName, Email, PasswordHash, RoleId, DepartmentId, Phone, Status, JoinedDate, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    ('EMP-0001', 'Admin User', 'admin@company.com', 'admin123', 1, 1, '+919876543210', 'Active', '2023-01-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0002', 'John Doe', 'john.doe@company.com', 'password123', 2, 1, '+919876543211', 'Active', '2023-01-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0003', 'Jane Smith', 'jane.smith@company.com', 'password123', 2, 2, '+919876543212', 'Active', '2023-02-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0004', 'Mike Johnson', 'mike.johnson@company.com', 'password123', 3, 1, '+919876543213', 'Active', '2023-02-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0005', 'Sarah Williams', 'sarah.williams@company.com', 'password123', 3, 1, '+919876543214', 'Active', '2023-03-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0006', 'Robert Brown', 'robert.brown@company.com', 'password123', 3, 2, '+919876543215', 'Active', '2023-03-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0007', 'Lisa Davis', 'lisa.davis@company.com', 'password123', 4, 1, '+919876543216', 'Active', '2023-04-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0008', 'David Wilson', 'david.wilson@company.com', 'password123', 4, 2, '+919876543217', 'Active', '2023-04-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0009', 'Emma Taylor', 'emma.taylor@company.com', 'password123', 4, 3, '+919876543218', 'Active', '2023-05-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0010', 'James Anderson', 'james.anderson@company.com', 'password123', 4, 4, '+919876543219', 'Active', '2023-05-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0011', 'Maria Garcia', 'maria.garcia@company.com', 'password123', 4, 5, '+919876543220', 'Active', '2023-06-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0012', 'Thomas Martinez', 'thomas.martinez@company.com', 'password123', 4, 6, '+919876543221', 'Active', '2023-06-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0013', 'Patricia Lee', 'patricia.lee@company.com', 'password123', 3, 3, '+919876543222', 'Active', '2023-07-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0014', 'Charles Clark', 'charles.clark@company.com', 'password123', 2, 3, '+919876543223', 'Active', '2023-07-15', SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('EMP-0015', 'Jennifer Rodriguez', 'jennifer.rodriguez@company.com', 'password123', 2, 4, '+919876543224', 'Active', '2023-08-01', SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO

-- ============================================================
-- 6. UserSettings
-- ============================================================
INSERT INTO UserSettings (UserId, Theme, TwoFactorEnabled, NotifyRequestUpdates, NotifyApprovalAlerts, NotifySLAWarnings, NotifyAssetEvents, NotifyEmailDigest, UpdatedAt)
SELECT UserId, 'light', 0, 1, 1, 1, 0, 0, SYSUTCDATETIME() FROM Users;
GO

-- ============================================================
-- 7. DepartmentPersonnel (HOD and Technicians)
-- ============================================================
-- IT Department Personnel
INSERT INTO DepartmentPersonnel (UserId, DepartmentId, IsHOD, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (2, 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- John Doe is HOD of IT
    (4, 1, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Mike Johnson is IT Technician
    (5, 1, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Sarah Williams is IT Technician

-- Maintenance Department Personnel
INSERT INTO DepartmentPersonnel (UserId, DepartmentId, IsHOD, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (3, 2, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Jane Smith is HOD of Maintenance
    (6, 2, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Robert Brown is Maintenance Technician

-- Housekeeping Department Personnel
INSERT INTO DepartmentPersonnel (UserId, DepartmentId, IsHOD, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (14, 3, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Charles Clark is HOD of Housekeeping
    (13, 3, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Patricia Lee is Housekeeping Staff

-- HR Department Personnel
INSERT INTO DepartmentPersonnel (UserId, DepartmentId, IsHOD, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    (15, 4, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Jennifer Rodriguez is HOD of HR
GO

-- ============================================================
-- 8. ServiceTypes
-- ============================================================
INSERT INTO ServiceTypes (ServiceTypeName, ServiceTypeCode, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    ('Technical', 'TECH', 'IT and Technical Support Services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Facility', 'FAC', 'Facility and Infrastructure Services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Administrative', 'ADMIN', 'Administrative and Office Services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO

-- ============================================================
-- 9. RequestTypes
-- ============================================================
INSERT INTO RequestTypes (RequestTypeName, ServiceTypeId, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    ('Computer Issue', 1, 'Hardware or software issues with computers', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Network Issue', 1, 'Internet, Wi-Fi, or network connectivity problems', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Printer Issue', 1, 'Printer not working or paper jam', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Software Installation', 1, 'Request for software installation or upgrade', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Access Request', 1, 'Request for system or application access', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AC Repair', 2, 'Air conditioning not cooling or making noise', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Electrical Issue', 2, 'Power outage, flickering lights, or electrical faults', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Plumbing Issue', 2, 'Leaking pipes, clogged drains, or faucet issues', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Furniture Repair', 2, 'Broken chairs, tables, or office furniture', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Cleaning Service', 3, 'Deep cleaning, carpet cleaning, or sanitation services', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Stationery Request', 3, 'Request for office stationery supplies', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Meeting Room Setup', 3, 'Setting up meeting rooms with AV equipment', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Software Purchase', 1, 'Request to purchase new software licenses', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('Hardware Purchase', 1, 'Request to purchase new hardware equipment', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO

-- ============================================================
-- 10. ServiceRequestStatuses
-- ============================================================
INSERT INTO ServiceRequestStatuses (StatusName, ColorCode, Description, IsActive, CreatedAt)
VALUES 
    ('Open', 'bg-blue-100 text-blue-800', 'Request has been created', 1, SYSUTCDATETIME()),
    ('Assigned', 'bg-purple-100 text-purple-800', 'Request has been assigned to a technician', 1, SYSUTCDATETIME()),
    ('Pending Approval', 'bg-yellow-100 text-yellow-800', 'Request is waiting for HOD approval', 1, SYSUTCDATETIME()),
    ('In Progress', 'bg-indigo-100 text-indigo-800', 'Technician is actively working on the request', 1, SYSUTCDATETIME()),
    ('Resolved', 'bg-green-100 text-green-800', 'Request has been resolved', 1, SYSUTCDATETIME()),
    ('Closed', 'bg-gray-100 text-gray-800', 'Request has been closed', 1, SYSUTCDATETIME()),
    ('Cancelled', 'bg-red-100 text-red-800', 'Request has been cancelled', 1, SYSUTCDATETIME()),
    ('Rejected', 'bg-red-100 text-red-800', 'Request has been rejected by HOD', 1, SYSUTCDATETIME());
GO

-- ============================================================
-- 11. AssetCategories
-- ============================================================
INSERT INTO AssetCategories (CategoryName, IsActive, CreatedAt)
VALUES 
    ('Laptop', 1, SYSUTCDATETIME()),
    ('Desktop', 1, SYSUTCDATETIME()),
    ('Printer', 1, SYSUTCDATETIME()),
    ('Network Device', 1, SYSUTCDATETIME()),
    ('HVAC', 1, SYSUTCDATETIME()),
    ('Furniture', 1, SYSUTCDATETIME()),
    ('AV Equipment', 1, SYSUTCDATETIME()),
    ('Phone', 1, SYSUTCDATETIME());
GO

-- ============================================================
-- 12. Assets
-- ============================================================
INSERT INTO Assets (AssetTag, AssetName, CategoryId, SerialNumber, AssignedToUserId, DepartmentId, Status, PurchaseDate, WarrantyUntil, BookValue, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    ('AST-IT-0001', 'Dell Latitude 7440', 1, 'SN-DELL-001', 2, 1, 'In Use', '2024-01-15', '2027-01-15', 120000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-IT-0002', 'HP EliteBook 840', 1, 'SN-HP-001', 4, 1, 'In Use', '2024-02-01', '2027-02-01', 95000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-IT-0003', 'Dell OptiPlex 7070', 2, 'SN-DELL-DESK-001', NULL, 1, 'Available', '2023-12-01', '2026-12-01', 85000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-IT-0004', 'HP LaserJet Pro', 3, 'SN-HP-PRINT-001', NULL, 1, 'Available', '2023-11-15', '2026-11-15', 45000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-FAC-0001', 'Daikin AC Unit', 5, 'SN-DAIKIN-001', NULL, 2, 'Available', '2023-10-01', '2026-10-01', 65000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-FAC-0002', 'Generic Office Chair', 6, 'SN-CHAIR-001', 5, 1, 'In Use', '2024-01-01', '2027-01-01', 15000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-IT-0005', 'Cisco Switch', 4, 'SN-CISCO-001', NULL, 1, 'Available', '2023-09-15', '2026-09-15', 75000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-AV-0001', 'Projector Epson EB-2250U', 7, 'SN-EPSON-001', NULL, 1, 'Available', '2024-02-15', '2027-02-15', 55000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-IT-0006', 'iPhone 15 Pro', 8, 'SN-APPLE-001', 7, 1, 'In Use', '2024-03-01', '2027-03-01', 80000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    ('AST-FAC-0003', 'Conference Table', 6, 'SN-TABLE-001', NULL, 1, 'Available', '2023-08-01', '2026-08-01', 25000.00, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO

-- ============================================================
-- 13. RequestTypeTechnicianMappings
-- ============================================================
INSERT INTO RequestTypeTechnicianMappings (RequestTypeId, DepartmentPersonnelId, IsActive, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
    -- IT Department Technician Mappings
    (1, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Computer Issue -> Mike Johnson
    (2, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Network Issue -> Mike Johnson
    (3, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Printer Issue -> Sarah Williams
    (4, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Software Installation -> Mike Johnson
    (5, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Access Request -> Sarah Williams
    (13, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Software Purchase -> Mike Johnson
    (14, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Hardware Purchase -> Sarah Williams
    
    -- Maintenance Department Technician Mappings
    (6, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- AC Repair -> Robert Brown
    (7, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Electrical Issue -> Robert Brown
    (8, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Plumbing Issue -> Robert Brown
    (9, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Furniture Repair -> Robert Brown
    
    -- Housekeeping Department Technician Mappings
    (10, 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Cleaning Service -> Patricia Lee
    (11, 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0), -- Stationery Request -> Patricia Lee
    (12, 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0); -- Meeting Room Setup -> Patricia Lee
GO

-- ============================================================
-- 14. ServiceRequests
-- ============================================================
INSERT INTO ServiceRequests (RequestNumber, Title, Description, ServiceTypeId, RequestTypeId, DepartmentId, RequesterUserId, AssigneeUserId, StatusId, Priority, CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, IsDeleted)
VALUES 
    ('SR-2026-0001', 'Laptop not booting up', 'My Dell laptop is not booting up. It shows a blue screen with error code 0x0000000F. I have tried restarting multiple times but it keeps failing. Need immediate assistance.', 1, 1, 1, 7, 4, 4, 'High', '2026-01-15 09:30:00', 1, '2026-01-15 09:30:00', 1, 0),
    ('SR-2026-0002', 'Wi-Fi connectivity issues', 'The Wi-Fi connection is very slow and disconnects frequently. This is affecting my work and productivity. Please check the router and network configuration.', 1, 2, 1, 8, 4, 4, 'Medium', '2026-01-16 10:15:00', 1, '2026-01-16 10:15:00', 1, 0),
    ('SR-2026-0003', 'Printer not working', 'The HP LaserJet printer in the IT department is not printing. It shows a paper jam error but there is no paper stuck. Please check and repair.', 1, 3, 1, 9, 5, 4, 'Low', '2026-01-17 11:00:00', 1, '2026-01-17 11:00:00', 1, 0),
    ('SR-2026-0004', 'Need Microsoft Office installation', 'I need Microsoft Office 365 installed on my new laptop for work. Please install it as soon as possible.', 1, 4, 1, 10, 4, 4, 'Medium', '2026-01-18 14:20:00', 1, '2026-01-18 14:20:00', 1, 0),
    ('SR-2026-0005', 'Access to HR Management System', 'I need access to the HR Management System to update employee records and process leave requests. Please grant me appropriate permissions.', 1, 5, 4, 11, 5, 3, 'High', '2026-01-19 09:00:00', 1, '2026-01-19 09:00:00', 1, 0),
    ('SR-2026-0006', 'AC not cooling properly', 'The AC in meeting room A is not cooling properly. The room temperature is 28°C even though the AC is set to 18°C. Please check and fix.', 2, 6, 2, 12, 6, 4, 'High', '2026-01-20 13:45:00', 1, '2026-01-20 13:45:00', 1, 0),
    ('SR-2026-0007', 'Power outage in floor 3', 'The power went out on the 3rd floor. Only emergency lights are working. Please check the electrical panel and restore power.', 2, 7, 2, 2, 6, 4, 'Critical', '2026-01-21 08:30:00', 1, '2026-01-21 08:30:00', 1, 0),
    ('SR-2026-0008', 'Leaking pipe in pantry', 'There is a water leak from the pipe under the sink in the pantry on floor 2. Water is spreading on the floor. Please fix urgently.', 2, 8, 2, 3, 6, 4, 'High', '2026-01-22 15:10:00', 1, '2026-01-22 15:10:00', 1, 0),
    ('SR-2026-0009', 'Broken office chair', 'My office chair is broken. The hydraulic lift is not working and the armrest is loose. Please replace or repair.', 2, 9, 2, 4, 6, 4, 'Low', '2026-01-23 11:30:00', 1, '2026-01-23 11:30:00', 1, 0),
    ('SR-2026-0010', 'Deep cleaning for executive floor', 'Need deep cleaning for the executive floor including carpet cleaning, window cleaning, and dusting. This is a weekly requirement.', 3, 10, 3, 5, 13, 4, 'Medium', '2026-01-24 10:00:00', 1, '2026-01-24 10:00:00', 1, 0),
    ('SR-2026-0011', 'Stationery request', 'Need stationery supplies including pens, notebooks, and printer paper for the HR department. Please provide 50 notebooks, 100 pens, and 10 reams of paper.', 3, 11, 4, 15, 13, 4, 'Low', '2026-01-25 09:15:00', 1, '2026-01-25 09:15:00', 1, 0),
    ('SR-2026-0012', 'Meeting room AV setup', 'Need AV equipment setup for the board meeting tomorrow. Need projector, sound system, and video conferencing setup.', 3, 12, 1, 2, 13, 4, 'Medium', '2026-01-26 16:00:00', 1, '2026-01-26 16:00:00', 1, 0),
    ('SR-2026-0013', 'Software purchase request', 'Need to purchase Adobe Creative Cloud licenses for the design team. Need 5 licenses for 1 year. Please approve and process.', 1, 13, 1, 2, 4, 3, 'High', '2026-01-27 11:20:00', 1, '2026-01-27 11:20:00', 1, 0),
    ('SR-2026-0014', 'Hardware purchase request', 'Need to purchase 10 new Dell monitors for the development team. Specification: 27-inch 4K monitors. Please approve and process.', 1, 14, 1, 2, 4, 3, 'Medium', '2026-01-28 14:45:00', 1, '2026-01-28 14:45:00', 1, 0),
    ('SR-2026-0015', 'Network speed issue', 'Network speed is very slow in the sales department. Videos are buffering and files take too long to download. Please check and fix.', 1, 2, 1, 11, 4, 4, 'High', '2026-01-29 09:30:00', 1, '2026-01-29 09:30:00', 1, 0);
GO

-- ============================================================
-- 15. ServiceRequestReplies
-- ============================================================
INSERT INTO ServiceRequestReplies (RequestId, AuthorUserId, Message, StatusTransitionId, CreatedAt)
VALUES 
    (1, 4, 'I have checked the laptop. It seems to be a hardware issue with the RAM. I will need to replace the RAM module. I will order the replacement today.', 4, '2026-01-15 10:30:00'),
    (1, 7, 'Thank you for the update. Please let me know when the replacement is done.', NULL, '2026-01-15 10:35:00'),
    (1, 4, 'RAM has been replaced. Laptop is working fine now. Please check and confirm.', 5, '2026-01-15 15:20:00'),
    (1, 7, 'Laptop is working perfectly. Thank you for the quick resolution.', 6, '2026-01-15 15:45:00'),
    (2, 4, 'I checked the Wi-Fi router and restarted it. The speed is now normal. Please confirm if you still face issues.', 4, '2026-01-16 11:30:00'),
    (2, 8, 'Wi-Fi is working great now. Thank you!', 5, '2026-01-16 11:45:00'),
    (5, 5, 'Access request has been processed. I have granted read and write permissions to the HR system.', 5, '2026-01-19 14:00:00'),
    (5, 11, 'Thank you for the access. I can now update records.', 6, '2026-01-19 14:30:00'),
    (6, 6, 'I have checked the AC. The refrigerant level was low. I have refilled the gas and the AC is now cooling properly.', 5, '2026-01-20 16:30:00'),
    (6, 12, 'AC is working great now. Thank you for the quick service.', 6, '2026-01-20 17:00:00');
GO

-- ============================================================
-- 16. ServiceRequestTimeline
-- ============================================================
INSERT INTO ServiceRequestTimeline (RequestId, StatusId, ChangedByUserId, ChangedAt, Note)
VALUES 
    (1, 1, 1, '2026-01-15 09:30:00', 'Request created'),
    (1, 3, 1, '2026-01-15 09:35:00', 'Pending HOD approval'),
    (1, 4, 2, '2026-01-15 09:45:00', 'Approved by HOD and assigned to technician'),
    (1, 5, 4, '2026-01-15 15:20:00', 'Resolved by technician'),
    (1, 6, 7, '2026-01-15 15:45:00', 'Closed by requester'),
    (2, 1, 1, '2026-01-16 10:15:00', 'Request created'),
    (2, 4, 4, '2026-01-16 10:30:00', 'Assigned to technician'),
    (2, 5, 4, '2026-01-16 11:30:00', 'Resolved'),
    (2, 6, 8, '2026-01-16 11:45:00', 'Closed by requester'),
    (5, 1, 1, '2026-01-19 09:00:00', 'Request created'),
    (5, 3, 1, '2026-01-19 09:10:00', 'Pending HOD approval'),
    (5, 4, 15, '2026-01-19 10:00:00', 'Approved by HOD'),
    (5, 5, 5, '2026-01-19 14:00:00', 'Access granted'),
    (5, 6, 11, '2026-01-19 14:30:00', 'Closed by requester');
GO

-- ============================================================
-- 17. ServiceRequestAttachments
-- ============================================================
INSERT INTO ServiceRequestAttachments (RequestId, ReplyId, FileName, FileSizeKB, FileUrl, UploadedByUserId, UploadedAt)
VALUES 
    (1, NULL, 'error_screenshot.png', 256, 'https://storage.service.com/attachments/5f89c3db-24b5-4b1f-9ae2-581ef5f72da0.png', 7, '2026-01-15 09:35:00'),
    (1, 1, 'ram_diagnostic_report.pdf', 512, 'https://storage.service.com/attachments/6f89c3db-24b5-4b1f-9ae2-581ef5f72da1.pdf', 4, '2026-01-15 10:35:00'),
    (2, NULL, 'wifi_speed_test.png', 128, 'https://storage.service.com/attachments/7f89c3db-24b5-4b1f-9ae2-581ef5f72da2.png', 8, '2026-01-16 10:20:00'),
    (6, NULL, 'ac_temperature_reading.jpg', 384, 'https://storage.service.com/attachments/8f89c3db-24b5-4b1f-9ae2-581ef5f72da3.jpg', 12, '2026-01-20 13:50:00');
GO

-- ============================================================
-- 18. Approvals
-- ============================================================
INSERT INTO Approvals (RequestId, Status, DecidedByUserId, DecidedAt, Remarks, SubmittedAt)
VALUES 
    (5, 'Approved', 15, '2026-01-19 10:00:00', 'Approved. Access granted for HR system.', '2026-01-19 09:10:00'),
    (13, 'Pending', NULL, NULL, NULL, '2026-01-27 11:20:00'),
    (14, 'Pending', NULL, NULL, NULL, '2026-01-28 14:45:00');
GO

-- ============================================================
-- 19. Notifications
-- ============================================================
INSERT INTO Notifications (UserId, Title, Message, IsRead, NotificationType, CreatedAt)
VALUES 
    (2, 'New HOD Approval Request', 'John Doe - A new request has been submitted for your approval.', 0, 'approval', '2026-01-27 11:20:00'),
    (2, 'New HOD Approval Request', 'John Doe - A new purchase request requires your approval.', 0, 'approval', '2026-01-28 14:45:00'),
    (4, 'New Service Request Assigned', 'Mike Johnson - You have been assigned a new service request.', 0, 'request', '2026-01-27 11:20:00'),
    (5, 'New Service Request Assigned', 'Sarah Williams - You have been assigned a new service request.', 0, 'request', '2026-01-28 14:45:00'),
    (7, 'Request Resolved', 'Lisa Davis - Your request has been resolved.', 0, 'request', '2026-01-15 15:20:00'),
    (8, 'Request Resolved', 'David Wilson - Your request has been resolved.', 0, 'request', '2026-01-16 11:30:00'),
    (11, 'Request Resolved', 'Maria Garcia - Your access request has been resolved.', 0, 'request', '2026-01-19 14:00:00'),
    (12, 'Request Resolved', 'Thomas Martinez - Your request has been resolved.', 0, 'request', '2026-01-20 16:30:00'),
    (2, 'System Maintenance', 'Scheduled maintenance on Sunday 2 AM to 4 AM.', 0, 'system', '2026-01-25 08:00:00'),
    (2, 'System Maintenance', 'Database backup completed successfully.', 0, 'system', '2026-01-26 04:00:00'),
    (3, 'New Service Request Assigned', 'Jane Smith - You have been assigned a new service request.', 0, 'request', '2026-01-20 13:45:00');
GO

-- ============================================================
-- 20. AuditLogs
-- ============================================================
INSERT INTO AuditLogs (ActorUserId, Action, TargetType, TargetId, TargetDisplay, Detail, IpAddress, CreatedAt)
VALUES 
    (1, 'Created user', 'User', '2', 'John Doe', 'Created new user with role HOD', '192.168.1.1', '2026-01-15 08:00:00'),
    (1, 'Created department', 'Department', '1', 'Information Technology', 'Created new IT department', '192.168.1.1', '2026-01-15 08:15:00'),
    (2, 'Approved request', 'ServiceRequest', '1', 'SR-2026-0001', 'Approved laptop request', '192.168.1.2', '2026-01-15 09:45:00'),
    (4, 'Updated request status', 'ServiceRequest', '1', 'SR-2026-0001', 'Status changed from Assigned to Resolved', '192.168.1.3', '2026-01-15 15:20:00'),
    (1, 'Created asset', 'Asset', '1', 'AST-IT-0001', 'Created new laptop asset', '192.168.1.1', '2026-01-15 16:00:00'),
    (15, 'Approved access request', 'Approval', '1', 'Approval for SR-2026-0005', 'Approved access to HR system', '192.168.1.4', '2026-01-19 10:00:00'),
    (5, 'Granted access', 'ServiceRequest', '5', 'SR-2026-0005', 'Granted access to HR system', '192.168.1.5', '2026-01-19 14:00:00'),
    (1, 'Updated user', 'User', '7', 'Lisa Davis', 'Updated user role', '192.168.1.1', '2026-01-20 09:00:00'),
    (6, 'Resolved AC issue', 'ServiceRequest', '6', 'SR-2026-0006', 'AC repaired successfully', '192.168.1.6', '2026-01-20 16:30:00'),
    (1, 'Created notification', 'Notification', '1', 'System Maintenance', 'Created system maintenance notification', '192.168.1.1', '2026-01-25 08:00:00'),
    (1, 'Logged in', 'User', '1', 'Admin User', 'Admin logged in from IP 192.168.1.1', '192.168.1.1', '2026-01-26 09:00:00'),
    (1, 'Deleted asset', 'Asset', '10', 'Conference Table', 'Soft deleted asset', '192.168.1.1', '2026-01-26 10:00:00');
GO

-- ============================================================
-- END OF SQL SCRIPT
-- ============================================================