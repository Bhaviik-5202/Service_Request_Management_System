---
name: Auth storage key and shape
description: Where auth session data is stored and what shape it takes
---

## Storage key
`"servicedesk.auth"` — used in both `localStorage` and `sessionStorage`.
`api.js` reads this key to extract the JWT token for Bearer auth headers.

## Shape
```json
{
  "userId": 1,
  "role": "Admin",
  "signedIn": true,
  "token": "eyJ...",
  "user": {
    "id": "1", "userId": 1, "employeeId": "EMP-101",
    "name": "...", "fullName": "...", "email": "...",
    "role": "Admin", "roleId": 1,
    "department": "IT", "departmentId": 1,
    "phone": "", "status": "Active", "avatar": "AD", "joined": "2024-01-01"
  }
}
```

## Rules
- `remember=true` (default) → localStorage; sessionStorage cleared
- `remember=false` → sessionStorage; localStorage cleared
- On 401 from API: both storages cleared, redirect to `/login`
- `setRole` dev utility was removed from `auth.jsx` and `TopBar.jsx` entirely

**Why:** Keeping token in the same storage object as the user avoids a second storage lookup per request.
