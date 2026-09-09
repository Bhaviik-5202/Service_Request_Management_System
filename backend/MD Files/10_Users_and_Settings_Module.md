# Step 10 — Users & Settings Module Planning

## 1. Objective

Plan the user management services, profile management APIs, password modification endpoints, and user settings preference operations.

---

## 2. Why This Step Is Required

The User and Settings module provides administration endpoints for creating, updating, and deactivating user accounts, as well as storing personal UI preferences (theme, 2FA toggle, email notifications).

---

## 3. Prerequisites

* Step 09 (Authentication & Identity) completed.

---

## 4. What Needs To Be Done

1. Plan `UserService` in `ServiceDesk.Application`.
2. Plan User Management Endpoints (`/api/v1/users`):
   * `GET /`: Get paginated users (Filters: `role`, `departmentId`, `status`, `search`).
   * `GET /{id}`: Get detailed user profile by ID.
   * `POST /`: Create user account (Admin only).
   * `PUT /{id}`: Update user profile details, role, department, or phone.
   * `DELETE /{id}`: Soft delete user (`IsDeleted = 1`).
   * `PUT /{id}/status`: Toggle user status (`Active` / `Inactive`).
   * `PUT /{id}/change-password`: Update password.
3. Plan User Settings Endpoints (`/api/v1/settings`):
   * `GET /`: Fetch current user settings (`Theme`, `TwoFactorEnabled`, 5 notification preferences).
   * `PUT /`: Update current user settings preferences.

---

## 5. Project-Specific Requirements

* **User Management Access**: Creating, editing roles/departments, and soft-deleting users is strictly restricted to `Admin` (`[Authorize(Roles = "Admin")]`).
* **Profile Edit**: Requesters, Technicians, and HODs can update their own phone number and password via `/api/v1/users/{id}`.

---

## 6. Important Decisions

* **Cascading Settings Creation**: When a new `User` is created, automatically insert a default `UserSettings` record (`Theme = "light"`, `NotifyRequestUpdates = true`, `NotifyApprovalAlerts = true`, `NotifySLAWarnings = true`).

---

## 7. Dependencies

* **Upstream**: Step 09 (Authentication).
* **Downstream**: Step 11 (Masters Module), Step 12 (Service Requests Module).

---

## 8. Expected Result

After completing this step:

✓ User management endpoints (`/api/v1/users`) are planned.
✓ User settings endpoints (`/api/v1/settings`) are planned.
✓ Automatic settings creation on user signup is established.

---

## 9. Verification Checklist

- [ ] Planned paginated user search with role and status filtering.
- [ ] Restricted Admin endpoints with `[Authorize(Roles = "Admin")]`.
- [ ] Planned soft delete operation for users.
- [ ] Planned 5 email notification preferences in UserSettings.
- [ ] Confirmed alignment with `_shell.users.index.jsx` and `_shell.profile.jsx`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [11_Master_Data_Module.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/11_Master_Data_Module.md)**
