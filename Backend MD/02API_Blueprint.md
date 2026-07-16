# Backend API Blueprint - ASP.NET Core Web API
## Service Request Management System

This document serves as the comprehensive planning blueprint for implementing the RESTful Web API using **ASP.NET Core Web API**. It details every endpoint required by the frontend application, payload schemas, authorization requirements, pagination/sorting standards, business rules, and implementation sequence.

---

## 1. Project Overview

### Purpose of the Backend
The backend provides a secure, transactional, and scalable business logic layer for the Service Request Management System. It manages request workflows, coordinates approvals, executes automated technician assignment routing, tracks inventory assets, alerts users in real-time, and generates department performance analytics.

### Overall API Architecture
The API is built using a clean, controller-based REST architecture. It separates concerns into:
*   **Controller Layer**: Handles HTTP requests, parses routing inputs, and enforces model validation attributes.
*   **Service Layer**: Encapsulates business logic, auto-assignment algorithms, and email dispatch routing.
*   **Repository Layer**: Interacts with SQL Server via Entity Framework Core.
*   **DTO Layer (Data Transfer Objects)**: Sanitizes input/output payloads to keep data contracts decoupled from DB entity structures.

### REST API Conventions
*   **Resource-Oriented URIs**: Nouns represent resources (e.g., `/requests`, `/users`).
*   **Standard HTTP Verbs**:
    *   `GET`: Read resource data (idempotent).
    *   `POST`: Create new resources or execute state-changing actions.
    *   `PUT`: Update existing resources in full.
    *   `DELETE`: Soft-delete resources.
*   **Stateless**: Each request carries authentication state in the header.

### Versioning Strategy
API versioning is managed via URL path parameters to prevent breaking changes:
*   `Base URL`: `https://api.servicedesk.company.com/api/v1` (Local dev: `https://localhost:7124/api/v1`)

### Naming Conventions
*   **URLs**: Lowercase, hyphen-delimited kebab-case (e.g., `/api/v1/activity-logs`).
*   **JSON Fields**: CamelCase for both request and response properties (e.g., `requestNumber`, `statusId`).

---

## Response Standards

### Success Response Standard
All single-resource endpoints return a uniform envelope containing a success flag, message, and target payload:
```json
{
  "success": true,
  "message": "Resource details retrieved successfully.",
  "data": {
    "id": "1",
    "name": "Dell Latitude 7440"
  }
}
```

### Pagination Standard
Endpoints returning list collections return a wrapped JSON array containing pagination metadata:
```json
{
  "success": true,
  "message": "Records fetched successfully.",
  "data": [
    {
      "id": "1",
      "name": "Dell Latitude 7440"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 48
  }
}
```

### Error Response Standard
All operational or validation failures return a standard error schema aligned with RFC 7807 (Problem Details) but simplified for the client UI:
```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": {
    "title": [
      "The title field is required."
    ],
    "description": [
      "Please provide a description of at least 20 characters."
    ]
  },
  "code": "VALIDATION_ERROR",
  "timestamp": "2026-07-16T14:15:13Z"
}
```

---

## 2. Authentication APIs

### 2.1 POST /api/v1/auth/login
*   **API Name**: User Login Authentication
*   **URL**: `/api/v1/auth/login`
*   **HTTP Method**: `POST`
*   **Description**: Authenticates user credentials and returns a JWT access token.
*   **Authentication Required**: No
*   **Required Role(s)**: Anonymous
*   **Request Headers**:
    *   `Content-Type: application/json`
*   **Request Body Schema**:
    ```json
    {
      "email": "requestor@gmail.com",
      "password": "hashed_or_plain_password",
      "rememberMe": true
    }
    ```
*   **Path Parameters**: None
*   **Query Parameters**: None
*   **Validation Rules**:
    *   `email`: Required, valid email address format.
    *   `password`: Required, minimum 6 characters.
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "Login successful.",
      "data": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "tokenType": "Bearer",
        "expiresIn": 3600,
        "user": {
          "userId": 6,
          "fullName": "Priya Sharma",
          "email": "requestor@gmail.com",
          "role": "Requestor",
          "department": "Sales"
        }
      }
    }
    ```
*   **Error Response**:
    ```json
    {
      "success": false,
      "message": "Invalid email or password.",
      "errors": null,
      "code": "UNAUTHORIZED",
      "timestamp": "2026-07-16T14:15:13Z"
    }
    ```
*   **Business Rules**:
    *   Locks account for 15 minutes after 5 consecutive failed login attempts.
    *   Updates `LastLoginAt` timestamp in `dbo.Users` on successful login.
*   **Related Database Tables**: `dbo.Users`, `dbo.Roles`
*   **Expected Status Codes**: 200, 400, 401
*   **Pagination/Sorting/Filtering/Search Support**: No

---

### 2.2 POST /api/v1/auth/signup
*   **API Name**: User Registration Signup
*   **URL**: `/api/v1/auth/signup`
*   **HTTP Method**: `POST`
*   **Description**: Registers a new corporate user accounts.
*   **Authentication Required**: No
*   **Required Role(s)**: Anonymous
*   **Request Headers**:
    *   `Content-Type: application/json`
*   **Request Body Schema**:
    ```json
    {
      "fullName": "Aarav Gupta",
      "email": "aarav.gupta@company.com",
      "password": "SecurePassword123!",
      "phone": "+91 98200 99887",
      "departmentId": 1
    }
    ```
*   **Path/Query Parameters**: None
*   **Validation Rules**:
    *   `fullName`: Required, length between 2 and 100.
    *   `email`: Required, valid corporate domain email.
    *   `password`: Required, >= 6 characters, contains uppercase, number, and special character.
    *   `departmentId`: Required, must correspond to a valid active department.
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "Registration successful. Welcome aboard!",
      "data": {
        "userId": 11,
        "email": "aarav.gupta@company.com",
        "role": "Requestor"
      }
    }
    ```
*   **Error Response**:
    ```json
    {
      "success": false,
      "message": "A user with this email address already exists.",
      "errors": null,
      "code": "DUPLICATE_RESOURCE",
      "timestamp": "2026-07-16T14:15:13Z"
    }
    ```
