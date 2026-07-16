# Validation & Business Rules Blueprint
## Service Request Management System

This document outlines the detailed validation rules, business logic, workflow constraints, and security checks for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Validation Strategy

To ensure data integrity and security, validation is enforced across four distinct layers:

```
[React Client] ──> [API DTO Validation] ──> [Business Service Rules] ──> [Database Constraints]
```

1.  **Client-Side Validation**:
    *   *Purpose*: Provides immediate feedback to the user and reduces unnecessary network traffic.
    *   *Rule*: Executed in the React UI using browser APIs and local form states (e.g., checking that required fields are filled out and minimum character lengths are met before form submission).
2.  **Server-Side DTO Validation**:
    *   *Purpose*: Validates incoming HTTP request payloads.
    *   *Rule*: Implemented using **FluentValidation** in the Application layer. Prevents processing malformed payloads and returns a standardized `400 Bad Request` validation response.
3.  **Business Logic Validation**:
    *   *Purpose*: Enforces functional business rules and workflow constraints.
    *   *Rule*: Executed in the Service layer (e.g., checking that a technician belongs to the correct department before assigning them a ticket, or verifying that a ticket is not marked as completed before work is started).
4.  **Database Constraint Validation**:
    *   *Purpose*: Enforces data integrity at the database level.
    *   *Rule*: Enforced by SQL Server database constraints (e.g., primary/foreign keys, unique constraints on emails, and check constraints on priority levels).

---

## 2. Entity-Wise Validation Rules

### 2.1 Users Entity (`dbo.Users`)

| Field Name | Type | Required | Range / Length | Allowed Characters | Default Value | Unique | Validation / Error Message | Business Notes |
| :--- | :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| **FullName** | `NVARCHAR` | Yes | `2` - `100` | Letters, spaces, hyphens | None | No | "Full name must be between 2 and 100 characters." | Sanitized of HTML tags. |
| **Email** | `NVARCHAR` | Yes | `5` - `256` | Alphanumeric, `@`, `.`, `_` | None | Yes | "Please enter a valid corporate email address." | Checked against a whitelist of company domains. |
| **PasswordHash**| `NVARCHAR` | Yes | `60` - `256` | Cryptographic hash | None | No | "Password validation failed." | Generated using BCrypt. |
| **RoleId** | `INT` | Yes | Valid FK | Digits | None | No | "Invalid security role selected." | Must reference a valid role in `dbo.Roles`. |
| **DepartmentId**| `INT` | No | Valid FK | Digits | `NULL` | No | "Invalid department selected." | Null for general admin accounts. |
| **Phone** | `NVARCHAR` | No | `10` - `20` | Digits, `+`, spaces, `-` | `NULL` | No | "Invalid phone format." | Checked against regex: `^\+?[0-9\s\-]{10,20}$`. |
| **Status** | `VARCHAR` | Yes | `Active`/`Inactive` | Letters | `'Active'` | No | "Status must be Active or Inactive." | Checked by `CK_Users_Status` in the DB. |
| **JoinedDate** | `DATE` | Yes | Current or past | Dates | `GETDATE()` | No | "Joined date cannot be in the future." | Logged on onboarding. |

---

### 2.2 ServiceRequests Entity (`dbo.ServiceRequests`)

