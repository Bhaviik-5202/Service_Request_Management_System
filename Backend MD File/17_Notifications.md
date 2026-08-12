# Step 17 — Notifications & Alerts Module Planning

## 1. Objective

Plan in-app user notifications, alert triggers, inbox retrieval APIs, unread count tracking, and mark-as-read operations.

---

## 2. Why This Step Is Required

Real-time user alerts notify Requesters when their tickets are updated/resolved, alert HODs when approvals are pending, notify Technicians when tickets are assigned, and alert Users on SLA warnings.

---

## 3. Prerequisites

* Step 12 (Service Requests), Step 13 (Approvals), and Step 14 (Assignment) completed.

---

## 4. What Needs To Be Done

1. Plan Notification Triggers:
   * **Ticket Creation**: Notify HOD if approval needed; notify assigned technician if auto-assigned.
   * **Approval Decision**: Notify Requester on approval or rejection.
   * **Technician Assignment**: Notify technician when ticket is assigned/reassigned.
   * **Status Change**: Notify Requester when ticket status changes to `In Progress`, `Resolved`, `Closed`.
   * **Discussion Reply**: Notify ticket author or technician when a new comment is posted.
2. Plan Notification Endpoints (`/api/v1/notifications`):
   * `GET /`: Get paginated inbox notifications for current user (Filters: `type`, `isRead`).
   * `GET /unread-count`: Get total unread count for navbar badge.
   * `PUT /{id}/read`: Mark a single notification as read (`IsRead = 1`).
   * `PUT /read-all`: Mark all notifications for current user as read.

---

## 5. Project-Specific Requirements

* **Notification Categories**: Enum `NotificationType` (`request`, `approval`, `asset`, `system`).
* **Frontend Alignment**: Powers top navbar bell icon badge and `_shell.notifications.jsx` page.

---

## 6. Important Decisions

* **Transactional Notification Insertion**: Insert notification records into `dbo.Notifications` inside the same DB transaction as ticket updates to ensure alerts are never lost.

---

## 7. Dependencies

* **Upstream**: Step 12 (Service Requests), Step 13 (Approvals), Step 14 (Assignment).
* **Downstream**: Step 19 (Dashboard), Step 21 (Frontend Integration).

---

## 8. Expected Result

After completing this step:

✓ Automatic notification trigger mechanisms are planned.
✓ Notification inbox APIs (`GET`, `PUT /read`, `PUT /read-all`) are planned.
✓ Unread counter endpoint is established.

---

## 9. Verification Checklist

- [ ] Planned automatic alert insertion on ticket status changes.
- [ ] Planned automatic alert insertion on HOD approval requests.
- [ ] Planned `GET /unread-count` endpoint.
- [ ] Planned `PUT /read-all` batch update endpoint.
- [ ] Confirmed alignment with `_shell.notifications.jsx`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [18_Audit_Logging.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/18_Audit_Logging.md)**