*   **Business Rules**:
    *   Users registered via signup default to the `Requestor` role.
    *   Automatically creates default records in `dbo.UserSettings` (theme set to 'light').
*   **Related Database Tables**: `dbo.Users`, `dbo.UserSettings`
*   **Expected Status Codes**: 201, 400, 409

---

### 2.3 POST /api/v1/auth/forgot-password
*   **API Name**: Forgot Password Request
*   **URL**: `/api/v1/auth/forgot-password`
*   **HTTP Method**: `POST`
*   **Description**: Generates password reset tokens and dispatches emails.
*   **Authentication Required**: No
*   **Request Body Schema**:
    ```json
    {
      "email": "aarav.gupta@company.com"
    }
    ```
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "If the email is registered, a password reset link has been sent.",
      "data": null
    }
    ```
*   **Error Response**: Standard 400 Validation Error.
*   **Business Rules**:
    *   To prevent account harvesting, return a success message even if the email does not exist.
    *   Reset token expires in 2 hours.
*   **Related Database Tables**: `dbo.Users`
*   **Expected Status Codes**: 200, 400

---

### 2.4 POST /api/v1/auth/reset-password
*   **API Name**: Reset User Password
*   **URL**: `/api/v1/auth/reset-password`
*   **HTTP Method**: `POST`
*   **Description**: Updates user password using reset token.
*   **Authentication Required**: No
*   **Request Body Schema**:
    ```json
    {
      "email": "aarav.gupta@company.com",
      "token": "RST-TOKEN-STRING",
      "newPassword": "NewPassword123!"
    }
    ```
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "Password reset successfully. You can now log in.",
      "data": null
    }
    ```
*   **Error Response**:
    ```json
    {
      "success": false,
      "message": "Invalid or expired token.",
      "errors": null,
      "code": "INVALID_TOKEN",
      "timestamp": "2026-07-16T14:15:13Z"
    }
    ```
*   **Related Database Tables**: `dbo.Users`
*   **Expected Status Codes**: 200, 400

---

### 2.5 POST /api/v1/auth/logout
*   **API Name**: Logout Session
*   **URL**: `/api/v1/auth/logout`
*   **HTTP Method**: `POST`
*   **Description**: Invalidates the user session (revokes refresh token).
*   **Authentication Required**: Yes
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "Logged out successfully.",
      "data": null
    }
    ```
*   **Expected Status Codes**: 200, 401

---

## 3. User APIs

### 3.1 GET /api/v1/users
*   **API Name**: Retrieve Users List
*   **URL**: `/api/v1/users`
*   **HTTP Method**: `GET`
*   **Description**: Fetches users with search, filtering, sorting, and pagination.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD
*   **Query Parameters**:
    *   `pageNumber`: integer (default `1`)
    *   `pageSize`: integer (default `10`)
    *   `search`: string (filters by `FullName` or `Email`)
    *   `role`: string (filters by role name)
    *   `departmentId`: integer
    *   `sortBy`: string (e.g., `'name'`, `'email'`, default `'name'`)
    *   `sortDir`: string (`'asc'` or `'desc'`, default `'asc'`)
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "Users retrieved successfully.",
      "data": [
        {
          "userId": 6,
          "employeeId": "EMP-0006",
          "fullName": "Priya Sharma",
          "email": "requestor@gmail.com",
          "role": "Requestor",
          "department": "Sales",
          "phone": "+91 98200 66778",
          "status": "Active",
          "joinedDate": "2023-02-01"
        }
      ],
      "pagination": {
        "pageNumber": 1,
        "pageSize": 1,
        "totalPages": 10,
        "totalRecords": 10
      }
    }
    ```
*   **Related Database Tables**: `dbo.Users`, `dbo.Roles`, `dbo.Departments`
*   **Expected Status Codes**: 200, 401, 403
*   **Pagination/Sorting/Filtering/Search Support**: Yes (detailed above)

---

### 3.2 GET /api/v1/users/{id}
*   **API Name**: User Profile Details
*   **URL**: `/api/v1/users/{id}`
*   **HTTP Method**: `GET`
*   **Description**: Fetches profile metadata and history metrics for a user.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD
*   **Path Parameters**:
    *   `id`: integer (UserId)
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "User details fetched successfully.",
      "data": {
        "userId": 2,
        "employeeId": "EMP-0002",
        "fullName": "Ronak",
        "email": "tech@gmail.com",
        "role": "Technician",
        "department": "IT",
        "phone": "+91 98200 22334",
        "status": "Active",
        "joinedDate": "2022-01-10",
        "requestsRaised": 4,
        "requestsResolved": 148
      }
    }
    ```
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 3.3 POST /api/v1/users
*   **API Name**: Create User Profile
*   **URL**: `/api/v1/users`
*   **HTTP Method**: `POST`
*   **Description**: Allows administrators to provision new user profiles.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "fullName": "Suresh Kumar",
      "email": "suresh.kumar@company.com",
      "role": "Technician",
      "departmentId": 2,
      "phone": "+91 98200 44556",
      "status": "Active"
    }
    ```
*   **Validation Rules**:
    *   `fullName`, `email`, `role`, `departmentId`: Required.
    *   `status`: Must be 'Active' or 'Inactive'.
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "User provisioned successfully.",
      "data": {
        "userId": 4,
        "employeeId": "EMP-0004"
      }
    }
    ```
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 3.4 PUT /api/v1/users/{id}
*   **API Name**: Update User Profile
*   **URL**: `/api/v1/users/{id}`
*   **HTTP Method**: `PUT`
*   **Description**: Updates user demographic and permissions metadata.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Path Parameters**:
    *   `id`: integer (UserId)
*   **Request Body Schema**: Same as `POST /api/v1/users`
*   **Success Response**: Status 200 indicating success.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 3.5 DELETE /api/v1/users/{id}
*   **API Name**: Soft Delete User
*   **URL**: `/api/v1/users/{id}`
*   **HTTP Method**: `DELETE`
*   **Description**: Deactivates and soft-deletes a user account.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Path Parameters**:
    *   `id`: integer (UserId)
*   **Business Rules**:
    *   Updates `IsDeleted = 1`, status is set to `Inactive`.
    *   Reassigns pending tickets of soft-deleted technicians to `Unassigned`.
*   **Expected Status Codes**: 200, 401, 403, 404

---

## 4. Role APIs

### 4.1 GET /api/v1/roles
*   **API Name**: Get Roles List
*   **URL**: `/api/v1/roles`
*   **HTTP Method**: `GET`
*   **Description**: Returns active roles for dropdown selections.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": [
        { "roleId": 1, "roleName": "Admin", "description": "System Administrator" },
        { "roleId": 2, "roleName": "HOD", "description": "Head of Department" }
      ]
    }
    ```
