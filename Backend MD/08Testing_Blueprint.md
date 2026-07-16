# Testing Blueprint
## Service Request Management System

This document outlines the detailed testing strategy and execution plan for the **Service Request Management System** backend built using **ASP.NET Core Web API**.

---

## 1. Testing Overview

### Purpose
To verify that the Web API endpoints, business logic flows, and database transactions meet the functional and security requirements defined in previous blueprint documents.

### Testing Goals
*   Ensure authentication and authorization controls block unauthorized operations.
*   Verify that service request workflow logic transitions tickets correctly.
*   Enforce validation rules on all incoming payloads before database persistence.
*   Ensure the database is not corrupted by transaction failures or cascade cycles.

---

## 2. Testing Types & Strategy

### 2.1 Unit Testing
*   *Target*: Isolated application services, entity logic, mapping profiles, and custom validators.
*   *Strategy*: Write tests using xUnit, mocking all external database and email dependencies using NSubstitute or Moq.

### 2.2 Integration Testing
*   *Target*: Database repositories and DbContext mapping configurations.
*   *Strategy*: Run tests against an isolated SQL Server test instance, validating data access queries and soft delete filters.

### 2.3 API Testing
*   *Target*: Controller endpoints and HTTP response mappings.
*   *Strategy*: Use WebApplicationFactory to run API controllers in memory, verifying response payloads, status codes, and routing parameters.

### 2.4 Database Testing
*   *Target*: Foreign keys, check constraints, default values, and index functionality.
*   *Strategy*: Verify database triggers, check constraints (e.g., priority bounds), and unique constraint integrity during database inserts.

### 2.5 Authentication & Authorization Testing
*   *Target*: JWT signature validation, token expiration limits, and claims checking.
*   *Strategy*: Send requests containing expired, tampered, or missing tokens, and verify that the API returns the correct HTTP status codes (`401 Unauthorized` or `403 Forbidden`).

### 2.6 Validation Testing
*   *Target*: Request validation boundaries.
*   *Strategy*: Verify that malformed or incomplete request payloads fail FluentValidation checks and return structured validation error messages.

### 2.7 Performance Testing
*   *Target*: High-load endpoints (e.g., logins, requests queries).
*   *Strategy*: Run tests using tools like k6 or JMeter to verify response times, database connection limits, and CPU usage.

### 2.8 Security Testing
*   *Target*: SQL Injection, XSS, file upload security, and CORS policies.
*   *Strategy*: Attempt to execute SQL injections in query strings and upload malicious files to verify that security controls block the requests.

### 2.9 Regression Testing
*   *Target*: Existing endpoints and use cases during code updates.
*   *Strategy*: Run the automated test suite in the CI/CD pipeline after merging feature branches to ensure changes do not break existing functionality.

### 2.10 User Acceptance Testing (UAT)
*   *Target*: Verification of end-to-end user workflows.
*   *Strategy*: Deploy the API to a staging environment and run validation tests against mock client requests.

---

## 3. Module-Wise Testing Plans

### 3.1 Authentication Module
*   **Features to Test**: Login credentials validation, JWT generation, password resets, registration domains, and account lockouts.
*   **Expected Results**: Successful credentials return access tokens, lockouts trigger after 5 failures, and password changes invalidate active refresh tokens.
*   **Dependencies**: Database engine (`dbo.Users`).

### 3.2 Service Requests Module
*   **Features to Test**: Ticket creation, auto-assignment mapping, status updates, timelines, and HOD approvals.
*   **Expected Results**: Restructured request parameters trigger HOD approval, valid requests are assigned automatically, and status updates record timeline entries.
*   **Dependencies**: Auth modules, master configurations, database engine.

---

## 4. API Endpoints Test Cases Matrix

The table below outlines test cases for key API endpoints:

