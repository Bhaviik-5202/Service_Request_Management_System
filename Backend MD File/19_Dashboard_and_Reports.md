# Step 19 — Dashboard & Reports Module Planning

## 1. Objective

Plan role-specific dashboard KPI summary statistics APIs, chart aggregation queries (department resolution distribution, priority breakdown, monthly trends), SLA compliance metrics, and report data export APIs.

---

## 2. Why This Step Is Required

The Dashboard and Analytics module powers the main overview dashboard (`_shell.index.jsx`) and analytics page (`_shell.reports.jsx`), delivering role-specific counters, charts, and SLA metrics.

---

## 3. Prerequisites

* Step 12 (Service Requests), Step 13 (Approvals), Step 16 (Assets) completed.

---

## 4. What Needs To Be Done

1. Plan `DashboardService` and `ReportService` in `ServiceDesk.Application`.
2. Plan Dashboard Endpoints (`/api/v1/dashboard`):
   * `GET /summary`: Get role-scoped KPI counters based on authenticated user role:
     * **Admin**: Total Requests, Pending Approvals, Active Users, Total Assets, Priority Breakdown.
     * **Technician**: My Assigned Tickets count, Open tickets, Resolved today count, Avg resolution time.
     * **HOD**: Pending Approvals count, Department Total Requests, Dept SLA %, Active Dept Techs count.
     * **Requestor**: My Open Requests count, In Progress count, Resolved count, My Total Requests.
3. Plan Report Endpoints (`/api/v1/reports`):
   * `GET /trends`: Monthly request creation vs resolution count.
   * `GET /departments`: Request count and resolution rates grouped by department.
   * `GET /sla`: SLA compliance percentages (% of resolved tickets within target SLA hours).
   * `GET /export`: Download aggregated report data (CSV/PDF simulation payload).

---

## 5. Project-Specific Requirements

* **SLA Calculation Target**: Calculate SLA compliance based on ticket priorities:
  * `Critical`: Resolution limit 4 hours.
  * `High`: Resolution limit 24 hours.
  * `Medium`: Resolution limit 48 hours.
  * `Low`: Resolution limit 72 hours.
* **Frontend Alignment**: Powers role-specific cards on `_shell.index.jsx` and charts/tables on `_shell.reports.jsx`.

---

## 6. Important Decisions

* **Database Aggregations**: Use optimized SQL `GROUP BY` and `COUNT()` LINQ queries directly in database repositories to prevent loading raw ticket lists into memory.

---

## 7. Dependencies

* **Upstream**: Step 12 (Service Requests), Step 13 (Approvals), Step 16 (Assets).
* **Downstream**: Step 20 (API Requirements), Step 21 (Frontend Integration).

---

## 8. Expected Result

After completing this step:

✓ Role-specific dashboard KPI summary endpoints are planned.
✓ Analytics chart aggregation queries are planned.
✓ SLA compliance calculation rules are established.

---

## 9. Verification Checklist

- [ ] Planned `GET /dashboard/summary` with 4 custom role payloads.
- [ ] Planned department distribution and priority breakdown queries.
- [ ] Configured SLA compliance calculation thresholds.
- [ ] Planned monthly trend aggregation endpoint.
- [ ] Confirmed alignment with dashboard and reports pages.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [20_API_Requirements.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/20_API_Requirements.md)**