*   **Expected Status Codes**: 200, 401, 403

---

### 4.2 GET /api/v1/roles/{id}/permissions
*   **API Name**: Get Role Permissions Mappings
*   **URL**: `/api/v1/roles/{id}/permissions`
*   **HTTP Method**: `GET`
*   **Required Role(s)**: Admin
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": {
        "roleId": 3,
        "roleName": "Technician",
        "permissions": ["dashboard.view", "requests.view", "requests.edit", "assets.view"]
      }
    }
    ```
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 4.3 PUT /api/v1/roles/{id}/permissions
*   **API Name**: Update Role Permissions Mappings
*   **URL**: `/api/v1/roles/{id}/permissions`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "permissions": ["dashboard.view", "requests.view"]
    }
    ```
*   **Business Rules**:
    *   Replaces mappings in `dbo.RolePermissions` for this role.
    *   Forces users with this role to re-authenticate on next API call to refresh claims.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

## 5. Permission APIs

### 5.1 GET /api/v1/permissions
*   **API Name**: Get All Claims Permissions
*   **URL**: `/api/v1/permissions`
*   **HTTP Method**: `GET`
*   **Description**: Returns all granular claims supported by the system.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Success Response**: List of permission key records.
*   **Expected Status Codes**: 200, 401, 403

---

## 6. Service Request APIs

### 6.1 GET /api/v1/requests
*   **API Name**: Retrieve Requests List
*   **URL**: `/api/v1/requests`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves requests with criteria filters, search queries, pagination, and sorting.
*   **Authentication Required**: Yes
*   **Query Parameters**:
    *   `pageNumber`, `pageSize`, `sortBy`, `sortDir` (standard inputs)
    *   `search`: string (checks `RequestNumber`, `Title`, `RequesterName`)
    *   `status`: string (filters by status name)
    *   `priority`: string (filters by priority)
    *   `department`: string (filters by department name)
    *   `assignee`: string (filters by technician name)
*   **Business Rules**:
    *   `Requestor` only sees requests where `RequesterEmail` matches their identity.
    *   `Technician` only sees requests where they are assigned.
    *   `HOD` only sees requests targeted to their department.
    *   `Admin` has global view access.
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": [
        {
          "requestId": 1,
          "requestNumber": "SR-2026-1041",
          "title": "Laptop not booting after update",
          "serviceType": "Technical",
          "requestType": "Computer Issue",
          "department": "IT",
          "requester": "Priya Sharma",
          "assignee": "Ronak",
          "status": "In Progress",
          "priority": "Critical",
          "createdAt": "2026-07-06T09:12:00"
        }
      ],
      "pagination": { "pageNumber": 1, "pageSize": 10, "totalPages": 1, "totalRecords": 1 }
    }
    ```
*   **Expected Status Codes**: 200, 401

---

### 6.2 GET /api/v1/requests/{id}
*   **API Name**: Request Details
*   **URL**: `/api/v1/requests/{id}`
*   **HTTP Method**: `GET`
*   **Description**: Returns complete details of a request, including comments, timelines, and files.
*   **Authentication Required**: Yes
*   **Path Parameters**:
    *   `id`: integer (RequestId)
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": {
        "requestId": 1,
        "requestNumber": "SR-2026-1041",
        "title": "Laptop not booting after update",
        "description": "My laptop shows a blue screen after windows update...",
        "serviceType": "Technical",
        "requestType": "Computer Issue",
        "department": "IT",
        "requester": "Priya Sharma",
        "requesterEmail": "requestor@gmail.com",
        "assignee": "Ronak",
        "status": "In Progress",
        "priority": "Critical",
        "createdAt": "2026-07-06T09:12:00",
        "updatedAt": "2026-07-07T14:30:00",
        "replies": [
          {
            "replyId": 1,
            "author": "Ronak",
            "role": "Technician",
            "message": "Running startup repair now.",
            "createdAt": "2026-07-06T10:05:00",
            "statusTransition": "In Progress"
          }
        ],
        "timeline": [
          {
            "timelineId": 1,
            "status": "Pending",
            "changedBy": "Priya Sharma",
            "changedAt": "2026-07-06T09:12:00",
            "note": "Request raised by Priya Sharma"
          }
        ],
        "attachments": [
          {
            "attachmentId": 1,
            "fileName": "screenshot_error.png",
            "fileSizeKB": 245,
            "fileUrl": "https://host/files/screenshot_error.png"
          }
        ]
      }
    }
    ```
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 6.3 POST /api/v1/requests
*   **API Name**: Submit Service Request
*   **URL**: `/api/v1/requests`
*   **HTTP Method**: `POST`
*   **Description**: Creates a new service request ticket.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "title": "AC not cooling in Room B",
      "description": "The AC has stopped cooling and room becomes hot by noon.",
      "serviceTypeId": 2,
      "requestTypeId": 8,
      "departmentId": 2,
      "priority": "High",
      "attachmentIds": [3]
    }
    ```
*   **Validation Rules**:
    *   `title`, `description`, `serviceTypeId`, `requestTypeId`, `departmentId`, `priority`: Required.
    *   `description`: Minimum 20 characters.
*   **Business Rules**:
    *   Checks if the request category is approval-required (e.g., Software, Hardware, Access).
        *   If **Yes**: Sets status to `Pending Approval` (StatusId = 3) and creates a record in `dbo.Approvals`.
        *   If **No**: Evaluates auto-assignment mappings. If a mapping rule exists for the `RequestTypeId`, assigns the technician and sets status to `Assigned`. Otherwise, sets status to `Open/Pending`.
    *   Logs timeline entry and dispatches alerts/notifications.
*   **Success Response**: Returns the generated request ID and tracking request number.
*   **Expected Status Codes**: 201, 400, 401

---

### 6.4 PUT /api/v1/requests/{id}
*   **API Name**: Edit Service Request
*   **URL**: `/api/v1/requests/{id}`
*   **HTTP Method**: `PUT`
*   **Description**: Updates ticket title, description, category, and priority.
*   **Authentication Required**: Yes
*   **Path Parameters**:
    *   `id`: integer (RequestId)
*   **Request Body Schema**: Same as `POST /api/v1/requests`
*   **Business Rules**:
    *   Requesters can only edit their own tickets when they are in `Pending` or `Open` status.
    *   Technicians and Admins can edit tickets at any point.
    *   If edited during work, notifies the assigned technician.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 6.5 DELETE /api/v1/requests/{id}
*   **API Name**: Soft Delete Request
*   **URL**: `/api/v1/requests/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Path/Query Parameters**: `id` path parameter.
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 6.6 POST /api/v1/requests/{id}/assign
*   **API Name**: Assign Request Technician
*   **URL**: `/api/v1/requests/{id}/assign`
*   **HTTP Method**: `POST`
*   **Description**: Assigns or updates the technician assigned to a request.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD, Technician (technicians can only self-assign or reassign within their department)
*   **Request Body Schema**:
    ```json
    {
      "assigneeUserId": 2
    }
    ```
