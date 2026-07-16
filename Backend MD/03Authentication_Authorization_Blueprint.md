# Authentication & Authorization Blueprint
## Service Request Management System

This document outlines the detailed architectural plan for implementing secure **Authentication** and **Authorization** in the Service Request Management System backend using **ASP.NET Core Web API**. It aligns with the security contexts, roles, and workflows required by the frontend application.

---

## 1. Authentication Overview

### Authentication Strategy
The system implements a **Stateless Token-Based Authentication** model. Upon submitting valid credentials, the client receives a cryptographically signed JSON Web Token (JWT) representing the user session. All subsequent requests must attach this token in the `Authorization` header.

```
+----------+                  +--------------+                  +--------------+
|  Client  | --Credentials--> |   API Auth   | --Verify Credentials--> |   Database   |
|  (React) | <---JWT Token--- |  Controller  | <----User Entity---- |  (SQL Server)|
+----------+                  +--------------+                  +--------------+
```

### Why JWT is Suitable
1.  **Statelessness**: The backend does not maintain active sessions in memory, allowing the Web API to scale horizontally across multiple instances or containers.
2.  **Decoupled Frontend**: The frontend is a single-page app (SPA) hosted independently. JWTs simplify API consumption across domains.
3.  **Encapsulated Context**: Claims inside the token body (e.g., role, department, employee ID) can be read directly by the API without querying the database for every request.

### Session vs. JWT Comparison

| Metric | Session-Based Authentication | Token-Based (JWT) Authentication |
| :--- | :--- | :--- |
| **Storage Location** | Server memory or DB (Stateful) | Client storage (Stateless) |
| **Scalability** | Harder (requires sticky sessions or Redis) | High (validated via public/private keys) |
| **Cross-Domain** | Complex CORS and cookie setup | Easy (header based: `Bearer <token>`) |
| **Revocation** | Immediate (destroy session in DB/Cache) | Complex (requires token blacklist or short lifetimes) |
| **Mobile Integration**| Harder (cookie handling constraints) | Native (standard authorization header) |

---

## 2. Authorization Overview

### Role-Based Access Control (RBAC)
Users are assigned exactly one primary Role (Admin, HOD, Technician, Requestor). High-level route access is mapped by checking the user's role claim:
*   *Example*: `[Authorize(Roles = "Admin,HOD")]` blocks other roles from calling master configuration APIs.

### Permission-Based Access Control
To support dynamic custom settings, the system maps roles to fine-grained permission claims stored in `dbo.RolePermissions`.
*   *Example*: A user with the role of `Technician` is granted claims like `requests.edit` and `assets.view` but not `users.manage`. The API middleware parses these permissions and validates them on controller actions (e.g., using policy requirements: `[HasPermission("requests.edit")]`).

### Recommended Approach
A **Hybrid RBAC + Permission-Based** design is recommended:
1.  Use **Roles** for broad-scope UI routing constraints and high-level endpoint categorization.
2.  Use **Permissions** inside Web API controllers to validate transactional capabilities (e.g., allowing a technician to edit *only* the status field of a request).

---

## 3. User Roles

The following four user roles are defined in the frontend:

### 3.1 Admin
*   **Description**: System Administrator.
*   **Responsibilities**: Manages user accounts, department structures, master lookup configurations, and auto-assignment mapping rules.
*   **Accessible Modules**: Users Management, Configuration Masters, Assets, Reports, Global Settings.

### 3.2 HOD (Head of Department)
*   **Description**: Department Manager.
*   **Responsibilities**: Approves or rejects incoming hardware, software, and access requests routed to their department, and manually redirects assignments.
*   **Accessible Modules**: Approval Management, Department Requests, Reports, Settings.

### 3.3 Technician
*   **Description**: Departmental Resolver/Staff.
*   **Responsibilities**: Picks up tickets, updates statuses, leaves comments, and records resolution notes.
*   **Accessible Modules**: Assigned Requests, Assets Inventory, Settings.

### 3.4 Requestor
*   **Description**: General Employee.
*   **Responsibilities**: Submits service requests, views ticket progress, leaves feedback, and tracks assets currently assigned to them.
*   **Accessible Modules**: New Request, My Requests, My Assets, Settings, Profile.

---

## 4. Permissions Matrix

The table below outlines authorization rules mapped to role categories:

| Role | Create Request | View Requests | Update Requests | Delete Requests | Approve Decisions | Assign Techs | Export Reports | Manage Users | Manage Settings | Manage Roles |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Admin** | Yes | Yes (All) | Yes | Yes (Soft) | Yes | Yes | Yes | Yes | Yes | Yes |
| **HOD** | Yes | Yes (Dept) | Yes (Dept) | No | Yes | Yes | Yes | No | Yes | No |
| **Technician**| No | Yes (Assigned) | Yes (Assigned) | No | No | No | No | No | Yes | No |
| **Requestor** | Yes | Yes (Own) | Yes (Own)* | No | No | No | No | No | Yes | No |

> *Note*: Requesters can only update request details while the request is in `Pending` or `Open` status. Once a technician is assigned or work starts, editing is locked.

---

## 5. JWT Token Design

### Access Token
*   **Purpose**: Validates authentication status on API requests.
*   **Lifetime**: 15 minutes.
*   **Signature Algorithm**: HMAC SHA256 (`HS256`).

### Refresh Token
*   **Purpose**: Requests a new Access Token without prompting for credentials.
*   **Lifetime**: 7 days.
*   **Format**: High-entropy cryptographically secure random string (e.g., cryptographically generated GUID or Base64 string).
*   **Storage**: Stored in a database table associated with the user, and sent to the client in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie.

### Token Claims (Payload Schema)
The JWT access token payload contains key metadata:
```json
{
  "iss": "https://api.servicedesk.company.com",
  "aud": "https://servicedesk.company.com",
  "sub": "2",
  "email": "tech@gmail.com",
  "role": "Technician",
  "dept_code": "IT",
  "emp_id": "EMP-0002",
  "nbf": 1784210400,
  "exp": 1784211300,
  "iat": 1784210400
}
```

---

## 6. Login Flow

The sequence diagram below shows the step-by-step login flow:

```
User (UI)                  Frontend (SPA)                Web API (ASP.NET Core)            Database
   |                             |                                  |                          |
   |---- Enters credentials ---->|                                  |                          |
   |                             |------- POST /auth/login -------->|                          |
   |                             |                                  |---- Query User by email->|
   |                             |                                  |<--- PasswordHash & Role -|
   |                             |                                  |                          |
   |                             |                                  |-- Verify Hash ---------> |
   |                             |                                  |-- Save Refresh Token --> |
   |                             |<-- JWT + Set-Cookie (Refresh)----|                          |
   |                             |                                  |                          |
   |<-- Renders Dashboard -------|                                  |                          |
```

1.  **Credentials Entry**: User enters email and password.
2.  **Submit Request**: Frontend sends a POST payload to `/api/v1/auth/login`.
3.  **User Verification**: API queries `dbo.Users` by email. If the user is active, verifies the password hash using `BCrypt`.
4.  **Token Generation**:
    *   Generates a JWT access token containing user claims.
    *   Generates a refresh token and saves it in the database.
5.  **Deliver Payload**: The API writes the refresh token to a secure cookie, and returns the JWT access token in the JSON body.

---

## 7. Logout Flow
1.  **Request**: Frontend sends `POST /api/v1/auth/logout` with the authorization header and cookie.
2.  **Revocation**: The API queries the user's refresh token record and deletes or marks it expired in the database.
3.  **Cookie Cleanup**: The API returns a response header resetting the cookie expiration:
    `Set-Cookie: RefreshToken=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; HttpOnly; Secure; SameSite=Strict`

---

## 8. Refresh Token Flow
1.  **Trigger**: Access token is near expiry.
2.  **Request**: Frontend sends `POST /api/v1/auth/refresh`. The browser automatically includes the HTTP-only cookie.
3.  **Verification**:
    *   The API reads the cookie value and checks it against the database.
    *   Checks that the refresh token is not expired and has not been revoked.
4.  **Rotation (Best Practice)**:
    *   Generates a new access token and a new refresh token.
    *   Deletes the old refresh token and saves the new one.
5.  **Response**: Sends the new refresh token in a cookie and the new access token in the response body.

---

## 9. Forgot Password Flow
1.  **Request**: User enters email on `/forgot-password` page.
2.  **Token Generation**: The API checks if the user exists. If yes, generates a temporary reset token (e.g., GUID).
3.  **Token Save**: The API hashes the reset token and saves it in the user record with an expiration time (e.g., +2 hours).
4.  **Dispatch**: The API sends a reset link containing the token to the user's email.

---