| Test ID | Endpoint | Method | Test Description | Preconditions | Input Data | Expected Response | Expected Status | Pass Criteria |
| :--- | :--- | :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-API-01**| `/auth/login` | `POST` | Valid credentials login | Active user exists | Valid email & password | Token payload and user detail | `200 OK` | Access token returned and refresh cookie set. |
| **TC-API-02**| `/auth/login` | `POST` | Invalid password login | Active user exists | Valid email, wrong password | Error message | `401` | Unauthorized status returned. |
| **TC-API-03**| `/auth/login` | `POST` | Lockout validation | Active user exists | 5 incorrect login attempts | Lockout warning message | `400` | Account locked out for 15 minutes. |
| **TC-API-04**| `/requests` | `POST` | Create standard request | Authenticated user | Valid title & description | Created request details | `201` | Request number generated and saved to DB. |
| **TC-API-05**| `/requests` | `POST` | Create short description | Authenticated user | Description under 20 chars | Validation error payload | `400` | Error details point to description field. |
| **TC-API-06**| `/requests` | `POST` | Create restricted request | Authenticated user | Request Type = 'Software' | Status: Pending Approval | `201` | Ticket created and approval entry generated. |
| **TC-API-07**| `/requests/{id}/assign`| `POST` | Unauthorized assignment | User role = Requestor| AssigneeUserId = 2 | Access denied message | `403` | Request blocked by role constraint policy. |

---

## 5. Authentication Test Cases

*   **TC-AUTH-01: Logout Revocation**: Verify that calling the logout endpoint invalidates the user's refresh token and removes the cookie.
*   **TC-AUTH-02: Expired Tokens**: Attempt to query a protected endpoint using an expired access token and verify the request is blocked with `401 Unauthorized`.
*   **TC-AUTH-03: Invalid Signature**: Send a request with a modified JWT signature and verify it is blocked.
*   **TC-AUTH-04: Refresh Token Reuse**: Attempt to refresh an access token using a previously rotated refresh token and verify the request is rejected.

---

## 6. Authorization Test Cases

*   **TC-AUTHZ-01: Admin Access**: Verify that users with the `Admin` role can query all database logs and configuration endpoints.
*   **TC-AUTHZ-02: HOD Department Boundaries**: Log in as an HOD and verify that attempts to approve tickets belonging to another department are blocked with `403 Forbidden`.
*   **TC-AUTHZ-03: Technician Edit Rules**: Verify that a technician can only edit the status field of tickets assigned to them.
*   **TC-AUTHZ-04: Requestor Query Boundaries**: Verify that a Requestor can only query their own raised tickets.

---

## 7. Validation Test Cases

*   **TC-VAL-01: Null Fields**: Submit a request with missing required fields (e.g., category ID) and verify the API returns `400 Bad Request`.
*   **TC-VAL-02: String Bounds**: Submit a request title longer than 150 characters and verify the validation fails.
*   **TC-VAL-03: Phone Regex**: Submit an invalid phone format during profile updates and verify the request fails validation checks.
*   **TC-VAL-04: Duplicate Registration**: Attempt to sign up using an email address that is already registered and verify it returns a `409 Conflict` error.

---

## 8. Database Integration Testing

*   **TC-DB-01: Soft Delete Filters**: Delete a request record and verify it is omitted from standard query results.
*   **TC-DB-02: Foreign Key Constraints**: Attempt to insert a request with an invalid status ID and verify the database rejects the write.
*   **TC-DB-03: Unique Indexes**: Verify that attempts to insert duplicate employee IDs trigger a unique constraint violation.
*   **TC-DB-04: Date Audit Checks**: Update a request and verify that the `UpdatedAt` field is updated with the current database time.

---

## 9. File Upload Testing

*   **TC-FILE-01: Block Executables**: Attempt to upload a `.exe` script file and verify the request returns `415 Unsupported Media Type`.
*   **TC-FILE-02: Max Size Violation**: Attempt to upload a 12MB file and verify it is rejected.
*   **TC-FILE-03: GUID Renaming**: Upload a file and verify it is saved in storage using a generated GUID filename to prevent collisions.

---

## 10. Notifications & Alerts Testing

*   **TC-NOTIF-01: Real-Time SignalR Broadcast**: Log in to the client UI as a technician, trigger an assignment, and verify that the technician receives a real-time SignalR alert.
*   **TC-NOTIF-02: Background Email Queue**: Verify that creating a request queues a verification email job in the background worker.

---

## 11. Dashboard Analytics Testing

*   **TC-DASH-01: Query Accuracy**: Verify that `/dashboard/stats` matches the sum of the user's active requests.
*   **TC-DASH-02: Activity Feed**: Verify that updating a ticket's status adds a corresponding record to the dashboard activity feed.

---

## 12. Search, Filter, and Pagination Testing

