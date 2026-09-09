# Step 23 — End-to-End System Testing & Workflow Verification

## 1. Objective

Execute complete end-to-end (E2E) integration test scenarios verifying full ticket lifecycles from creation through approval, auto-assignment, technician resolution, and requester closure/reopening.

---

## 2. Why This Step Is Required

End-to-end system testing verifies that all backend modules (Auth, Masters, Requests, Approvals, Assignment Router, Replies, Attachments, Notifications, Assets, Reports) work together seamlessly with the React UI.

---

## 3. Prerequisites

* Step 22 (Role-Based Testing) completed.

---

## 4. What Needs To Be Done

Execute 3 complete E2E system workflows:

### Workflow 1: Standard IT Support Ticket (No Approval Required)
1. **Requestor (`requestor@gmail.com`)**: Log in, navigate to New Request, submit a "Computer Issue" ticket (`Title: "Laptop screen blue error"`, `Priority: "High"`).
2. **Backend**: Verify request number `SR-2026-XXXX` generated, status set to `Open`/`Assigned`, auto-assignment engine assigns technician (`Ronak`), notification sent to technician.
3. **Technician (`tech@gmail.com`)**: Log in, see ticket in My Tickets, update status to `In Progress`, post discussion reply (`"Diagnosing driver conflict"`), upload diagnostic log file.
4. **Backend**: Verify reply saved, file saved in `wwwroot/uploads/`, attachment record created, timeline entry logged, notification sent to Requestor.
5. **Technician**: Mark ticket `Resolved` with resolution note.
6. **Requestor**: Log in, see ticket resolved, click Close Ticket. Verify status becomes `Closed`.

### Workflow 2: Software License Ticket (HOD Approval Required)
1. **Requestor (`neha.gupta@company.com`)**: Submit a "Software Request" ticket (`Title: "Figma Pro License"`).
2. **Backend**: Detect `RequiresApproval == true`. Set ticket status to `Pending Approval`, insert `Approval` record (`Status = "Pending"`), notify IT HOD (`divya.nair@company.com`).
3. **HOD (`divya.nair@company.com`)**: Log in, see pending approval card in Approvals page, click Approve with remarks (`"Approved for Q3 product squad"`).
4. **Backend**: Update `Approval` status to `Approved`, set ticket status to `Assigned`, trigger auto-assignment router to assign technician (`Anita Desai`).
5. **Technician (`anita.desai@company.com`)**: Complete work, set ticket `Resolved`.

### Workflow 3: Rejection & Reopen Edge Cases
1. **Requestor**: Submit a high-cost hardware request.
2. **HOD**: Reject ticket with remarks (`"Budget ceiling reached for this quarter"`).
3. **Backend**: Verify ticket status set to `Rejected`, approval remarks saved, notification sent to Requestor.
4. **Requestor**: Open a resolved ticket and click Reopen Ticket with note (`"Screen flickering again after update"`).
5. **Backend**: Verify ticket status reset to `In Progress`, technician notified.

---

## 5. Project-Specific Requirements

* Verify that all database transactions roll back cleanly if an exception occurs during multi-step operations.

---

## 6. Important Decisions

* **Database Reset between E2E Runs**: Use test database instances or transaction rollbacks during automated integration runs.

---

## 7. Dependencies

* **Upstream**: Step 22 (Role-Based Testing).
* **Downstream**: Step 24 (Completion Checklist).

---

## 8. Expected Result

After completing this step:

✓ All 3 major ticket lifecycle workflows pass seamlessly.
✓ Approval routing and auto-assignment engine perform accurately.
✓ File attachments, timeline logs, and alerts operate without errors.

---

## 9. Verification Checklist

- [ ] Tested Workflow 1 (Standard ticket creation -> auto-assign -> resolve -> close).
- [ ] Tested Workflow 2 (Software ticket -> HOD approval -> auto-assign -> resolve).
- [ ] Tested Workflow 3 (HOD rejection with remarks & Requester ticket reopen).
- [ ] Verified physical file saving in `wwwroot/uploads/`.
- [ ] Confirmed zero unhandled exceptions in API logs.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [24_Backend_Completion_Checklist.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/24_Backend_Completion_Checklist.md)**