*   **Business Rules**:
    *   If `assigneeUserId` is null, unassigns the ticket and changes status back to `Pending`.
    *   If the ticket was `Pending`, sets status to `Assigned` (or `In Progress` if already started).
    *   Creates a timeline change log.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 6.7 POST /api/v1/requests/{id}/status
*   **API Name**: Transition Request Status
*   **URL**: `/api/v1/requests/{id}/status`
*   **HTTP Method**: `POST`
*   **Description**: Updates ticket status (e.g. In Progress, Completed).
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD, Technician
*   **Request Body Schema**:
    ```json
    {
      "statusId": 4,
      "note": "Replaced HDMI cable."
    }
    ```
*   **Business Rules**:
    *   Validates status transitions. A technician cannot set status back to `Pending Approval` without raising a new approval process.
    *   Automatically creates a timeline record.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 6.8 POST /api/v1/requests/{id}/reopen
*   **API Name**: Reopen Request Ticket
*   **URL**: `/api/v1/requests/{id}/reopen`
*   **HTTP Method**: `POST`
*   **Description**: Allows a user to reopen a resolved or closed ticket.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "reason": "AC started leaking again after technician left."
    }
    ```
*   **Business Rules**:
    *   Resets status to `Assigned` if a technician is still linked, otherwise `Pending`.
    *   Creates a comment and timeline entry detailing the reopening reason.
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 6.9 POST /api/v1/requests/{id}/close
*   **API Name**: Close Request Ticket
*   **URL**: `/api/v1/requests/{id}/close`
*   **HTTP Method**: `POST`
*   **Description**: Requester marks a resolved ticket as closed.
*   **Authentication Required**: Yes
*   **Business Rules**:
    *   Can only close tickets that are currently in `Resolved` or `Completed` status.
    *   Sets status to `Closed`.
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 6.10 POST /api/v1/requests/{id}/replies
*   **API Name**: Post Request Reply
*   **URL**: `/api/v1/requests/{id}/replies`
*   **HTTP Method**: `POST`
*   **Description**: Adds a comment/reply to a request.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "message": "Thanks for the quick response! Testing the setup now.",
      "attachmentIds": [5]
    }
    ```
*   **Business Rules**:
    *   Dispatches notifications to the requester or assigned technician depending on who posted.
*   **Success Response**: Created reply object metadata.
*   **Expected Status Codes**: 201, 400, 401, 404

---

### 6.11 POST /api/v1/requests/{id}/approvals
*   **API Name**: Decides Request Approval
*   **URL**: `/api/v1/requests/{id}/approvals`
*   **HTTP Method**: `POST`
*   **Description**: HOD approves or rejects a request.
*   **Authentication Required**: Yes
*   **Required Role(s)**: HOD, Admin
*   **Request Body Schema**:
    ```json
    {
      "decision": "Approved",
      "remarks": "Approved. Badge photo is ready.",
      "assigneeUserId": 3
    }
    ```
*   **Validation Rules**:
    *   `decision`: Must be 'Approved' or 'Rejected'.
    *   `remarks`: Required if decision is 'Rejected'.
*   **Business Rules**:
    *   If **Approved**: Updates `dbo.Approvals` status to `Approved`. Sets request status to `Assigned` (if `assigneeUserId` provided) or `Pending`.
    *   If **Rejected**: Updates `dbo.Approvals` status to `Rejected`. Sets request status to `Cancelled` or `Closed`.
    *   Logs the decision in the ticket timeline and posts it as a reply comment.
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

## 7. Category APIs

### 7.1 GET /api/v1/categories/service-types
*   **API Name**: Get Service Types
*   **URL**: `/api/v1/categories/service-types`
*   **HTTP Method**: `GET`
*   **Description**: Returns active service types.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