| Field Name | Type | Required | Range / Length | Allowed Characters | Default Value | Unique | Validation / Error Message | Business Notes |
| :--- | :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| **RequestNumber**| `VARCHAR` | Yes | `14` | Alphanumeric, hyphens | None | Yes | "Invalid request number format." | Format: `SR-YYYY-XXXX`. |
| **Title** | `NVARCHAR` | Yes | `5` - `150` | Alphanumeric, spaces, punctuation | None | No | "Title must be between 5 and 150 characters." | Sanitized to prevent XSS. |
| **Description** | `NVARCHAR` | Yes | Min `20` | Alphanumeric, spaces, punctuation | None | No | "Please provide a description of at least 20 characters." | Sanitized to prevent XSS. |
| **ServiceTypeId**| `INT` | Yes | Valid FK | Digits | None | No | "Invalid Service Type selected." | Mapped to parent service category. |
| **RequestTypeId**| `INT` | Yes | Valid FK | Digits | None | No | "Invalid Request Type selected." | Mapped to sub-category. |
| **DepartmentId** | `INT` | Yes | Valid FK | Digits | None | No | "Invalid department selected." | Target department handling the ticket. |
| **RequesterUserId**| `INT` | Yes | Valid FK | Digits | None | No | "Invalid requester user." | Mapped to the user raising the ticket. |
| **AssigneeUserId**| `INT` | No | Valid FK | Digits | `NULL` | No | "Invalid technician assigned." | Mapped to the technician handling the ticket. |
| **StatusId** | `INT` | Yes | Valid FK | Digits | `1` (Open) | No | "Invalid status selected." | Defaults to `1` (Open/Pending). |
| **Priority** | `VARCHAR` | Yes | `Critical`/`High`/`Medium`/`Low` | Letters | `'Medium'` | No | "Invalid priority selected." | Managed by enum constraints. |

---

## 3. API Input Validation Rules

### 3.1 POST /api/v1/auth/login
*   **Required Fields**: `email` (string), `password` (string).
*   **Optional Fields**: `rememberMe` (boolean, defaults to `false`).
*   **Invalid Request Scenarios**:
    *   Missing credentials: Returns `400 Bad Request` containing validation errors.
    *   Incorrect credentials or inactive user account: Returns `401 Unauthorized`.

### 3.2 POST /api/v1/requests
*   **Required Fields**: `title` (string, length 5-150), `description` (string, minLength 20), `serviceTypeId` (int), `requestTypeId` (int), `departmentId` (int).
*   **Optional Fields**: `priority` (string, defaults to `'Medium'`), `attachmentIds` (array of integers).
*   **Invalid Request Scenarios**:
    *   Description under 20 characters: Returns `400 Bad Request`.
    *   Invalid category IDs: Returns `400 Bad Request` (referenced IDs must exist in lookup tables).

### 3.3 POST /api/v1/requests/{id}/approvals
*   **Required Fields**: `decision` (string, either `'Approved'` or `'Rejected'`).
*   **Optional Fields**: `remarks` (string, max 1000), `assigneeUserId` (int).
*   **Invalid Request Scenarios**:
    *   Decision is `'Rejected'` but `remarks` is empty: Returns `400 Bad Request`.
    *   The user is not an HOD or Admin: Returns `403 Forbidden`.

---

## 4. Core Business Rules

### 4.1 Request Creation Rules
*   **SLA Assignment**: The system calculates response limits based on priority:
    *   `Critical`: Response in 1 hour.
    *   `High`: Response in 4 hours.
    *   `Medium`: Response in 1 business day.
    *   `Low`: Response in 3 business days.
*   **Approval Trigger**: If the `RequestType` is marked as approval-required (e.g. Software, Hardware, Access Requests), the ticket is created with a status of `Pending Approval` (StatusId = 3) and a record is added to the approvals table. It cannot be assigned to a technician until the HOD approves it.

### 4.2 Automated Assignment Rules
*   If a request does not require approval and a matching rule exists in the auto-assignment mappings, the system assigns the technician and sets the ticket status to `Assigned`.
*   If no auto-assignment rule exists, the ticket is created in `Pending` status and must be manually assigned by an HOD or Admin.

### 4.3 HOD Approval Rules
*   Only the HOD of the *target department* handling the request (or an Admin) can approve or reject the request.
*   *Rejections* require remarks and set the ticket status to `Cancelled` or `Rejected`.
*   *Approvals* update the ticket status to `Assigned` (if a technician is selected) or `Pending`.

### 4.4 Status & Progress Rules
*   Only the assigned technician, HOD of the handling department, or an Admin can mark a ticket as `In Progress`, `Resolved`, or `Completed`.
*   A technician must change the status to `In Progress` before they can mark it as `Completed` or `Resolved`.

