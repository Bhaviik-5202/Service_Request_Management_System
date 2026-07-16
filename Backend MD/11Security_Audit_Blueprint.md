# Security Audit Blueprint
## Service Request Management System

This document outlines the detailed security audit rules, threat modeling, security checklists, vulnerability protections, and incident response procedures for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Security Overview

### Security Objectives
Enforce data confidentiality, request integrity, and auditing. The backend is designed to protect sensitive user credentials, prevent database corruption, and ensure only authorized users can perform system operations.

### Security Principles
*   **Defense in Depth**: Layer security controls across multiple boundaries (network firewalls, API gateway rate limiting, token validations, and database constraints).
*   **Least Privilege**: Users are granted only the minimum permissions required to perform their specific role responsibilities.
*   **Secure defaults**: Default configurations are secure (e.g. enforcing HTTPS and setting CORS to block unauthorized domains).

---

## 2. Threat Model

To protect the system, we define the following threat model:

*   **System Assets**: User accounts, password hashes, ticket histories, and uploaded attachments.
*   **API Entry Points**: Public auth controllers (`/auth/login`, `/auth/signup`), and protected ticket controllers.
*   **Trust Boundaries**:
    *   *External*: The public internet connection to the client UI.
    *   *Internal*: The secure virtual network linking the Web API and SQL Server.
*   **Threat Actors**: Malicious external entities, compromised users, and unauthorized insiders.
*   **Attack Vectors**: Brute force login attempts, SQL injection, cross-site scripting (XSS), and malicious file uploads.

---

## 3. Authentication Security

*   **Login Security**: Enforce lockouts (lock account for 15 minutes after 5 consecutive failures) to prevent brute-force password guessing.
*   **Password Strategy**: Passwords must be hashed using a strong hashing algorithm, such as **Argon2id** or **BCrypt**, before being saved to the database.
*   **JWT Configurations**: Sign access tokens using a secure algorithm (HMAC SHA256) and set short lifetimes (15 minutes).
*   **Refresh Token Security**: Store refresh tokens in `HttpOnly`, `Secure`, `SameSite=Strict` cookies to protect them from XSS theft.
*   **Session Revocation**: Password changes invalidate all active refresh tokens in the database, forcing users to log in again.

---

## 4. Authorization Security

*   **RBAC Boundaries**: Enforce role-based access control policies at the controller level to restrict access to sensitive resources.
*   **Granular Permissions**: Map permission claims directly to roles and verify them on controller actions (e.g. using `[HasPermission("requests.edit")]`).
*   **Verification**: Enforce policies to ensure users can only query or update resources within their authorization scope (e.g. technicians can only edit their assigned requests).

---

## 5. OWASP Top 10 Review

*   **Broken Access Control**: Checked by mapping permission policies to routes and verifying resource ownership.
*   **Cryptographic Failures**: Prevented by enforcing TLS 1.2/1.3 and hashing passwords.
*   **Injection**: Prevented by using Entity Framework Core's parameterized queries to block SQL injection.
*   **Insecure Design**: Solved by implementing Clean Architecture boundaries.
*   **Security Misconfiguration**: Prevented by setting the production environment flag `ASPNETCORE_ENVIRONMENT = Production`, which disables detailed error pages.
*   **Vulnerable Components**: Monitored by running dependency vulnerability scans in the CI/CD pipeline.
*   **Authentication Failures**: Mitigated by account lockouts, secure token storage, and session revocation policies.
*   **Software & Data Integrity Failures**: Enforce signature checks on JWTs and check file sizes during upload.
*   **Logging Failures**: Solved by configuring Serilog to capture audit logs and exception traces.
*   **SSRF (Server-Side Request Forgery)**: Prevented by verifying that file storage and external API integrations use fixed domain whitelists.

---

## 6. Input Validation Review

*   **DTO Validations**: Implement validation rules using FluentValidation to check data types, string lengths, and formats.
*   **XSS Protection**: HTML-encode all incoming text fields to sanitize inputs.
*   **File Upload Validation**: Enforce file size limits (max 10MB) and validate file signatures (magic bytes) to block malicious files.

---

## 7. API Security