### 7.2 POST /api/v1/categories/service-types
*   **API Name**: Create Service Type
*   **URL**: `/api/v1/categories/service-types`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "name": "Administrative Services",
      "code": "ADMIN",
      "description": "Furniture, onboarding, and facilities."
    }
    ```
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 7.3 PUT /api/v1/categories/service-types/{id}
*   **API Name**: Update Service Type
*   **URL**: `/api/v1/categories/service-types/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 7.4 DELETE /api/v1/categories/service-types/{id}
*   **API Name**: Delete Service Type
*   **URL**: `/api/v1/categories/service-types/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 7.5 GET /api/v1/categories/request-types
*   **API Name**: Get Request Types
*   **URL**: `/api/v1/categories/request-types`
*   **HTTP Method**: `GET`
*   **Description**: Returns active request types, optionally filtered by `serviceTypeId`.
*   **Authentication Required**: Yes
*   **Query Parameters**:
    *   `serviceTypeId`: integer (optional)
*   **Expected Status Codes**: 200, 401

---

### 7.6 POST /api/v1/categories/request-types
*   **API Name**: Create Request Type
*   **URL**: `/api/v1/categories/request-types`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "name": "Figma Software License",
      "serviceTypeId": 1,
      "description": "Software subscription requests."
    }
    ```
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 7.7 PUT /api/v1/categories/request-types/{id}
*   **API Name**: Update Request Type
*   **URL**: `/api/v1/categories/request-types/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 7.8 DELETE /api/v1/categories/request-types/{id}
*   **API Name**: Delete Request Type
*   **URL**: `/api/v1/categories/request-types/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 7.9 GET /api/v1/categories/asset-categories
*   **API Name**: Get Asset Categories
*   **URL**: `/api/v1/categories/asset-categories`
*   **HTTP Method**: `GET`
*   **Description**: Returns asset categories for dropdown filters.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

## 8. Status APIs

### 8.1 GET /api/v1/statuses
*   **API Name**: Get All Statuses
*   **URL**: `/api/v1/statuses`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves statuses, including colors and descriptions.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

### 8.2 POST /api/v1/statuses
*   **API Name**: Create Status
*   **URL**: `/api/v1/statuses`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "statusName": "Escalated",
      "colorCode": "bg-destructive text-destructive-foreground",
      "description": "Critical ticket escalated to management."
    }
    ```
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 8.3 PUT /api/v1/statuses/{id}
*   **API Name**: Update Status
*   **URL**: `/api/v1/statuses/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 8.4 DELETE /api/v1/statuses/{id}
*   **API Name**: Delete Status
*   **URL**: `/api/v1/statuses/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

## 9. Priority APIs

### 9.1 GET /api/v1/priorities
*   **API Name**: Get Static Priorities
*   **URL**: `/api/v1/priorities`
*   **HTTP Method**: `GET`
*   **Description**: Returns static priorities (Critical, High, Medium, Low) and their styling colors.
*   **Authentication Required**: Yes
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": [
        { "name": "Critical", "color": "text-destructive bg-destructive/10" },
        { "name": "High", "color": "text-warning bg-warning/10" },
        { "name": "Medium", "color": "text-primary bg-primary/10" },
        { "name": "Low", "color": "text-muted-foreground bg-muted" }
      ]
    }
    ```
*   **Expected Status Codes**: 200, 401

---

## 10. Dashboard APIs

### 10.1 GET /api/v1/dashboard/stats
*   **API Name**: Get Dashboard Summary Stats
*   **URL**: `/api/v1/dashboard/stats`
*   **HTTP Method**: `GET`
*   **Description**: Returns summary count metrics filtered by the user's role context.
*   **Authentication Required**: Yes
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": {
        "openRequests": 3,
        "inProgressRequests": 4,
        "pendingApprovalRequests": 2,
        "resolvedRequests": 3,
        "closedRequests": 3,
        "totalAssetsAssigned": 2
      }
    }
    ```
*   **Business Rules**:
    *   `Requestor` gets counts of their own raised requests.
    *   `Technician` gets counts of requests assigned to them.
    *   `HOD` gets counts of requests in their department.
    *   `Admin` gets global metrics.
*   **Expected Status Codes**: 200, 401

---

### 10.2 GET /api/v1/dashboard/recent-activity
*   **API Name**: Get Recent Activities Log
*   **URL**: `/api/v1/dashboard/recent-activity`
*   **HTTP Method**: `GET`
*   **Description**: Returns recent audit log entries for the dashboard activity feed.
*   **Authentication Required**: Yes
*   **Success Response**: List of recent activities.
*   **Expected Status Codes**: 200, 401

---

## 11. Attachment APIs

### 11.1 POST /api/v1/attachments/upload
*   **API Name**: Upload File Attachment
*   **URL**: `/api/v1/attachments/upload`
*   **HTTP Method**: `POST`
*   **Description**: Uploads a file and returns its storage URL.
*   **Authentication Required**: Yes
*   **Request Headers**:
    *   `Content-Type: multipart/form-data`
*   **Request Body Schema**: Form-data with a `file` field.
*   **Validation Rules**:
    *   File size: Maximum 10MB.
    *   File type: Allowed extensions (PNG, JPG, JPEG, PDF, TXT, DOCX, XLSX).
*   **Success Response**:
    ```json
    {
      "success": true,
      "message": "File uploaded successfully.",
      "data": {
        "attachmentId": 123,
        "fileName": "screenshot.png",
        "fileSizeKB": 154,
        "fileUrl": "https://storage.company.com/attachments/guid-name.png"
      }
    }
    ```
*   **Business Rules**: Refer to the **File Upload API Standard** section below.
*   **Expected Status Codes**: 201, 400, 401, 415

---

### 11.2 DELETE /api/v1/attachments/{id}
*   **API Name**: Delete Attachment
*   **URL**: `/api/v1/attachments/{id}`
*   **HTTP Method**: `DELETE`
*   **Description**: Removes the database record and deletes the physical file.
*   **Authentication Required**: Yes
*   **Path Parameters**: `id` (integer)
*   **Business Rules**: Only the uploader or an Admin can delete an attachment.
*   **Expected Status Codes**: 200, 401, 403, 404

---

## 12. Notification APIs

### 12.1 GET /api/v1/notifications
*   **API Name**: Get User Notifications
*   **URL**: `/api/v1/notifications`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves notifications for the logged-in user.
*   **Authentication Required**: Yes
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": [
        {
          "notificationId": 1,
          "title": "New request assigned",
          "message": "SR-2026-1041 has been assigned to you.",
          "createdAt": "2026-07-16T14:05:00",
          "isRead": false,
          "notificationType": "request"
        }
      ]
    }
    ```