---

## 5. Workflow Rules

### 5.1 User Registration Workflow
```
[User Form] ──> [Verify corporate email domain] ──> [Save User & Settings] ──> [Send Verification Email]
```

### 5.2 Service Request Creation Workflow
```
[Request Form] ──> [Check if Approval Required] ──(Yes)──> [Status: Pending Approval] ──> [HOD Alert]
                       │
                      (No)
                       │
                       v
[Check Auto-Assignment Mappings] ──(Match)──> [Status: Assigned] ──> [Technician Alert]
                       │
                    (No Match)
                       │
                       v
            [Status: Pending] ──> [Department HOD Alert]
```

---

## 6. State Transition Rules

The table below defines the allowed status transitions for service requests:

| Current Status | Target Status | Authorized Roles | Preconditions |
| :--- | :--- | :--- | :--- |
| **Pending Approval**| `Assigned` | HOD, Admin | Decision set to 'Approved' and technician assigned. |
| **Pending Approval**| `Pending` | HOD, Admin | Decision set to 'Approved' without technician assignment. |
| **Pending Approval**| `Cancelled` | HOD, Admin | Decision set to 'Rejected' (Remarks required). |
| **Pending** | `Assigned` | HOD, Admin | Technician assigned. |
| **Assigned** | `In Progress` | Assigned Technician | Technician clicks "Start Work". |
| **In Progress** | `Completed`/`Resolved`| Assigned Technician | Technician adds resolution notes. |
| **Completed** | `Closed` | Requester | User confirms issue resolution. |
| **Completed** | `Assigned`/`Pending` | Requester | User reopens ticket due to unresolved issue. |

---

## 7. Duplicate Prevention Rules

*   **Request Token Double-Submit**: Frontend requests must include a unique transaction ID (`X-Transaction-Id`) generated on page mount. The API checks the cache for this ID and rejects duplicate submissions within a 30-second window to prevent users from submitting forms multiple times.
*   **Database Constraints**: Unique indexes on `Email`, `EmployeeId`, and `AssetTag` prevent duplicate entity records.

---

## 8. File Upload Validation

To secure file uploads, the backend enforces the following validation rules:

1.  **Allowed File Types**: Restricts uploads to allowed extensions (e.g. `png`, `jpg`, `jpeg`, `pdf`, `txt`, `docx`, `xlsx`). Blocks executable files (e.g. `exe`, `dll`, `msi`, `js`, `bat`).
2.  **Maximum File Size**: Limits uploads to 10MB per file.
3.  **Naming Convention**: Renames files to unique GUIDs (e.g. `e3d0f412-9c32.pdf`) and stores them in cloud storage.
4.  **Virus Scan**: Scans uploaded files using an anti-virus service (e.g. ClamAV integration) before saving them to storage.

---

## 9. Date and Time Rules

*   **Time Zone**: The API stores all date and time records in **Coordinated Universal Time (UTC)**. The client UI formats dates to the user's local timezone.
*   **Audit Dates**: `CreatedAt`, `UpdatedAt`, and `DeletedAt` are populated using database timestamps (`SYSUTCDATETIME()`) rather than client-provided values.
*   **Date Checks**:
    *   Birth dates and purchase dates must be in the past (`Date <= Today`).
    *   Warranty expiry dates must be in the future (`WarrantyDate >= PurchaseDate`).

---

## 10. Search Validation Rules

*   **Minimum Query Length**: Requires search queries to be at least 3 characters long.
*   **Maximum Query Length**: Limits search queries to a maximum of 100 characters to prevent database load spikes.
*   **Sanitization**: Strips special characters (e.g. `;`, `--`, `/*`) to prevent SQL injection attempts in raw queries.

---

## 11. Pagination & Query Rules

*   **Default Page Size**: Defaults to 10 records per page.
*   **Maximum Page Size**: Limits page sizes to a maximum of 100 records to prevent performance issues.
*   **Sorting**: Restricts sort columns to whitelisted fields (e.g. `RequestNumber`, `CreatedAt`, `Priority`, `Status`).
*   **Filtering**: Restricts filters to active database lookup IDs (e.g., status, department, priority).

