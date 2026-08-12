# Step 14 — Assignment & Technician Auto-Routing Module Planning

## 1. Objective

Plan the automated technician assignment routing engine, technician workload resolution, and manual ticket reassignment APIs.

---

## 2. Why This Step Is Required

Automated assignment routes newly approved/submitted tickets to designated departmental technicians without requiring manual dispatching for routine tickets.

---

## 3. Prerequisites

* Step 12 (Service Requests) and Step 13 (Approvals) completed.

---

## 4. What Needs To Be Done

1. Plan Auto-Assignment Engine Logic:
   * When a ticket enters `Open` / `Assigned` status:
     * Query `dbo.RequestTypeTechnicianMappings` for the ticket's `RequestTypeId`.
     * If an active mapping exists:
       * Retrieve designated `DepartmentPersonnel`.
       * Set `ServiceRequest.AssigneeUserId` to the mapped technician's `UserId`.
       * Log timeline entry: `"Assigned to technician [Technician Name]"`.
       * Create inbox notification for assigned technician.
     * If no mapping exists:
       * Leave `AssigneeUserId` as `NULL`.
       * Keep status as `Open` / `Pending Assignment`.
2. Plan Reassignment Endpoints (`/api/v1/requests/{id}/assign`):
   * `PUT /{id}/assign`: Reassign ticket to another technician (`AssigneeUserId`). Restricted to HODs and Admins.

---

## 5. Project-Specific Requirements

* **Workload Fallback**: If a mapped technician is marked `Inactive`, the auto-assignment engine assigns the ticket to another active technician in the same department with the fewest active tickets.
* **Frontend Alignment**: Reassignment modal supported on ticket detail page (`_shell.requests.$requestId.index.jsx`).

---

## 6. Important Decisions

* **Technician Department Boundary**: A technician can only be assigned to tickets belonging to their home department (verified via `DepartmentPersonnel`).

---

## 7. Dependencies

* **Upstream**: Step 12 (Service Requests), Step 13 (Approvals).
* **Downstream**: Step 15 (Replies/Attachments), Step 17 (Notifications).

---

## 8. Expected Result

After completing this step:

✓ Auto-assignment routing engine logic is planned.
✓ Manual ticket reassignment API is planned.
✓ Technician department boundary validation is established.

---

## 9. Verification Checklist

- [ ] Planned auto-routing query using `RequestTypeTechnicianMappings`.
- [ ] Planned automated `AssigneeUserId` binding.
- [ ] Planned timeline audit entry generation on assignment.
- [ ] Restricted `PUT /{id}/assign` endpoint to HOD and Admin.
- [ ] Validated technician department boundary check.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [15_Replies_Timeline_and_Attachments.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/15_Replies_Timeline_and_Attachments.md)**
