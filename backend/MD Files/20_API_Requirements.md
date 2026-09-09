# Step 20 — API Requirements & Specifications Master List

## 1. Objective

Provide the consolidated master REST API specification covering all 9 endpoint groups, HTTP methods, route parameters, authorization requirements, and payload contracts.

---

## 2. Why This Step Is Required

A unified API specification document serves as the master contract between backend controller implementation and frontend integration.

---

## 3. Prerequisites

* Steps 09 through 19 completed.

---

## 4. What Needs To Be Done

Review and validate the complete REST API contract:

### 1. Authentication (`/api/v1/auth`)
* `POST /login` (Anonymous): Authenticate credentials, return JWT & user object.
* `POST /signup` (Anonymous): Register user.
* `POST /forgot-password` (Anonymous): Request password reset.
* `POST /reset-password` (Anonymous): Reset password.
* `GET /me` (Authorized): Get current user claims.

### 2. Users (`/api/v1/users`)
* `GET /` (Admin/HOD): Get paginated users.
* `GET /{id}` (Authorized): Get user details.
* `POST /` (Admin): Create user.
* `PUT /{id}` (Admin/User): Update user profile/role/department.
* `DELETE /{id}` (Admin): Soft delete user.
* `PUT /{id}/change-password` (Authorized): Change user password.

### 3. Service Requests (`/api/v1/requests`)
* `GET /` (Authorized): Get paginated requests (Scoped by role).
* `GET /{id}` (Authorized): Get ticket details with replies & timeline.
* `POST /` (Authorized): Create request.
* `PUT /{id}/status` (Technician/Admin): Update ticket status.
* `PUT /{id}/assign` (HOD/Admin): Reassign technician.
* `PUT /{id}/cancel` (Requestor): Cancel pending ticket.
* `PUT /{id}/reopen` (Requestor): Reopen resolved/closed ticket.

### 4. Approvals (`/api/v1/approvals`)
* `GET /pending` (HOD/Admin): Get pending approval tickets for department.
* `GET /history` (HOD/Admin): Get approval history.
* `POST /{id}/decide` (HOD/Admin): Approve or reject ticket with remarks.

### 5. Discussion & Files (`/api/v1/requests/{id}/...`)
* `GET /{id}/replies` (Authorized): Get comments.
* `POST /{id}/replies` (Authorized): Post comment.
* `GET /{id}/timeline` (Authorized): Get audit timeline.
* `POST /attachments/upload` (Authorized): Upload file to `wwwroot/uploads`.

### 6. Asset Management (`/api/v1/assets`)
* `GET /` (Authorized): Get paginated assets.
* `POST /` (Admin/Technician): Create asset.
* `PUT /{id}` (Admin/Technician): Update asset.
* `DELETE /{id}` (Admin/Technician): Soft delete asset.
* `PUT /{id}/assign` (Admin/Technician): Assign asset to employee.

### 7. Configuration Masters (`/api/v1/masters/...`)
* CRUD for `statuses`, `departments`, `personnel`, `service-types`, `request-types`, `mappings` (Read: Authorized; Write: Admin).

### 8. Notifications (`/api/v1/notifications`)
* `GET /` (Authorized): Get inbox alerts.
* `GET /unread-count` (Authorized): Unread badge count.
* `PUT /{id}/read` (Authorized): Mark single alert read.
* `PUT /read-all` (Authorized): Mark all alerts read.

### 9. Dashboard & Reports (`/api/v1/dashboard`, `/api/v1/reports`)
* `GET /dashboard/summary` (Authorized): Role-specific summary stats.
* `GET /reports/trends`, `/reports/departments`, `/reports/sla`, `/reports/export` (Admin/HOD).

---

## 5. Project-Specific Requirements

* **Base URL**: `https://localhost:7124/api/v1`
* **Response Envelope**: Standardized envelope containing `success` (bool), `message` (string), `data` (object/array), and `pagination` metadata.

---

## 6. Important Decisions

* **HTTP Verbs**: Standardized on `GET` (read), `POST` (create/action), `PUT` (update/status), `DELETE` (soft-delete).

---

## 7. Dependencies

* **Upstream**: Steps 09 through 19.
* **Downstream**: Step 21 (Frontend Integration), Step 22 (Testing).

---

## 8. Expected Result

After completing this step:

✓ Complete REST API specifications across 9 groups are finalized.
✓ Authorization policies per endpoint are established.
✓ Standard response format is defined.

---

## 9. Verification Checklist

- [ ] Validated endpoints across all 9 API groups.
- [ ] Confirmed standard response envelope structure.
- [ ] Verified `[Authorize(Roles = "...")]` attributes on sensitive routes.
- [ ] Planned pagination parameters (`pageNumber`, `pageSize`).
- [ ] Confirmed 1-to-1 matching with frontend features.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [21_Frontend_API_Integration.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/21_Frontend_API_Integration.md)**