*   **Expected Status Codes**: 200, 401

---

### 12.2 GET /api/v1/notifications/unread-count
*   **API Name**: Get Unread Notifications Count
*   **URL**: `/api/v1/notifications/unread-count`
*   **HTTP Method**: `GET`
*   **Description**: Returns count of unread notifications.
*   **Authentication Required**: Yes
*   **Success Response**:
    ```json
    {
      "success": true,
      "data": { "unreadCount": 1 }
    }
    ```
*   **Expected Status Codes**: 200, 401

---

### 12.3 POST /api/v1/notifications/{id}/read
*   **API Name**: Mark Notification As Read
*   **URL**: `/api/v1/notifications/{id}/read`
*   **HTTP Method**: `POST`
*   **Description**: Marks a specific notification as read.
*   **Authentication Required**: Yes
*   **Path Parameters**: `id` (integer)
*   **Expected Status Codes**: 200, 401, 404

---

### 12.4 POST /api/v1/notifications/read-all
*   **API Name**: Mark All Notifications Read
*   **URL**: `/api/v1/notifications/read-all`
*   **HTTP Method**: `POST`
*   **Description**: Marks all notifications for the user as read.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

## 13. Activity Log APIs

### 13.1 GET /api/v1/activity-logs
*   **API Name**: Search Activity Logs
*   **URL**: `/api/v1/activity-logs`
*   **HTTP Method**: `GET`
*   **Description**: Allows administrators to view system-wide logs.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Query Parameters**:
    *   `pageNumber`, `pageSize`, `sortBy`, `sortDir` (standard inputs)
    *   `actorUserId`: integer (optional)
    *   `targetType`: string (optional)
    *   `search`: string (optional)
*   **Success Response**: List of log entries.
*   **Expected Status Codes**: 200, 401, 403

---

## 14. Admin APIs

### 14.1 GET /api/v1/admin/personnel
*   **API Name**: Get Department Personnel Mappings
*   **URL**: `/api/v1/admin/personnel`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves department personnel mappings.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Success Response**: List of personnel mapping records.
*   **Expected Status Codes**: 200, 401, 403

---

### 14.2 POST /api/v1/admin/personnel
*   **API Name**: Create Department Personnel Mapping
*   **URL**: `/api/v1/admin/personnel`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "userId": 2,
      "departmentId": 1,
      "isHOD": false,
      "isActive": true
    }
    ```
*   **Validation Rules**:
    *   `userId`, `departmentId`: Required. Must exist and be active.
*   **Success Response**: Mapped ID details.
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 14.3 PUT /api/v1/admin/personnel/{id}
*   **API Name**: Update Department Personnel Mapping
*   **URL**: `/api/v1/admin/personnel/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 14.4 DELETE /api/v1/admin/personnel/{id}
*   **API Name**: Delete Department Personnel Mapping
*   **URL**: `/api/v1/admin/personnel/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 14.5 GET /api/v1/admin/mappings
*   **API Name**: Get Auto-Assignment Rules
*   **URL**: `/api/v1/admin/mappings`
*   **HTTP Method**: `GET`
*   **Description**: Lists technician auto-assignment mappings.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403

---

### 14.6 POST /api/v1/admin/mappings
*   **API Name**: Create Auto-Assignment Rule
*   **URL**: `/api/v1/admin/mappings`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "requestTypeId": 1,
      "departmentPersonnelId": 1
    }
    ```
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 14.7 PUT /api/v1/admin/mappings/{id}
*   **API Name**: Update Auto-Assignment Rule
*   **URL**: `/api/v1/admin/mappings/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 14.8 DELETE /api/v1/admin/mappings/{id}
*   **API Name**: Delete Auto-Assignment Rule
*   **URL**: `/api/v1/admin/mappings/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

### 14.9 GET /api/v1/admin/assets
*   **API Name**: Retrieve Assets List
*   **URL**: `/api/v1/admin/assets`
*   **HTTP Method**: `GET`
*   **Description**: Returns corporate assets list with filtering and search.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, Technician
*   **Query Parameters**:
    *   `pageNumber`, `pageSize`, `search`, `status`
*   **Success Response**: List of assets.
*   **Expected Status Codes**: 200, 401, 403

---

### 14.10 POST /api/v1/admin/assets
*   **API Name**: Create Asset Record
*   **URL**: `/api/v1/admin/assets`
*   **HTTP Method**: `POST`
*   **Required Role(s)**: Admin
*   **Request Body Schema**:
    ```json
    {
      "assetTag": "AST-IT-0205",
      "assetName": "iPad Pro 11 M4",
      "categoryId": 5,
      "serialNumber": "IPD11-44321",
      "assignedToUserId": null,
      "departmentId": 1,
      "status": "Available",
      "purchaseDate": "2026-05-10",
      "warrantyUntil": "2028-05-10",
      "bookValue": 85000.00
    }
    ```
*   **Validation Rules**:
    *   `assetTag`, `assetName`, `categoryId`, `serialNumber`, `purchaseDate`, `warrantyUntil`, `bookValue`: Required.
    *   `warrantyUntil`: Must be equal to or after `purchaseDate`.
*   **Expected Status Codes**: 201, 400, 401, 403, 409

---

### 14.11 PUT /api/v1/admin/assets/{id}
*   **API Name**: Update Asset Record
*   **URL**: `/api/v1/admin/assets/{id}`
*   **HTTP Method**: `PUT`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 400, 401, 403, 404

---

### 14.12 DELETE /api/v1/admin/assets/{id}
*   **API Name**: Delete Asset Record
*   **URL**: `/api/v1/admin/assets/{id}`
*   **HTTP Method**: `DELETE`
*   **Required Role(s)**: Admin
*   **Expected Status Codes**: 200, 401, 403, 404

---

## 15. Profile APIs