*   **HTTPS Enforced**: Force SSL connections on all API endpoints.
*   **CORS Whitelist**: Restrict API access to authorized frontend domains.
*   **Security Headers**: Enforce security headers (HSTS, Content-Security-Policy, X-Frame-Options) on all API responses.
*   **Rate Limiting**: Apply rate-limiting policies to prevent denial-of-service (DoS) attacks.
*   **Payload Size Limits**: Set maximum request body sizes (max 15MB) in Web API configurations.

---

## 8. Database Security

*   **Least Privilege Access**: Database users are granted only read and write permissions (e.g. using `db_datareader` and `db_datawriter` roles). Schema modification permissions are blocked.
*   **Backup Security**: Database backups are encrypted and stored in geo-replicated, private storage.

---

## 9. File Upload Security Strategy

```
[UI Upload] ──> [Check File Size & Ext] ──> [Scan for Malware] ──> [Save as GUID in Cloud]
```

1.  **Allowed Extensions**: Restrict uploads to allowed formats (e.g., PDF, PNG, JPG).
2.  **Signature Check**: Validate file magic bytes to verify files match their extension formats.
3.  **Name Sanitization**: Rename files to GUIDs to prevent directory traversal attacks.
4.  **Malware Scanning**: Run files through a malware scanning service before saving.
5.  **Secure Storage**: Save files to cloud storage and use pre-signed URLs to restrict download access.

---

## 10. Logging & Audit Security

*   **Audit Logging**: Log all critical transactions (e.g. user updates, status changes, approvals) to a dedicated database table.
*   **Data Masking**: Mask sensitive personal data (e.g. passwords, payment details, session tokens) in application logs.
*   **Log Integrity**: Save logs to read-only storage to prevent tampering.

---

## 11. Secrets Management

*   **No Code Secrets**: Production secrets, database connection strings, and JWT signing keys must never be committed to Git repository files.
*   **Storage Practices**: Inject production secrets using environment variables or cloud key vaults.

---

## 12. Dependency Security

*   **Vulnerability Scanning**: Run dependency scanning tools (e.g. Snyk, Dependabot) in the CI pipeline to identify vulnerable libraries.
*   **Update Policy**: Set up monthly updates to upgrade third-party NuGet packages.

---

## 13. Infrastructure Security

*   **Network Firewalls**: Restrict database access to Web API container IP ranges.
*   **Reverse Proxy**: Configure Nginx to terminates SSL connections and block large header packets.
*   **Environment Isolation**: Separate networks and databases for development, staging, and production environments.

---

## 14. Monitoring & Incident Response

### Security Alerts
Set up real-time email alerts for critical security events:
*   Consecutive login failures indicating brute-force attacks.
*   Database connection errors.
*   Failed attempts to access administrative endpoints.

### Incident Workflow
```
[Event Triggered] ──> [Log Exception details] ──> [Alert Administrators] ──> [Revoke affected User sessions]
                                                                                        │
                                                                                 (Investigation)
                                                                                        │
[Restore normal operations] <── [Apply security patches] <── [Isolate affected services] ┘
```

---

## 15. Security Testing Plan

*   **Vulnerability Scanning**: Run security scans (e.g. OWASP ZAP) against staging environments.
*   **Penetration Testing**: Perform annual penetration testing to identify system vulnerabilities.
*   **Auth Testing**: Run tests using invalid or missing JWT tokens to verify authorization controls.

---

## 16. Security Release Checklist

Verify the following items before deploying:

- [ ] Changed all default secrets and passwords in production settings.
- [ ] Configured SSL/TLS certificates and verified auto-renewal schedules.
- [ ] Verified that CORS policies restrict access to authorized frontend domains.
- [ ] Configured rate-limiting policies to prevent DoS attacks.
- [ ] Set up server health monitoring and configured alerts.
- [ ] Confirmed that all SQL queries use parameterized parameters to prevent SQL injection.
- [ ] Verified that password hashes are generated using BCrypt before database save.
- [ ] Configured secure file validations and set request body size limits.
- [ ] Set log level to `Warning` and masked sensitive data in logs.
- [ ] Tested rollback procedures for database migrations and container deployments.

---

## 17. Future Security Enhancements

*   **SSO (Single Sign-On)**: Support Microsoft Entra ID or Google Workspace login.
*   **Two-Factor Authentication (2FA)**: Require a one-time password (TOTP) from an authenticator app (e.g. Google Authenticator).
*   **Zero Trust Architecture**: Enforce strict network segmentation and require authentication on all internal communications.
