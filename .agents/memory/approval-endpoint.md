---
name: Approval endpoint
description: The correct endpoint for approval decisions and what was removed
---

## Correct endpoint
`PUT /api/Approvals/DecideApproval/{id}/decide`

Payload: `{ status: "Approved"|"Rejected", decidedByUserId: number, remarks: string|null }`

## What was removed
`updateApproval()` was removed from `api.js` — it called a non-existent `PUT /api/Approvals/PutApproval/{id}` route.
All callers (`_shell.approvals.jsx`, `_shell.index.jsx`) now use `decideApproval()`.

**Why:** The backend `ApprovalsController` never had a `PutApproval` action. The `DecideApproval` action at `/decide` is the only mutation endpoint for approvals.