*   **TC-PAGE-01: Page Sizes**: Query requests with a page size of 5 and verify the response contains exactly 5 records and correct pagination metadata.
*   **TC-PAGE-02: Search Filter**: Search for "Figma" and verify that only requests containing "Figma" in their title or description are returned.
*   **TC-PAGE-03: Whitelist Sorting**: Sort requests by `InvalidField` and verify that the API falls back to default sorting (`CreatedAt`) rather than failing.

---

## 13. Centralized Error Handling Testing

*   **TC-ERR-01: Database Connection Failure**: Simulate a database connection loss and verify that the API returns a standard `500 Internal Server Error` payload without exposing SQL stack traces.
*   **TC-ERR-02: Custom Domain Exceptions**: Trigger a custom domain exception (e.g., ticket is locked) and verify the API returns a structured validation error response.

---

## 14. Performance Testing Plan

*   **Load Test**: Simulate 100 concurrent users performing queries on `/requests` and verify that the average response time remains under 200ms.
*   **Stress Test**: Scale load up to 1000 concurrent users to identify database connection bottle-necks.
*   **Bulk Query Performance**: Populate the database with 100,000 requests and verify that queries using pagination and indexes execute in less than 100ms.

---

## 15. Security Verification Testing

*   **SQL Injection (SQLi)**: Attempt to pass SQL commands (e.g., `requests?search='; DROP TABLE ServiceRequests;--`) in query inputs and verify they are treated as literal strings.
*   **Cross-Site Scripting (XSS)**: Submit a request containing HTML tags (e.g., `<script>alert('xss')</script>`) and verify that the tags are sanitized.
*   **CORS Verification**: Send requests from unauthorized domains and verify they are blocked by CORS policies.

---

## 16. Logging Verification

*   **Security Events Logs**: Verify that failed login attempts and lockout events are logged in the database audit log.
*   **Exceptions Stack Trace**: Verify that all application exceptions are logged with stack traces to the log files.

---

## 17. Test Data Strategy

A set of static test data is defined to verify API operations:

*   **Mock Users**:
    *   `Admin`: `admin@gmail.com` / `123456`
    *   `HOD (IT)`: `hod@gmail.com` / `123456`
    *   `Technician`: `tech@gmail.com` / `123456`
    *   `Requestor`: `requestor@gmail.com` / `123456`
*   **Mock Masters**: Seed 5 active departments and 14 request categories.
*   **Mock Requests**: Populate the test environment with 15 requests of varying priorities and statuses.

---

## 18. Bug Tracking Strategy

To manage bugs identified during testing, use the following tracking guidelines:

### Bug Severity
*   `Blocker`: Authentication failures, security vulnerabilities, database corruption.
*   `Major`: Ticket state transitions fail, search filters do not return correct results.
*   `Minor`: Typographical errors in messages, color codes do not render correctly in badges.

### Bug Priority
*   `High`: Core workflow blockages or security vulnerabilities. Must resolve before release.
*   `Medium`: Operational errors with available workarounds.
*   `Low`: Cosmetical issues or future enhancements.

---

## 19. Test Execution Checklist

Verify the following before release:

- [ ] Run the complete unit test suite and verify that all tests pass.
- [ ] Run the integration test suite against a test database.
- [ ] Run automated API validation tests for all endpoints.
- [ ] Verify that access tokens expire after 15 minutes and refresh cookies are set.
- [ ] Test file uploads with both allowed and blocked file extensions.
- [ ] Run database migration tests and verify seed data.
- [ ] Test rate-limiting policies on authentication endpoints.

---

## 20. Final Release Checklist

All of the following items must pass before production deployment:

- [ ] All automated tests (unit, integration) pass successfully.
- [ ] Code coverage meets the target minimum threshold (80%).
- [ ] Vulnerability scans show no open threats.
- [ ] Centralized logging is configured and sending logs to the log consolidator.
- [ ] Production database connection strings and JWT secrets are configured.
- [ ] CORS policies restrict access to authorized frontend domains.
- [ ] HTTPS connections are enforced on all API endpoints.
- [ ] Database backup schedules and recovery procedures are verified.
- [ ] Load tests show response times remain under 200ms under load.
- [ ] API documentation (Swagger UI) is enabled and documented.
- [ ] Clean roll-back scripts are prepared for all database migrations.
- [ ] Domain names and SSL configurations are active and verified.
