# Step 02 — Final Backend Requirements

## 1. Objective

Define the final backend project scope, feature requirements, entity list, database table counts, enums, roles, and module boundaries based on the React frontend source code.

---

## 2. Why This Step Is Required

Creating a clear list of required entities, tables, and module boundaries prevents scope creep, eliminates unnecessary database tables (such as dynamic permissions tables or asset categories masters), and ensures every frontend page is fully supported by backend capabilities.

---

## 3. Prerequisites

* Step 01 completed.
* Reviewed React frontend routes (`frontend/src/routes/`), components, and seed data in `frontend/src/data/mock.js`.

---

## 4. What Needs To Be Done

1. Analyze all frontend pages to list every required feature.
2. Confirm the 16 core required database tables.
3. Confirm the 6 fixed C# enums.
4. Confirm the 4 system roles and access boundaries.
5. Identify required vs unnecessary features from old backend planning.

---

## 5. Project-Specific Requirements

### 1. Final Required Entities (16 Core Entities)
1. `User`: Account identity and credentials.
2. `UserSettings`: Theme preference, 2FA toggle, 5 email notification checkboxes.
3. `Department`: Master organizational units.
4. `DepartmentPersonnel`: Staff roster mapping users to departments as HOD or Technician.
5. `ServiceType`: Top-level categories (Technical, Facility, Administrative).
6. `RequestType`: Specific sub-categories (Computer Issue, Software Request, AC Repair).
7. `RequestTypeTechnicianMapping`: Rules mapping Request Types to default Technicians.
8. `ServiceRequestStatus`: Master ticket statuses (`Open`, `In Progress`, `Pending Approval`, `Resolved`, `Closed`, `Rejected`) with badge color codes.
9. `ServiceRequest`: Core ticket entity.
10. `ServiceRequestReply`: Ticket discussion thread messages.
11. `ServiceRequestTimeline`: Audit log of status changes and assignments.
12. `ServiceRequestAttachment`: File metadata (`FileName`, `FileSizeKB`, `FileUrl`).
13. `Approval`: HOD sign-off records with decision timestamp & remarks.
14. `Asset`: Hardware & equipment inventory tracking with INR book value (`DECIMAL(18,2)`).
15. `Notification`: User alert inbox items.
16. `AuditLog`: System-wide write-only activity tracking.

### 2. Final Enums (6 Enums)
* `UserRole`: `Admin`, `HOD`, `Technician`, `Requestor`
* `UserStatus`: `Active`, `Inactive`
* `Priority`: `Critical`, `High`, `Medium`, `Low`
* `AssetStatus`: `In Use`, `Available`, `Under Repair`, `Retired`
* `ApprovalStatus`: `Pending`, `Approved`, `Rejected`
* `NotificationType`: `request`, `approval`, `asset`, `system`

---

## 6. Important Decisions

* **Removed Unnecessary Tables**: Dynamic permission tables (`dbo.Permissions`, `dbo.RolePermissions`) and standalone `dbo.AssetCategories` are removed as database table requirements. Roles are fixed in code via C# Enum `UserRole`, and Asset Categories are stored as string/Enum on `dbo.Assets`.
* **Required Tables Retained**: `ServiceRequestStatuses` remains a database table because the frontend provides a Master CRUD screen in `masters.jsx` to manage status names, descriptions, and badge color codes.

---

## 7. Dependencies

* **Upstream**: Step 01 (Project Overview).
* **Downstream**: Step 03 (Project Setup), Step 04 (Database Design).

---

## 8. Expected Result

After completing this step:

✓ Final backend scope is locked at 16 Core Entities and 16 Database Tables.
✓ 6 System Enums are confirmed.
✓ 4 User Roles are finalized.
✓ All unnecessary entities and tables from previous planning are pruned.

---

## 9. Verification Checklist

- [ ] Confirmed exactly 16 core required database tables.
- [ ] Confirmed 6 system enums.
- [ ] Confirmed 4 user roles (`Admin`, `HOD`, `Technician`, `Requestor`).
- [ ] Verified that dynamic permission tables are pruned.
- [ ] Verified that `ServiceRequestStatuses` is retained as a master table.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [03_Backend_Project_Setup.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/03_Backend_Project_Setup.md)**
