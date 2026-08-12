# Step 05 — Models & Entity Data Structures

## 1. Objective

Define the exact properties, data types, nullability rules, and descriptions for all 16 core domain entities and 6 system enums in `ServiceDesk.Core`.

---

## 2. Why This Step Is Required

Explicit property definitions ensure data contract agreement between the ASP.NET Core API and the React frontend. It prevents null reference crashes and mismatched data types.

---

## 3. Prerequisites

* Step 04 (Database Design) completed.

---

## 4. What Needs To Be Done

Map the property requirements for all 16 core entities:

### 1. User
* `UserId`: Primary Key (Integer, Auto-increment)
* `EmployeeId`: Unique String (Required, max 20 chars, e.g. `EMP-0001`)
* `FullName`: String (Required, max 100 chars)
* `Email`: Unique String (Required, max 256 chars)
* `PasswordHash`: String (Required, max 256 chars)
* `Role`: Enum `UserRole` (Required, `Admin`, `HOD`, `Technician`, `Requestor`)
* `DepartmentId`: Foreign Key -> `Department` (Optional/Nullable)
* `Phone`: String (Optional, max 20 chars)
* `Status`: Enum `UserStatus` (Required, `Active` or `Inactive`)
* `JoinedDate`: Date (Required)
* `LastLoginAt`: DateTime (Optional/Nullable)
* `CreatedAt`, `UpdatedAt`: DateTime (Required)
* `IsDeleted`: Boolean (Required, default `false`)

### 2. UserSettings
* `UserId`: Primary Key & Foreign Key -> `User` (1-to-1)
* `Theme`: String (Required, `light` or `dark`)
* `TwoFactorEnabled`: Boolean (Required)
* `NotifyRequestUpdates`, `NotifyApprovalAlerts`, `NotifySLAWarnings`, `NotifyAssetEvents`, `NotifyEmailDigest`: Boolean (Required)

### 3. Department
* `DepartmentId`: Primary Key (Integer, Auto-increment)
* `DepartmentName`: Unique String (Required, max 100 chars)
* `DepartmentCode`: Unique String (Required, max 10 chars)
* `Description`: String (Optional, max 250 chars)
* `IsActive`: Boolean (Required)

### 4. DepartmentPersonnel
* `DepartmentPersonnelId`: Primary Key (Integer, Auto-increment)
* `UserId`: Foreign Key -> `User` (Required)
* `DepartmentId`: Foreign Key -> `Department` (Required)
* `IsHOD`: Boolean (Required)
* `IsActive`: Boolean (Required)

### 5. ServiceType
* `ServiceTypeId`: Primary Key (Integer, Auto-increment)
* `ServiceTypeName`: Unique String (Required, max 50 chars)
* `ServiceTypeCode`: Unique String (Required, max 10 chars)
* `Description`: String (Optional, max 250 chars)
* `IsActive`: Boolean (Required)

### 6. RequestType
* `RequestTypeId`: Primary Key (Integer, Auto-increment)
* `RequestTypeName`: Unique String (Required, max 100 chars)
* `ServiceTypeId`: Foreign Key -> `ServiceType` (Required)
* `Description`: String (Optional, max 250 chars)
* `RequiresApproval`: Boolean (Required, true for Software/Hardware/Access)
* `IsActive`: Boolean (Required)

### 7. RequestTypeTechnicianMapping
* `MappingId`: Primary Key (Integer, Auto-increment)
* `RequestTypeId`: Foreign Key -> `RequestType` (Required)
* `DepartmentPersonnelId`: Foreign Key -> `DepartmentPersonnel` (Required)
* `IsActive`: Boolean (Required)

### 8. ServiceRequestStatus
* `StatusId`: Primary Key (Integer, Auto-increment)
* `StatusName`: Unique String (Required, max 50 chars, e.g. `Open`, `In Progress`, `Pending Approval`, `Resolved`, `Closed`, `Rejected`)
* `ColorCode`: String (Optional, Tailwind badge class string)
* `Description`: String (Optional, max 250 chars)
* `IsActive`: Boolean (Required)

### 9. ServiceRequest
* `RequestId`: Primary Key (Integer, Auto-increment)
* `RequestNumber`: Unique String (Required, max 20 chars, e.g. `SR-2026-1041`)
* `Title`: String (Required, max 150 chars)
* `Description`: String (Required, min 20 chars)
* `ServiceTypeId`: Foreign Key -> `ServiceType` (Required)
* `RequestTypeId`: Foreign Key -> `RequestType` (Required)
* `DepartmentId`: Foreign Key -> `Department` (Required)
* `RequesterUserId`: Foreign Key -> `User` (Required)
* `AssigneeUserId`: Foreign Key -> `User` (Optional/Nullable)
* `StatusId`: Foreign Key -> `ServiceRequestStatus` (Required)
* `Priority`: Enum `Priority` (Required, `Critical`, `High`, `Medium`, `Low`)
* `CreatedAt`, `UpdatedAt`: DateTime (Required)

