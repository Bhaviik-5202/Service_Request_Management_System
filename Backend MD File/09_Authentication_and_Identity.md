# Step 09 — Authentication & Identity System Planning

## 1. Objective

Plan JWT authentication, user registration, BCrypt password hashing, JWT payload claims schema, refresh token flow, and authentication endpoints in `ServiceDesk.Application` and `ServiceDesk.API`.

---

## 2. Why This Step Is Required

Secure token-based authentication enables stateless API communication with the React frontend, allowing users to log in, receive a secure JWT token, and send it in the `Authorization` header (`Bearer <token>`).

---

## 3. Prerequisites

* Step 08 (Database Migration & Seed Data) completed.
* Seed users populated in database with BCrypt password hashes.

---

## 4. What Needs To Be Done

1. Plan `AuthService` in `ServiceDesk.Application`.
2. Plan JWT Token Generation rules:
   * **Algorithm**: HMAC SHA256 (`HS256`).
   * **Secret Key**: Minimum 256-bit key configured in `appsettings.json` (`JwtSettings:Secret`).
   * **Access Token Expiration**: 15 minutes.
   * **Refresh Token Expiration**: 7 days.
3. Plan Claims Payload:
   * `sub` (UserId)
   * `email` (User Email)
   * `role` (Enum `UserRole` string: `Admin`, `HOD`, `Technician`, `Requestor`)
   * `dept_id` (DepartmentId)
   * `emp_id` (EmployeeId)
4. Plan Authentication Endpoints (`/api/v1/auth`):
   * `POST /login`: Validate email/password, return access token & user object.
   * `POST /signup`: Register user with default `Requestor` role.
   * `POST /forgot-password`: Generate reset token simulation.
   * `POST /reset-password`: Reset user password using token.
   * `GET /me`: Return active authenticated user claims & profile.

---

## 5. Project-Specific Requirements

* **Login Response Payload**:
  Must return `accessToken`, `tokenType: "Bearer"`, `expiresIn`, and user object containing `userId`, `fullName`, `email`, `role`, and `department`.
* **Password Hashing**: Use `BCrypt.Net-Next` with work factor >= 11.

---

## 6. Important Decisions

* **Role Claims**: Embed the primary `role` claim directly into the JWT token payload. This enables instant role-based access checks in ASP.NET Core controllers via `[Authorize(Roles = "Admin,HOD")]`.

---

## 7. Dependencies

* **Upstream**: Step 08 (Migration & Seed).
* **Downstream**: Step 10 (Users & Settings), Step 11 (Masters), Step 12 (Service Requests).

---

## 8. Expected Result

After completing this step:

✓ JWT authentication engine is planned.
✓ Password hashing via BCrypt is planned.
✓ Authentication endpoints (`/login`, `/signup`, `/forgot-password`, `/reset-password`, `/me`) are planned.

---

## 9. Verification Checklist

- [ ] Planned JWT secret key and 15-minute token lifetime.
- [ ] Included `role`, `email`, `sub`, and `dept_id` in token claims.
- [ ] Planned BCrypt password validation.
- [ ] Planned 5 auth endpoints under `/api/v1/auth`.
- [ ] Confirmed alignment with React `lib/auth.jsx`.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [10_Users_and_Settings_Module.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/10_Users_and_Settings_Module.md)**
