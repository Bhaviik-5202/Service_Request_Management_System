# Step 11 — Configuration Masters Module Planning

## 1. Objective

Plan the CRUD APIs and validation services for the 6 configuration master management tabs in `_shell.masters.jsx` (`Statuses`, `Departments`, `Personnel`, `Service Types`, `Request Types`, `Mappings`).

---

## 2. Why This Step Is Required

The Master Data module powers all dropdown selections, approval rules, auto-routing rules, and status badge colors used across ticket forms and dashboards.

---

## 3. Prerequisites

* Step 10 (Users & Settings) completed.

---

## 4. What Needs To Be Done

Plan master data endpoints under `/api/v1/masters`:

1. **Statuses (`/api/v1/masters/statuses`)**:
   * `GET /`: Get all statuses.
   * `POST /`: Create status (`StatusName`, `ColorCode`, `Description`).
   * `PUT /{id}`: Update status name/color/description.
   * `DELETE /{id}`: Soft delete or deactivate status.

2. **Departments (`/api/v1/masters/departments`)**:
   * `GET /`: Get active departments.
   * `POST /`: Create department (`DepartmentName`, `DepartmentCode`, `Description`).
   * `PUT /{id}`: Update department.
   * `DELETE /{id}`: Soft delete department.

3. **Personnel (`/api/v1/masters/personnel`)**:
   * `GET /`: Get department staff mappings (Users mapped to Departments as HOD or Tech).
   * `POST /`: Map user to department (`UserId`, `DepartmentId`, `IsHOD`).
   * `PUT /{id}`: Update personnel mapping (`IsHOD`, `IsActive`).
   * `DELETE /{id}`: Remove personnel mapping.

4. **Service Types (`/api/v1/masters/service-types`)**:
   * `GET /`: Get service categories.
   * `POST /`: Create service type (`ServiceTypeName`, `ServiceTypeCode`).
   * `PUT /{id}`: Update service type.
   * `DELETE /{id}`: Soft delete service type.

5. **Request Types (`/api/v1/masters/request-types`)**:
   * `GET /`: Get request sub-types.
   * `POST /`: Create request type (`RequestTypeName`, `ServiceTypeId`, `RequiresApproval`).
   * `PUT /{id}`: Update request type.
   * `DELETE /{id}`: Soft delete request type.

6. **Mappings (`/api/v1/masters/mappings`)**:
   * `GET /`: Get auto-assignment rules.
   * `POST /`: Map Request Type to Technician (`RequestTypeId`, `DepartmentPersonnelId`).
   * `PUT /{id}`: Update mapping.
   * `DELETE /{id}`: Remove mapping.

---

## 5. Project-Specific Requirements

* **Access Restrictions**: Modifying master data is restricted to `Admin` (`[Authorize(Roles = "Admin")]`). Reading master lists is accessible to all authenticated users.
* **Frontend Alignment**: Master endpoints map 1-to-1 to the 6 tabs in `_shell.masters.jsx`.

---

## 6. Important Decisions

* **Unique Code Checks**: Enforce unique checks on `DepartmentCode` and `ServiceTypeCode` during creation and modification.

---

## 7. Dependencies

* **Upstream**: Step 10 (Users & Settings).
* **Downstream**: Step 12 (Service Requests), Step 13 (Approvals), Step 14 (Assignment).

---

## 8. Expected Result

After completing this step:

✓ All 6 Master CRUD API contracts are planned.
✓ Department personnel and auto-assignment mapping endpoints are established.
✓ Alignment with `_shell.masters.jsx` is confirmed.

---

## 9. Verification Checklist

- [ ] Planned CRUD endpoints for Statuses master.
- [ ] Planned CRUD endpoints for Departments master.
- [ ] Planned CRUD endpoints for Department Personnel (HOD/Tech mapping).
- [ ] Planned CRUD endpoints for Service Types & Request Types.
- [ ] Planned CRUD endpoints for Request Type -> Technician Mappings.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [12_Service_Requests.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/12_Service_Requests.md)**
