# Step 04 — Database Design & Schema Planning

## 1. Objective

Plan the Microsoft SQL Server relational database schema for all 16 core required tables, specifying table names, normalization levels, soft-delete flags, and audit field standards.

---

## 2. Why This Step Is Required

A well-structured relational database design in Third Normal Form (3NF) prevents data redundancy, guarantees referential integrity, and optimizes performance for ticket searching and reporting queries.

---

## 3. Prerequisites

* Step 02 (Final Requirements) and Step 03 (Project Setup) completed.
* SQL Server instance accessible locally or on network.

---

## 4. What Needs To Be Done

1. Confirm database name: `ServiceDeskDb`.
2. Define schema naming convention: `dbo.<TableName>` (PascalCase plural names).
3. Plan 16 core database tables:
   * `dbo.Users`
   * `dbo.UserSettings`
   * `dbo.Departments`
   * `dbo.DepartmentPersonnel`
   * `dbo.ServiceTypes`
   * `dbo.RequestTypes`
   * `dbo.RequestTypeTechnicianMappings`
   * `dbo.ServiceRequestStatuses`
   * `dbo.ServiceRequests`
   * `dbo.ServiceRequestReplies`
   * `dbo.ServiceRequestTimeline`
   * `dbo.ServiceRequestAttachments`
   * `dbo.Approvals`
   * `dbo.Assets`
   * `dbo.Notifications`
   * `dbo.AuditLogs`
4. Standardize soft-delete columns on core entities (`IsDeleted`, `DeletedAt`, `DeletedByUserId`).
5. Standardize timestamp columns on all entities (`CreatedAt`, `UpdatedAt`).

---

## 5. Project-Specific Requirements

* **Normalization**: Design schema to satisfy Third Normal Form (3NF).
* **Soft Delete Strategy**: Apply soft delete to `Users`, `Departments`, `DepartmentPersonnel`, `ServiceTypes`, `RequestTypes`, `RequestTypeTechnicianMappings`, `ServiceRequests`, and `Assets`.
* **Hard Delete Strategy**: Transactional child records (`Replies`, `Timeline`, `Attachments`, `Notifications`, `AuditLogs`, `UserSettings`) use hard delete or cascade deletion when parent records are purged.
* **Audit Logs Strategy**: `dbo.AuditLogs` is strictly write-only (never soft-deleted or updated).

---

## 6. Important Decisions

* **Decimal Precision**: Asset book values in `dbo.Assets` are stored with SQL precision `DECIMAL(18,2)` formatted in INR currency.
* **Soft Delete Query Filters**: Soft-deleted records are filtered out using global EF Core query filters (`WHERE IsDeleted = 0`).

---

## 7. Dependencies

* **Upstream**: Step 02 (Requirements), Step 03 (Setup).
* **Downstream**: Step 05 (Models and Entities), Step 06 (Relationships), Step 07 (DbContext Planning).

---

## 8. Expected Result

After completing this step:

✓ Database schema design is established for 16 core tables.
✓ Soft delete and timestamp conventions are defined.
✓ 3NF normalization rules are applied.

---

## 9. Verification Checklist

- [ ] Confirmed database name `ServiceDeskDb`.
- [ ] Confirmed 16 core table names.
- [ ] Applied `IsDeleted`, `DeletedAt` soft-delete pattern to primary entities.
- [ ] Verified `DECIMAL(18,2)` data type for INR asset values.
- [ ] Confirmed write-only behavior for `dbo.AuditLogs`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [05_Models_and_Entities.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/05_Models_and_Entities.md)**
