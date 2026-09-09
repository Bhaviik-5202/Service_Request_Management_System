# Step 22 — Role-Based Access Control (RBAC) Testing

## 1. Objective

Perform systematic role-based authorization verification across all 4 user roles (`Admin`, `HOD`, `Technician`, `Requestor`) to verify data access boundaries and endpoint protection.

---

## 2. Why This Step Is Required

Role-based testing verifies that security constraints enforced in ASP.NET Core controllers (`[Authorize(Roles = "...")]`) prevent unauthorized users from accessing administrative screens or inspecting other users' private data.

---

## 3. Prerequisites

* Step 21 (Frontend Integration) completed.

---

## 4. What Needs To Be Done

Execute role-based security test matrices for each role:

### 1. Admin Role Verification (`admin@gmail.com`)
* [ ] Can view global dashboard metrics, charts, and all system requests.
* [ ] Can access User Management (`_shell.users.index.jsx`) and create/edit/delete users.
* [ ] Can access Configuration Masters (`_shell.masters.jsx`) and modify all 6 master tabs.
* [ ] Can create, edit, soft-delete, and assign assets (`_shell.assets.index.jsx`).
* [ ] Can override HOD approval decisions if necessary.

### 2. HOD Role Verification (`hod@gmail.com`)
* [ ] Can view Pending Approvals queue for their target department (`_shell.approvals.jsx`).
* [ ] Can Approve or Reject requests with mandatory remarks.
* [ ] Can view all requests belonging to their handling department.
* [ ] Can manually reassign department technicians (`PUT /requests/{id}/assign`).
* [ ] **Blocked** from accessing Configuration Masters or creating system users (`403 Forbidden`).

### 3. Technician Role Verification (`tech@gmail.com`)
* [ ] Can view tickets assigned to them or unassigned in their department.
* [ ] Can change assigned ticket status to `In Progress`, `Resolved`, `Closed`.
* [ ] Can post discussion replies and upload resolution attachments.
* [ ] Can view asset inventory.
* [ ] **Blocked** from accessing HOD approvals or User Management (`403 Forbidden`).

### 4. Requestor Role Verification (`requestor@gmail.com`)
* [ ] Can create new service requests (`_shell.requests.new.jsx`).
* [ ] Can view only tickets created by them (`RequesterUserId == CurrentUser`).
* [ ] Can cancel own pending requests (`PUT /cancel`).
* [ ] Can reopen own resolved/closed requests (`PUT /reopen`).
* [ ] Can view assets assigned to them.
* [ ] **Blocked** from viewing other users' tickets, HOD approvals, or Master configuration tabs (`403 Forbidden`).

---

## 5. Project-Specific Requirements

* **HTTP 403 Verification**: Attempting unauthorized operations must return HTTP status `403 Forbidden`.
* **Data Scoping Verification**: LINQ queries must correctly restrict returned ticket lists based on the token's `role` and `sub` claims.

---

## 6. Important Decisions

* **Automated Security Tests**: Run integration test suites using test JWT tokens issued for each role to verify endpoint authorization attributes.

---

## 7. Dependencies

* **Upstream**: Step 21 (Frontend Integration).
* **Downstream**: Step 23 (End to End Testing), Step 24 (Completion Checklist).

---

## 8. Expected Result

After completing this step:

✓ All 4 user role security boundaries are verified.
✓ Unauthorized operations are confirmed blocked with `403 Forbidden`.
✓ Scoped LINQ queries prevent data leakage between users.

---

## 9. Verification Checklist

- [ ] Tested Admin full access permissions.
- [ ] Tested HOD department approval & reassignment scope.
- [ ] Tested Technician ticket resolution boundaries.
- [ ] Tested Requestor own-ticket scoping, cancellation, and reopening.
- [ ] Confirmed HTTP `403 Forbidden` on unauthorized API attempts.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [23_End_to_End_Testing.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/23_End_to_End_Testing.md)**