---

## 12. Security Validation Rules

*   **Input Sanitization**: Encodes HTML content to prevent Cross-Site Scripting (XSS) attacks.
*   **SQL Injection Prevention**: Uses Entity Framework Core's parameterized queries to prevent SQL injection.
*   **Password Policies**: Enforces password strength rules (at least 8 characters, with uppercase, lowercase, numbers, and special characters).
*   **JWT Verification**: Validates token signatures, issuers, audiences, and expiration times on every request.

---

## 13. Error Message Standards

Validation errors return a standard JSON format to help the frontend display error messages next to their respective form fields:

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": {
    "title": [
      "The title field is required.",
      "Title must be between 5 and 150 characters."
    ],
    "description": [
      "Description is required."
    ]
  },
  "code": "VALIDATION_ERROR",
  "timestamp": "2026-07-16T14:17:52Z"
}
```

---

## 14. Audit Logging Rules

The system logs key operational and security events to the database audit log:

*   **Change Audit Trails**: Tracks who changed the data, when, and stores a JSON payload of the changes.
*   **Target Scopes**: Logs changes to tickets, user registrations, role configurations, and asset assignments.

---

## 15. Notification Triggers

The system triggers in-app and push notifications for the following events:

*   `Ticket Assignment`: Notifies a technician when they are assigned a ticket.
*   `Approval Required`: Notifies a department HOD when a request requires their approval.
*   `Status Update`: Notifies the requester when a technician updates the status of their ticket.
*   `Comment Posted`: Notifies the other party when a comment is added to a ticket.

---

## 16. Email Dispatch Rules

The system sends notification emails for the following high-priority events:

*   `User Signup`: Sends a verification email containing a link when a new account is registered.
*   `Password Reset`: Sends a link containing a password reset token when requested.
*   `Critical SLA Warnings`: Sends email alerts to HODs and assigned technicians when a critical ticket is near its SLA response limit.

---

## 17. Edge Case Configurations

*   **Technician Deactivation**: When a technician account is deactivated, all of their pending tickets are set to `Unassigned` and their status is reset to `Pending`. The department HOD is alerted to reassign the tickets.
*   **Duplicate Mappings**: If multiple auto-assignment rules match a request, the ticket is assigned to the technician with the fewest active tickets.
*   **Department Changes**: If a user is moved to a new department, their active requests remain under their old department's workflow to preserve historical record integrity.

---

## 18. Future Validation Improvements

*   **IP Whitelisting**: Restrict administrative portal access to corporate VPN IP ranges.
*   **Automated Virus Scans**: Integrate cloud virus scanning (e.g. Windows Defender, VirusTotal API) for uploads.
*   **SSO Mapping Rules**: Implement mapping rules to auto-provision user accounts from Microsoft Entra ID.

---

## 19. Backend Validation Checklist

Use this checklist to verify validation rules before deploying:

- `[ ]` **Length Controls**: Enforced minimum and maximum character limits on all text inputs.
- `[ ]` **Unique Indices**: Created unique database indexes for email and asset tag fields.
- `[ ]` **Token Expiry**: Configured access token expiration to 15 minutes and refresh tokens to 7 days.
- `[ ]` **File Security**: Configured file type extensions checks and size limits (max 10MB).
- `[ ]` **Centralized Error Format**: Verified that all validation and middleware errors return a consistent error message payload.
- `[ ]` **Audit Trails**: Configured database logs to record change audit payloads for all update transactions.
- `[ ]` **CORS Check**: Configured the CORS whitelist to restrict API access to authorized frontend domains.
- `[ ]` **Password Hash Verification**: Verified that passwords are encrypted using BCrypt before database save.
- `[ ]` **UTC Timestamps**: Verified that all dates and times are stored in UTC format.
- `[ ]` **SLA Limits**: Configured SLA thresholds based on ticket priorities.
