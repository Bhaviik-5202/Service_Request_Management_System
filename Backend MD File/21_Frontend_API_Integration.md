# Step 21 — Frontend API Integration & Mock Replacement

## 1. Objective

Guide the step-by-step replacement of frontend dummy/mock data (`mock.js` and `localStorage`) with live REST API HTTP requests pointing to the ASP.NET Core Web API backend.

---

## 2. Why This Step Is Required

Once the ASP.NET Core Web API is developed, the React frontend must be wired to real API endpoints to transition from a static prototype to a fully operational full-stack application.

---

## 3. Prerequisites

* Steps 01 through 20 completed.
* ASP.NET Core Web API backend running on `https://localhost:7124`.

---

## 4. What Needs To Be Done

Follow this sequential integration order:

1. **API Client & Auth Header Setup**:
   * Configure `axios` or `fetch` base client pointing to `https://localhost:7124/api/v1`.
   * Add request interceptor attaching `Authorization: Bearer <accessToken>` from `localStorage` / `auth.jsx`.
2. **Auth Integration (`lib/auth.jsx`)**:
   * Replace `mock.js` login/signup/logout helper functions with real API calls to `/api/v1/auth/login`, `/signup`, `/me`.
   * Store returned JWT access token in `localStorage`.
3. **Master Configuration Integration (`_shell.masters.jsx`)**:
   * Replace `statuses`, `departmentsMaster`, `departmentPersons`, `serviceTypesMaster`, `requestTypesMaster`, `requestTypePersonMappings` mock arrays with HTTP GET calls to `/api/v1/masters/...`.
   * Wire Add/Edit/Delete dialogs to backend master POST/PUT/DELETE endpoints.
4. **User Management Integration (`_shell.users.index.jsx`)**:
   * Wire users table and Add/Edit user dialogs to `/api/v1/users`.
5. **Service Requests Integration (`_shell.requests.*`)**:
   * Wire request creation form to `POST /api/v1/requests`.
   * Wire request list table and filters to `GET /api/v1/requests`.
   * Wire request detail view, discussion thread, timeline, attachments to `/api/v1/requests/{id}/...`.
   * Wire Cancel and Reopen buttons to `PUT /cancel` and `PUT /reopen`.
6. **Approvals Integration (`_shell.approvals.jsx`)**:
   * Wire pending/past approvals tabs and Approve/Reject modal to `/api/v1/approvals/...`.
7. **Asset Management Integration (`_shell.assets.index.jsx`)**:
   * Wire asset inventory table and Add/Edit/Assign dialogs to `/api/v1/assets`.
8. **Notifications Integration (`_shell.notifications.jsx`)**:
   * Wire top navbar bell icon unread badge and inbox list to `/api/v1/notifications`.
9. **Dashboard & Reports Integration (`_shell.index.jsx`, `_shell.reports.jsx`)**:
   * Wire dashboard summary counters and chart widgets to `/api/v1/dashboard/summary` and `/api/v1/reports/...`.

---

## 5. Project-Specific Requirements

* **Base Environment Configuration**: Store API base URL in React `.env` file: `VITE_API_BASE_URL=https://localhost:7124/api/v1`.
* **Error Toast Display**: Handle backend RFC 7807 error responses in frontend HTTP interceptor and display friendly user error messages using Sonner toast notices (`toast.error(message)`).

---

## 6. Important Decisions

* **Gradual Component Replacement**: Replace mock functions feature by feature (Auth -> Masters -> Users -> Requests -> Approvals -> Assets -> Dashboard) to allow isolated verification of each module.

---

## 7. Dependencies

* **Upstream**: Step 20 (API Requirements).
* **Downstream**: Step 22 (Role-Based Testing), Step 23 (E2E Testing).

---

## 8. Expected Result

After completing this step:

✓ All dummy/mock data helpers in `mock.js` are replaced with real REST API calls.
✓ Frontend state is driven entirely by SQL Server database records via the Web API.
✓ Authentication and authorization state flow seamlessly between React and ASP.NET Core.

---

## 9. Verification Checklist

- [ ] Configured API base client with Authorization header interceptor.
- [ ] Connected login, signup, and logout to `/api/v1/auth`.
- [ ] Connected 6 Master configuration screens in `masters.jsx`.
- [ ] Connected ticket creation, listing, detail view, replies, and attachments.
- [ ] Connected HOD approvals, assets, notifications, and dashboard stats.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [22_Role_Based_Testing.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/22_Role_Based_Testing.md)**
