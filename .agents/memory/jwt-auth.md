---
name: JWT auth implementation
description: How JWT is configured, where the key comes from, and the legacy password migration flow
---

## Signing key
`SESSION_SECRET` env var is read first; falls back to `appsettings.json["Jwt:Key"]`.
Both `Program.cs` and `JwtService.cs` use the same resolution logic.

## Login response shape
`POST /api/Users/Login` returns `{ token, userId, employeeId, fullName, email, roleId, roleName, ... }`.
The `token` field is stripped by `auth.jsx` before storing the user object.

## Legacy password migration
Accounts with plain-text passwords are automatically migrated to BCrypt on the first successful login.
Detection: if `PasswordHash` does NOT start with `$2`, compare plain-text; if match, rehash and save.

## Frontend storage
`auth.jsx` stores `{ userId, role, signedIn, user, token }` under key `"servicedesk.auth"` in localStorage (remember=true) or sessionStorage (remember=false).
`api.js` reads this key to inject `Authorization: Bearer <token>` on every request.

## 401 handling
`api.js` request() auto-clears storage and redirects to `/login` on any 401 response.

**Why:** Eliminates the need for refresh-token logic while keeping the UX clean on session expiry.