## 10. Reset Password Flow
1.  **Request**: User clicks the email link, enters a new password, and submits `POST /api/v1/auth/reset-password`.
2.  **Verification**: The API searches for the user, checks that the token is valid, hashes the new password, and updates the user record.
3.  **Cleanup**: Invalidates the reset token and revokes all active refresh tokens for the user to terminate existing sessions.

---

## 11. Change Password Flow
1.  **Request**: Authenticated user submits `PUT /api/v1/profile/change-password`.
2.  **Verification**:
    *   The API fetches the user by the authenticated `sub` claim.
    *   Verifies the current password against the stored hash.
3.  **Update**: Hashes the new password and updates the database record.

---

## 12. Email Verification Flow
1.  **Trigger**: User signs up.
2.  **Token Generation**: The API generates an email verification token, saves it to the database, and sends a verification link to the user's email.
3.  **Action**: User clicks the link. The API validates the token, sets the user's email status to verified, and clears the token.

---

## 13. Account Lockout Strategy

To mitigate brute-force attacks, the authentication controller implements an account lockout strategy:

*   **Trigger**: 5 consecutive failed login attempts within 10 minutes.
*   **Action**: Sets `LockoutEnd` timestamp to `CurrentUTC + 15 minutes` and resets failed attempts count.
*   **Resolution**: Subsequent login requests are rejected with a lockout error message until the lockout period expires.
*   **Manual Unlock**: Administrators can manually reset the lockout status via `/users` management tools.

---

## 14. Password Policy

The backend enforces password strength rules on signup, password resets, and updates:

1.  **Length Constraints**: Minimum 8 characters, maximum 128 characters.
2.  **Complexity Rules**: Must contain at least:
    *   1 uppercase letter (`A-Z`)
    *   1 lowercase letter (`a-z`)
    *   1 digit (`0-9`)
    *   1 special character (e.g., `@`, `#`, `$`, `%`)
3.  **Password Reuse History**: Users cannot reuse any of their last 3 passwords.
4.  **Expiration Policy**: Passwords must be updated every 90 days.

---

## 15. Security Headers

Configure these HTTP response headers in the API middleware:

*   **CORS (Cross-Origin Resource Sharing)**: Restricts API access to authorized frontend domains.
    `Access-Control-Allow-Origin: https://servicedesk.company.com`
    `Access-Control-Allow-Credentials: true`
*   **HSTS (HTTP Strict Transport Security)**: Enforces SSL connections.
    `Strict-Transport-Security: max-age=63072000; includeSubDomains; preload`
*   **Content Security Policy (CSP)**: Restricts script loading sources.
    `Content-Security-Policy: default-src 'none'; sandbox; frame-ancestors 'none';`
*   **XSS Protection**:
    `X-Content-Type-Options: nosniff`
    `X-Frame-Options: DENY`

---

## 16. API Authorization Strategy

The table below maps the authorization levels required for key API resource endpoints:

| Endpoint Path | Method | Minimum Access Level |
| :--- | :---: | :--- |
| `/api/v1/auth/login` | `POST` | Anonymous |
| `/api/v1/auth/signup` | `POST` | Anonymous |
| `/api/v1/auth/forgot-password` | `POST` | Anonymous |
| `/api/v1/profile` | `GET` | Authenticated (Any Role) |
| `/api/v1/requests` | `POST` | Authenticated (Any Role) |
| `/api/v1/requests/{id}/assign` | `POST` | HOD / Admin |
| `/api/v1/requests/{id}/approvals`| `POST` | HOD / Admin |
| `/api/v1/admin/personnel` | `*` | Admin |
| `/api/v1/admin/mappings` | `*` | Admin |

---

## 17. Claims Design

Configure the following claims in access tokens:

*   `sub`: Unique User ID (`UserId`).
*   `email`: User email address.
*   `role`: User role (e.g., `'Requestor'`).
*   `dept_code`: Mapped department code.
*   `emp_id`: Corporate employee ID.

---

## 18. Permission Evaluation Flow

To enforce dynamic permission checks, implement an authorization policy handler:

```
[Incoming Request] -> [Authorize(Policy="requests.edit")]
                            |
                     [Policy Handler] -> [Check User Token Claims]
                            |
                   (Read Permissions Array)
                            |
                     [Access Decision]
```