### 10. ServiceRequestReply
* `ReplyId`: Primary Key (Integer, Auto-increment)
* `RequestId`: Foreign Key -> `ServiceRequest` (Required)
* `AuthorUserId`: Foreign Key -> `User` (Required)
* `Message`: String (Required)
* `StatusTransitionId`: Foreign Key -> `ServiceRequestStatus` (Optional/Nullable)
* `CreatedAt`: DateTime (Required)

### 11. ServiceRequestTimeline
* `TimelineId`: Primary Key (Integer, Auto-increment)
* `RequestId`: Foreign Key -> `ServiceRequest` (Required)
* `StatusName`: String (Required)
* `ChangedByUserId`: Foreign Key -> `User` (Required)
* `ChangedAt`: DateTime (Required)
* `Note`: String (Required, max 500 chars)

### 12. ServiceRequestAttachment
* `AttachmentId`: Primary Key (Integer, Auto-increment)
* `RequestId`: Foreign Key -> `ServiceRequest` (Required)
* `ReplyId`: Foreign Key -> `ServiceRequestReply` (Optional/Nullable)
* `FileName`: String (Required, max 256 chars)
* `FileSizeKB`: Integer (Required)
* `FileUrl`: String (Required, max 2048 chars)
* `UploadedByUserId`: Foreign Key -> `User` (Required)
* `UploadedAt`: DateTime (Required)

### 13. Approval
* `ApprovalId`: Primary Key (Integer, Auto-increment)
* `RequestId`: Foreign Key -> `ServiceRequest` (Required)
* `Status`: Enum `ApprovalStatus` (Required, `Pending`, `Approved`, `Rejected`)
* `DecidedByUserId`: Foreign Key -> `User` (Optional/Nullable)
* `DecidedAt`: DateTime (Optional/Nullable)
* `Remarks`: String (Optional, max 1000 chars)
* `SubmittedAt`: DateTime (Required)

### 14. Asset
* `AssetId`: Primary Key (Integer, Auto-increment)
* `AssetTag`: Unique String (Required, max 30 chars, e.g. `AST-IT-0142`)
* `AssetName`: String (Required, max 100 chars)
* `Category`: String/Enum (Required, e.g. `Laptop`, `Printer`, `Monitor`, `HVAC`, `Network`)
* `SerialNumber`: Unique String (Required, max 100 chars)
* `AssignedToUserId`: Foreign Key -> `User` (Optional/Nullable)
* `DepartmentId`: Foreign Key -> `Department` (Optional/Nullable)
* `Status`: Enum `AssetStatus` (Required, `In Use`, `Available`, `Under Repair`, `Retired`)
* `PurchaseDate`, `WarrantyUntil`: Date (Required)
* `BookValue`: Decimal (Required, precision `18,2`)

### 15. Notification
* `NotificationId`: Primary Key (Integer, Auto-increment)
* `UserId`: Foreign Key -> `User` (Required)
* `Title`: String (Required, max 150 chars)
* `Message`: String (Required, max 500 chars)
* `IsRead`: Boolean (Required, default `false`)
* `NotificationType`: Enum `NotificationType` (Required, `request`, `approval`, `asset`, `system`)
* `CreatedAt`: DateTime (Required)

### 16. AuditLog
* `AuditLogId`: Primary Key (BigInt, Auto-increment)
* `ActorUserId`: Foreign Key -> `User` (Optional/Nullable)
* `Action`: String (Required, max 100 chars)
* `TargetType`: String (Required, max 50 chars)
* `TargetId`: String (Optional, max 50 chars)
* `TargetDisplay`: String (Optional, max 100 chars)
* `Detail`: String (Optional)
* `IpAddress`: String (Optional, max 45 chars)
* `CreatedAt`: DateTime (Required)

---

## 5. Project-Specific Requirements

* All 16 entities must reside in `ServiceDesk.Core/Entities/`.
* All 6 Enums must reside in `ServiceDesk.Core/Enums/`.

---

## 6. Important Decisions

* **Enums over Tables**: Fixed value sets (`UserRole`, `UserStatus`, `Priority`, `AssetStatus`, `ApprovalStatus`, `NotificationType`) are defined as C# Enums rather than database tables because their values do not require admin CRUD screens in the frontend.

---

## 7. Dependencies

* **Upstream**: Step 04 (Database Design).
* **Downstream**: Step 06 (Relationships & Constraints), Step 07 (DbContext Planning).

---

## 8. Expected Result

After completing this step:

✓ All 16 entity data structures are fully defined.
✓ All 6 system enums are defined.
✓ Field nullabilities and character length constraints are specified.

---

## 9. Verification Checklist

- [ ] Defined properties for all 16 core entities.
- [ ] Confirmed 6 enum structures.
- [ ] Verified `RequiresApproval` boolean property on `RequestType`.
- [ ] Verified `ColorCode` property on `ServiceRequestStatus`.
- [ ] Verified `BookValue` precision constraint on `Asset`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [06_Relationships_and_Constraints.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/06_Relationships_and_Constraints.md)**
