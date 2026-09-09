# Step 12 — Service Requests Core Module Planning

## 1. Objective

Plan the service request creation engine, sequential ticket number generation (`SR-YYYY-XXXX`), multi-filtered search endpoints, ticket details query, status modification APIs, and ticket cancellation/reopen logic.

---

## 2. Why This Step Is Required

The Service Requests module is the core functionality of the system. It handles ticket creation by employees, enforces role-based ticket visibility, and updates status transitions.

---

## 3. Prerequisites

* Step 11 (Configuration Masters) completed.

---

## 4. What Needs To Be Done

1. Plan `RequestService` in `ServiceDesk.Application`.
2. Plan Ticket Number Generation algorithm (`SR-YYYY-XXXX`, e.g., `SR-2026-1041`).
3. Plan Request Endpoints (`/api/v1/requests`):
   * `GET /`: Get paginated requests with search, sorting, and filters (`statusId`, `priority`, `serviceTypeId`, `departmentId`). Apply role-based data scoping:
     * `Admin`: Sees all tickets.
     * `HOD`: Sees tickets routed to their handling department.
     * `Technician`: Sees tickets assigned to them or unassigned in their department.
     * `Requestor`: Sees only tickets created by them (`RequesterUserId == CurrentUser`).
   * `GET /{id}`: Get full ticket details (Includes replies, timeline history, attachments).
   * `POST /`: Create ticket (`Title`, `Description`, `ServiceTypeId`, `RequestTypeId`, `DepartmentId`, `Priority`).
   * `PUT /{id}/status`: Update ticket status (Technician/Admin).
   * `PUT /{id}/cancel`: Cancel pending ticket (Requester only, permitted if status is `Pending` or `Open`).
   * `PUT /{id}/reopen`: Reopen resolved/closed ticket (Requester only, resets status to `In Progress`).

---

## 5. Project-Specific Requirements

* **Validation Rules**:
  * `Title`: Required, length between 5 and 150 chars.
  * `Description`: Required, minimum 20 characters.
  * `Priority`: Required Enum (`Critical`, `High`, `Medium`, `Low`).
* **Frontend Alignment**: Maps directly to `_shell.requests.index.jsx`, `_shell.requests.new.jsx`, and `_shell.requests.$requestId.index.jsx`.

---

## 6. Important Decisions

* **Automatic Ticket Numbering**: Generate ticket numbers in backend transaction: `SR-` + `CurrentYear` + `-` + `SequentialNumber` (e.g. `SR-2026-1042`).
* **Requester Action Bounds**: Requesters are permitted to cancel their own ticket if work hasn't started, or reopen a ticket if the issue reoccurs after resolution.

---

## 7. Dependencies

* **Upstream**: Step 11 (Masters).
* **Downstream**: Step 13 (Approvals), Step 14 (Assignment), Step 15 (Replies/Attachments).

---

## 8. Expected Result

After completing this step:

✓ Core ticket creation and ticket number generator are planned.
✓ Role-scoped search and filtering APIs are planned.
✓ Cancel and Reopen endpoints are specified.

---

## 9. Verification Checklist

- [ ] Planned `SR-YYYY-XXXX` auto-numbering logic.
- [ ] Configured role-based data visibility rules.
- [ ] Validated minimum 20-character description rule.
- [ ] Planned `PUT /cancel` and `PUT /reopen` endpoints.
- [ ] Confirmed alignment with ticket list and detail pages.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [13_Request_Approval_Workflow.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/13_Request_Approval_Workflow.md)**