### 15.1 GET /api/v1/profile
*   **API Name**: Get Personal Profile
*   **URL**: `/api/v1/profile`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves profile details of the authenticated user.
*   **Authentication Required**: Yes
*   **Success Response**: Profile details.
*   **Expected Status Codes**: 200, 401

---

### 15.2 PUT /api/v1/profile
*   **API Name**: Update Personal Profile
*   **URL**: `/api/v1/profile`
*   **HTTP Method**: `PUT`
*   **Description**: Updates user's name or phone number.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "fullName": "Priya Sharma",
      "phone": "+91 98200 66778"
    }
    ```
*   **Expected Status Codes**: 200, 400, 401

---

### 15.3 POST /api/v1/profile/upload-avatar
*   **API Name**: Upload Profile Avatar
*   **URL**: `/api/v1/profile/upload-avatar`
*   **HTTP Method**: `POST`
*   **Description**: Uploads and sets profile avatar picture.
*   **Authentication Required**: Yes
*   **Request Body Schema**: Form-data with a `file` field.
*   **Expected Status Codes**: 200, 400, 401

---

### 15.4 PUT /api/v1/profile/change-password
*   **API Name**: Update Account Password
*   **URL**: `/api/v1/profile/change-password`
*   **HTTP Method**: `PUT`
*   **Description**: Updates account password.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "currentPassword": "OldPassword123!",
      "newPassword": "NewPassword123!"
    }
    ```
*   **Validation Rules**:
    *   `currentPassword`, `newPassword`: Required.
    *   `newPassword`: Minimum 6 characters.
*   **Expected Status Codes**: 200, 400, 401

---

## 16. Search APIs

Search is integrated into list queries. Below is the standard mapping syntax:

### Search Syntax
*   `search`: Query parameter passed to `/users`, `/requests`, and `/admin/assets`.
*   **Request**: `GET /api/v1/requests?search=Figma`
*   **Execution Logic**: The backend translates this into a SQL query searching across multiple text columns:
    ```sql
    SELECT * FROM dbo.ServiceRequests
    WHERE (RequestNumber LIKE '%Figma%' OR Title LIKE '%Figma%' OR Description LIKE '%Figma%')
      AND IsDeleted = 0;
    ```

---

## 17. Filter APIs

### 17.1 GET /api/v1/filters/departments
*   **API Name**: Get Active Departments List
*   **URL**: `/api/v1/filters/departments`
*   **HTTP Method**: `GET`
*   **Description**: Returns active departments for filter menus.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

### 17.2 GET /api/v1/filters/technicians
*   **API Name**: Get Active Technicians List
*   **URL**: `/api/v1/filters/technicians`
*   **HTTP Method**: `GET`
*   **Description**: Returns active technicians, optionally filtered by `departmentId`.
*   **Authentication Required**: Yes
*   **Expected Status Codes**: 200, 401

---

## 18. Report APIs

### 18.1 GET /api/v1/reports/export-requests
*   **API Name**: Export Requests Data
*   **URL**: `/api/v1/reports/export-requests`
*   **HTTP Method**: `GET`
*   **Description**: Returns requests data as a file download (CSV/XLSX).
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD
*   **Query Parameters**: Matches query filters for `/requests`.
*   **Success Response**:
    *   `Content-Type`: `text/csv` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
    *   `Content-Disposition`: `attachment; filename="ServiceRequests_Export.xlsx"`
*   **Expected Status Codes**: 200, 401, 403

---

### 18.2 GET /api/v1/reports/resolution-trends
*   **API Name**: Get Monthly Resolution Trends
*   **URL**: `/api/v1/reports/resolution-trends`
*   **HTTP Method**: `GET`
*   **Description**: Returns monthly average resolution times in hours.
*   **Authentication Required**: Yes
*   **Required Role(s)**: Admin, HOD
*   **Success Response**: List of monthly trend data.
*   **Expected Status Codes**: 200, 401, 403

---

### 18.3 GET /api/v1/reports/department-analytics
*   **API Name**: Get Department Performance Summary
*   **URL**: `/api/v1/reports/department-analytics`
*   **HTTP Method**: `GET`
*   **Required Role(s)**: Admin, HOD
*   **Success Response**: Summary of tickets resolved and average duration per department.
*   **Expected Status Codes**: 200, 401, 403

---

## 19. Settings APIs

### 19.1 GET /api/v1/settings
*   **API Name**: Get User Settings
*   **URL**: `/api/v1/settings`
*   **HTTP Method**: `GET`
*   **Description**: Retrieves settings preferences for the authenticated user.
*   **Authentication Required**: Yes
*   **Success Response**: User settings profile.
*   **Expected Status Codes**: 200, 401

---

### 19.2 PUT /api/v1/settings
*   **API Name**: Update User Settings
*   **URL**: `/api/v1/settings`
*   **HTTP Method**: `PUT`
*   **Description**: Updates user settings preferences.
*   **Authentication Required**: Yes
*   **Request Body Schema**:
    ```json
    {
      "theme": "dark",
      "twoFactorEnabled": false,
      "notifyRequestUpdates": true,
      "notifyApprovalAlerts": true,
      "notifySLAWarnings": true,
      "notifyAssetEvents": false,
      "notifyEmailDigest": false
    }
    ```
*   **Expected Status Codes**: 200, 400, 401

---

## File Upload API Standard

The upload endpoint `POST /api/v1/attachments/upload` follows a standard workflow to handle files securely:

```
[UI Upload] -> [API Validation] -> [Cloud Storage] -> [Return Metadata]
```

1.  **File Stream Handling**: The API processes the upload stream using `MultipartReader` to avoid loading large files into memory.
2.  **File Size/Type Check**: The request is rejected with `415 Unsupported Media Type` or `400 Bad Request` if size exceeds 10MB or the extension is not in the allowed list (e.g. executable files like `.exe` are blocked).
3.  **Sanitization**: The file is assigned a unique GUID (e.g. `d3b07384d113.png`) and uploaded to storage.
4.  **Database Log**: Creates a record in the database and returns the generated `attachmentId` and storage URL.

---

## Authentication Flow

