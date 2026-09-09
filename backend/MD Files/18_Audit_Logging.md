# Step 18 — Audit Logging Module Planning

## 1. Objective

Plan system-level write-only activity tracking (`dbo.AuditLogs`), administrative action logging, IP tracking, and audit query APIs.

---

## 2. Why This Step Is Required

System audit logging records administrative changes (user role modifications, department creation, master data changes, asset assignments, deletion events), ensuring security compliance and system traceability.

---

## 3. Prerequisites

* Step 10 (Users & Settings) and Step 11 (Masters) completed.

---

## 4. What Needs To Be Done

1. Plan `AuditLogService` in `ServiceDesk.Application`.
2. Plan Audit Event Triggers:
   * **User Actions**: User created, role changed, department changed, user deactivated.
   * **Master Actions**: Status created/edited, department created/edited, personnel mapping updated.
   * **Asset Actions**: Asset created, assigned to user, soft-deleted.
   * **Security Actions**: Failed login attempt, password reset.
3. Plan Audit Endpoint (`/api/v1/audit-logs`):
   * `GET /`: Get paginated write-only audit logs (Admin only, Filters: `actorUserId`, `targetType`, `action`, `startDate`, `endDate`).

---

## 5. Project-Specific Requirements

* **Write-Only Integrity**: `dbo.AuditLogs` records are insert-only. No `UPDATE` or `DELETE` operations are permitted on audit log tables.
* **Frontend Alignment**: Powers User Profile Activity Log tab (`_shell.profile.jsx`).

---

## 6. Important Decisions

* **IP Address Tracking**: Capture client IP address (`HttpContext.Connection.RemoteIpAddress`) on administrative write operations to record the IP context.

---

## 7. Dependencies

* **Upstream**: Step 10 (Users), Step 11 (Masters), Step 16 (Assets).
* **Downstream**: Step 19 (Dashboard & Reports).

---

## 8. Expected Result

After completing this step:

✓ Write-only audit logging engine is planned.
✓ Audit trigger events for sensitive admin actions are specified.
✓ Admin audit query API is established.

---

## 9. Verification Checklist

- [ ] Planned write-only insertion rule for `dbo.AuditLogs`.
- [ ] Configured IP address and actor user ID recording.
- [ ] Planned audit logs query endpoint with target filtering.
- [ ] Restricted audit API access to `Admin`.
- [ ] Confirmed alignment with profile activity log UI.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [19_Dashboard_and_Reports.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/19_Dashboard_and_Reports.md)**
