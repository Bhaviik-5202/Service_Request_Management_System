# Step 13 — Request Approval Workflow Module Planning

## 1. Objective

Plan the HOD approval workflow engine, approval requirement checks, pending approvals queue, HOD decision APIs (`Approve` / `Reject`), and post-rejection ticket handling.

---

## 2. Why This Step Is Required

Certain high-priority or cost-incurring service request types (e.g. Software Requests, Hardware Requests, Access Requests) require sign-off from the target Department's HOD before work can begin.

---

## 3. Prerequisites

* Step 12 (Service Requests) completed.

---

## 4. What Needs To Be Done

1. Plan Approval Trigger Check on Ticket Creation:
   * When a ticket is submitted, query `dbo.RequestTypes` for the selected `RequestTypeId`.
   * If `RequiresApproval == true`:
     * Set `ServiceRequest.StatusId` to `Pending Approval`.
     * Create an `Approval` record linked to the ticket (`Status = "Pending"`, `SubmittedAt = SYSUTCDATETIME()`).
     * Trigger inbox notification for the target department's HOD.
   * If `RequiresApproval == false`:
     * Set `ServiceRequest.StatusId` to `Open` / `Assigned` and proceed directly to auto-assignment.
2. Plan Approval Endpoints (`/api/v1/approvals`):
   * `GET /pending`: Get pending approval tickets assigned to current user's HOD department.
   * `GET /history`: Get past approval decisions made by current user.
   * `POST /{id}/decide`: Process HOD decision (`Status = "Approved"` or `"Rejected"`, `Remarks`).
3. Plan Post-Decision Actions:
   * If **Approved**: Update `Approval` record (`DecidedByUserId`, `DecidedAt`), update ticket status to `Open` / `Assigned`, trigger auto-assignment router.
   * If **Rejected**: Update `Approval` record with mandatory HOD `Remarks`, update ticket status to `Rejected`, notify Requester.

---

## 5. Project-Specific Requirements

* **Access Control**: Approving or rejecting tickets is restricted to users with `HOD` or `Admin` roles (`[Authorize(Roles = "HOD,Admin")]`).
* **Remarks Validation**: Rejection decisions require a non-empty `Remarks` description explaining the reason.

---

## 6. Important Decisions

* **Target HOD Determination**: The approval is routed to the HOD of the **target handling department** selected for the ticket (`ServiceRequest.DepartmentId`).

---

## 7. Dependencies

* **Upstream**: Step 12 (Service Requests).
* **Downstream**: Step 14 (Assignment), Step 17 (Notifications).

---

## 8. Expected Result

After completing this step:

✓ Approval requirement check on ticket submission is planned.
✓ Pending approvals queue and decision APIs are planned.
✓ Approval/Rejection transition workflows are established.

---

## 9. Verification Checklist

- [ ] Planned `RequiresApproval` check on ticket submission.
- [ ] Planned `Pending Approval` initial ticket state for software/hardware/access requests.
- [ ] Planned HOD pending queue query filtered by target department.
- [ ] Enforced mandatory `Remarks` validation for rejections.
- [ ] Confirmed alignment with `_shell.approvals.jsx`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [14_Assignment_and_Technician_Module.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/14_Assignment_and_Technician_Module.md)**