```
[Requestor UI] --(Credentials)--> [API Auth Controller]
                                          |
                               (Validates PasswordHash)
                                          |
[Requestor UI] <---(Access/Refresh)-------/
```

1.  **Request**: The client submits a POST request to `/api/v1/auth/login` with their credentials.
2.  **Validation**: The API checks the credentials against the database.
3.  **Token Generation**: Upon successful validation, the API generates a JWT containing standard claims (e.g., UserId, Email, Role, Department).
4.  **Response**: The API returns the token and expiration details.
5.  **Refresh Token Flow**: A refresh token is stored in a secure, HTTP-only cookie. When the access token expires, the client calls the refresh endpoint to obtain a new one without prompting the user to log in again.

---

## Authorization Flow

```
[Request Header] -> [Authentication Middleware] -> [Claims Principal Extraction]
                                                           |
[Request Blocked] <---(No/Insufficient Claims)---- [Checks Permissions]
                                                           |
[Controller Access] <--(Has Required Claims)---------------/
```

1.  **Token Parsing**: The authentication middleware extracts the JWT from the `Authorization: Bearer <token>` header of incoming requests.
2.  **Claims Principal Extraction**: The middleware validates the signature and expiration, converting the claims into a `ClaimsPrincipal` accessible in the request context.
3.  **Role/Permission Enforcement**: The controller uses authorization attributes (e.g. `[Authorize(Roles = "Admin,HOD")]` or custom permission policies like `[HasPermission("approvals.decide")]`) to enforce access control. If the claims are missing, the request is rejected with `403 Forbidden`.

---

## API Dependency Flow

The diagram below shows the dependency flow between APIs. Certain lookup and administrative endpoints must be fully implemented and populated before core request and approval workflows can function:

```
  [1. Auth APIs] 
        |
        v
  [2. Master Lookups] (Statuses, Departments, Service/Request Types)
        |
        v
  [3. Administrative Maps] (Personnel Mapping, Assignment Rules)
        |
        +-----------------------+-----------------------+
        |                                               |
        v                                               v
  [4. Service Requests] (Create/Update)            [5. Assets Management]
        |                                               |
        +-----------------------+-----------------------+
        |
        v
  [6. Timeline & Replies] (Workflows / Technician updates)
        |
        v
  [7. Approvals Decisions] (HOD Action)
        |
        v
  [8. Notifications & Dashboards]
```

*   **Phase 1**: Authentication APIs (`/api/v1/auth/login`) are needed first to secure subsequent API calls.
*   **Phase 2**: Configuration Masters (`/statuses`, `/departments`, `/categories/*`) must be populated to provide lookups for creating users and requests.
*   **Phase 3**: Personnel mapping and auto-assignment rules must be configured before requests can be assigned to technicians.
*   **Phase 4**: Once dependencies are met, service requests and assets can be created and managed.

---

## Endpoint Implementation Order

This recommended sequence minimizes dependency blockages during development:

### Step 1: Authentication & Infrastructure
1.  `POST /api/v1/auth/login`
2.  `POST /api/v1/auth/signup`
3.  `GET /api/v1/profile`

### Step 2: Configuration Masters (Lookups)
4.  `GET /api/v1/statuses`
5.  `GET /api/v1/filters/departments`
6.  `GET /api/v1/categories/service-types`
7.  `GET /api/v1/categories/request-types`

### Step 3: Admin Management & Personnel Mappings
8.  `GET /api/v1/users` & `POST /api/v1/users`
9.  `POST /api/v1/admin/personnel` (Assign HODs / Technicians)
10. `POST /api/v1/admin/mappings` (Technician assignments rules)

### Step 4: Asset Tracking
11. `GET /api/v1/admin/assets`
12. `POST /api/v1/admin/assets`

### Step 5: Service Request Lifecycle
13. `POST /api/v1/attachments/upload`
14. `POST /api/v1/requests` (Raising tickets)
15. `GET /api/v1/requests` & `GET /api/v1/requests/{id}`
16. `POST /api/v1/requests/{id}/assign`
17. `POST /api/v1/requests/{id}/status` (Technician work transitions)
18. `POST /api/v1/requests/{id}/replies` (Adding comments)

### Step 6: Approvals & Operations
19. `POST /api/v1/requests/{id}/approvals` (HOD decisions)
20. `GET /api/v1/notifications`
21. `GET /api/v1/dashboard/stats`

---

## API Testing Checklist

Use this checklist to verify the API implementation using tools like Postman or Swagger UI:

- `[ ]` **Auth**: Verify requests fail with `411 Unauthorized` if the `Authorization` header is missing or contains an invalid token.
- `[ ]` **Validation**: Submit invalid payloads to verify the API returns `400 Bad Request` with structured validation messages.
- `[ ]` **Permissions**: Log in as a `Requestor` and attempt to call `/api/v1/users` or `/api/v1/admin/mappings` to verify the request is blocked with `403 Forbidden`.
- `[ ]` **Auto-Assignment**: Submit a request matching an auto-assignment rule and verify the ticket is created in `Assigned` status with the correct `AssigneeUserId` populated.
- `[ ]` **Approval Trigger**: Submit a "Software Request" and verify the request is created in `Pending Approval` status and a pending record is added to the approvals table.
- `[ ]` **Soft Delete**: Delete a user or request and verify they are omitted from standard lists but remain in the database with deletion audit fields populated.
- `[ ]` **Upload Bounds**: Attempt to upload a 15MB file or an `.exe` file to verify the request is rejected with `400` or `415`.

---

## Future APIs

These endpoints can be added in future iterations to support more advanced features:

1.  **SLA Alerts (`/api/v1/alerts/sla-breaches`)**: Returns tickets that have breached or are approaching their SLA limits.
2.  **Knowledge Base Search (`/api/v1/kb/search`)**: Allows users to search for self-service solutions before raising a ticket.
3.  **Satisfaction Survey (`/api/v1/requests/{id}/feedback`)**: Collects user feedback ratings and comments after ticket resolution.
4.  **Asset Audit Log (`/api/v1/admin/assets/{id}/history`)**: Lists the assignment history of a specific asset.