1.  **Request**: The client calls an endpoint decorated with a permission policy, e.g., `[Authorize(Policy = "requests.edit")]`.
2.  **Handler**: The policy handler checks the user's role and retrieves its permissions.
3.  **Evaluation**: Checks if the required permission claim (e.g., `requests.edit`) is in the user's permissions list. If yes, access is granted.

---

## 19. Token Storage Comparison

| Target Location | XSS Vulnerability Risk | CSRF Vulnerability Risk | Recommended Usage |
| :--- | :--- | :--- | :--- |
| **LocalStorage** | **High** (Accessible by malicious JS) | None | Do not use for authentication tokens. |
| **SessionStorage**| **High** (Accessible by malicious JS) | None | Do not use for authentication tokens. |
| **HttpOnly Cookie**| **None** (Inaccessible to JS) | **High** (Browser sends it automatically)| **Highly Recommended** for Refresh Tokens. |

**Recommendation**: Store the **Access Token** in memory (React application state) and the **Refresh Token** in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie.

---

## 20. Security Best Practices

*   **Password Hashing**: Passwords must be hashed using a strong hashing algorithm, such as **Argon2id** or **BCrypt**, before being saved to the database.
*   **Enforce HTTPS**: Reject all insecure HTTP requests at the gateway or API middleware level.
*   **Rate Limiting**: Apply rate limiting to auth endpoints (e.g., maximum 10 requests per minute per IP address on `/auth/login`).
*   **Audit Logging**: Log all critical security events, such as password changes, failed login attempts, and permission updates, to the audit log table.
*   **Input Validation**: Validate all incoming requests using validation attributes (e.g. `[Required]`, `[EmailAddress]`).
*   **SQL Injection Prevention**: Enforce the use of parameterized queries (e.g., via Entity Framework Core LINQ queries).

---

## 21. Error Handling

Return standard error schemas for security-related failures:

### Authentication Failure (401 Unauthorized)
```json
{
  "success": false,
  "message": "Authentication failed. Invalid token or credentials.",
  "errors": null,
  "code": "UNAUTHORIZED",
  "timestamp": "2026-07-16T14:16:08Z"
}
```

### Authorization Failure (403 Forbidden)
```json
{
  "success": false,
  "message": "Access denied. Insufficient permissions.",
  "errors": null,
  "code": "FORBIDDEN",
  "timestamp": "2026-07-16T14:16:08Z"
}
```

---

## 22. Future Improvements

The authentication design can be extended in future phases to support:
1.  **OAuth2/SSO (Single Sign-On)**: Integration with Microsoft Entra ID or Google Workspace for enterprise authentication.
2.  **Two-Factor Authentication (2FA)**: Requiring a time-based one-time password (TOTP) from an authenticator app (e.g., Google Authenticator).
3.  **Biometric Login**: Integration with Windows Hello or Apple FaceID on mobile clients.

---

## 23. Authentication Testing Checklist

Verify your authentication and authorization implementation using the manual test scenarios below:

- `[ ]` **TC-01: Login Success**: Submit valid credentials and verify the API returns a 200 response with a JWT token and sets the refresh cookie.
- `[ ]` **TC-02: Login Locked**: Submit invalid credentials 5 times and verify subsequent attempts return a 400 response with an account lockout message.
- `[ ]` **TC-03: Invalid Signature**: Attempt to access a protected endpoint using a modified JWT signature and verify the request is blocked with `401 Unauthorized`.
- `[ ]` **TC-04: Expired Token**: Attempt to access a protected endpoint using an expired access token and verify the request is blocked with `401 Unauthorized`.
- `[ ]` **TC-05: RBAC Enforcement**: Log in as a Requestor and verify that calling `/api/v1/admin/personnel` returns `403 Forbidden`.
- `[ ]` **TC-06: Token Rotation**: Call `/api/v1/auth/refresh` and verify the API returns a new access token and updates the refresh cookie.
- `[ ]` **TC-07: Logout Invalidation**: Log out and verify the refresh token is deleted in the database and subsequent refresh calls are rejected.
- `[ ]` **TC-08: Reopen Bounds**: Verify that a Requestor cannot reopen a ticket unless they are the original creator of that request.
- `[ ]` **TC-09: Password Strength**: Attempt to sign up with a simple password (e.g., `12345`) and verify the request fails validation rules.
- `[ ]` **TC-10: Expiration Exceeded**: Verify that attempting to perform token refresh after the 7-day expiration limit has elapsed returns a `401 Unauthorized` response.
