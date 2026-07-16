# Project Handover Blueprint
## Service Request Management System

This document serves as the master technical handover guide and system reference for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Executive Summary

### Overview
*   **Project Name**: Service Request Management System.
*   **Project Purpose**: Centralized corporate service desk backend to raise, track, assign, and approve facility, IT, and administrative requests.
*   **Business Objective**: Automate manual ticketing workflows, enforce response SLAs, coordinate departmental HOD approvals, and generate performance analytics.
*   **Target Users**: Requesters, Technicians, Heads of Department (HODs), and Administrators.
*   **Project Scope**: RESTful API Web services, JWT authentication, RBAC, background scheduler workers, and cloud attachment storage. (Note: Frontend is a React application).

---

## 2. Functional Overview

The backend comprises six core functional modules:
1.  **Identity Management**: Enforces registration, JWT credential validation, role mapping, password resets, and account lockout rules.
2.  **Service Request Lifecycle**: Manages ticket creation, timeline tracking, replies, auto-assignment mapping, and status updates.
3.  **HOD Approvals**: Implements validation rules for high-priority tickets (e.g. software, hardware, and access requests) requiring department manager sign-offs.
4.  **Asset Management**: Tracks corporate equipment categories, warranty metrics, and employee assignments.
5.  **Notifications & Alerts**: Broadcasts real-time SignalR alerts to the UI and queues background notification emails.
6.  **Configuration Masters**: Provides administrative screens for managing statuses, departments, and auto-assignment rules.

---

## 3. System Architecture Summary

The backend uses a **Clean Architecture** pattern to isolate core business rules from external infrastructure dependencies:

```
[ Web API Presentation ] ──> ( Handles HTTP / Controls Auth )
         │
         v
[ Application Core ] ──> ( Interfaces / Use Cases / Mappings )
         │
         v
[ Persistence / Infra ] ──> ( SQL Server DbContext / Cloud Storage / SMTP Client )
         │
         v
[ Domain Model ] ──> ( Entities / Constants / Enums )
```

*   **HTTP Request Flow**:
    `React Client -> Web API Controller -> Use Case Service -> Persistence Repository -> SQL Database`.
*   The API remains stateless, allowing horizontal scaling across containerized clusters.

---

## 4. Database Summary

*   **Total Tables**: 20.
*   **Clustered Indexes**: Default on primary key identity columns.
*   **Master Tables**: `dbo.Roles`, `dbo.Permissions`, `dbo.Departments`, `dbo.ServiceTypes`, `dbo.RequestTypes`, `dbo.ServiceRequestStatuses`, `dbo.AssetCategories`.
*   **Transaction Tables**: `dbo.Users`, `dbo.ServiceRequests`, `dbo.ServiceRequestReplies`, `dbo.Approvals`, `dbo.Assets`, `dbo.Notifications`.
*   **Audit Tables**: `dbo.ServiceRequestTimeline`, `dbo.AuditLogs`.

---

## 5. API Summary

*   **Authentication APIs**: Endpoint mappings for login, signup, refresh tokens, password resets, and logouts.
*   **Master APIs**: Read and write endpoints for lookup tables (departments, statuses, request types).
*   **Service Request APIs**: Core ticket operations, assignments, status transitions, comments, and approvals.
*   **Dashboard APIs**: Role-based summary stats and recent activity feeds.
*   **Report APIs**: CSV/XLSX reporting data export and resolution trend metrics.
*   **Notification APIs**: In-app alerts, read tracking, and unread counters.

---

## 6. Authentication & Authorization Summary

*   **Security Model**: JWT authentication with claims-based authorization.
*   **Role Mapping**: Standard roles (Admin, HOD, Technician, Requestor) mapped to permissions in `dbo.RolePermissions`.
*   **Access Control**: Access tokens expire in 15 minutes, and refresh tokens are stored in secure cookies for session renewal.
*   **Password Encryption**: Passwords are encrypted using BCrypt before being saved to the database.

---

## 7. Folder Structure Summary

The code repositories are organized to enforce architectural boundaries:
*   `src/ServiceDesk.Domain/`: Core entities, enums, constants, and domain exceptions.
*   `src/ServiceDesk.Application/`: Services, DTOs, interfaces, mappings, and validators.
*   `src/ServiceDesk.Persistence/`: DbContext, database repositories, configurations, and migration logs.
*   `src/ServiceDesk.Infrastructure/`: SMTP clients, cloud storage integrations, and background schedulers.
*   `src/ServiceDesk.WebApi/`: Web controllers, custom middlewares, filters, and configuration files.

---

## 8. Configuration Guide

Production configurations are overridden using host environment variables:
*   `ConnectionStrings__DefaultConnection`: Connection string for SQL Server.
*   `JwtSettings__Secret`: JWT symmetric security key.
*   `EmailSettings__Password`: SMTP gateway password.
*   `ASPNETCORE_ENVIRONMENT`: Set to `Production` to disable detailed error logs.

---

## 9. Deployment Summary

*   **Development**: Run Web API in Docker containers with local SQL Server instances.
*   **Staging/Production**: Deploy Web API to container services (e.g., AWS ECS, Azure App Service) behind load balancers with managed SQL databases.

---

## 10. Testing Summary

*   **Unit Tests**: Test core domain rules and application services using mock frameworks (xUnit, NSubstitute).
*   **Integration Tests**: Verify database queries and transaction states against test database instances.
*   **API Tests**: Test HTTP routing parameters and JSON responses using WebApplicationFactory in memory.

---

## 11. Security Summary

*   **Input Sanitization**: HTML-encode text fields to protect against XSS injections.
*   **Database Security**: Use parameterized queries to block SQL injection.
*   **File Upload Validation**: Restrict file extensions, scan uploads, and store files in cloud storage.
*   **Header Protection**: Enforce security headers (HSTS, CSP, X-Frame-Options) on all API responses.

---

## 12. Performance Summary

*   **Query Optimization**: Use `.AsNoTracking()` and explicit projections in read-only queries.
*   **Caching Strategy**: Cache static lookup tables in memory and user sessions in Redis.
*   **Async Patterns**: Use asynchronous operations for all database calls and I/O tasks.

---

## 13. Maintenance Summary

*   **Logging**: Use Serilog for asynchronous logging and set production log level to `Warning`.
*   **Backups**: Configure transaction log backups every 15 minutes, full database backups daily, and files incremental backups daily.
*   **Index Rebuilds**: Rebuild database indexes weekly and update statistics daily.

---

## 14. Troubleshooting Guide

| Issue Identified | Likely Cause | Recommended Troubleshooting Steps |
| :--- | :--- | :--- |
| **401 Unauthorized** | Expired or tampered JWT access token. | Verify the token signature and expiration, and check that token refresh runs on expiration. |
| **403 Forbidden** | Role or permission claims are missing. | Check that permissions are mapped correctly in the database and user roles are updated. |
| **500 Database Error** | Database connection issues or connection pool starvation. | Verify connection strings, database server availability, and check connection pool limits. |
| **Upload Failure** | Unsupported file extension or file size exceeds limits. | Check file size restrictions (max 10MB) and allowed extensions. |

---

## 15. Known Limitations

*   **File Upload Limits**: Large file uploads (>10MB) are blocked by the gateway to prevent server load spikes.
*   **MFA Availability**: Multi-factor authentication is not supported in the initial version.
*   **Single-Role Mapping**: Users can only be assigned exactly one role at a time.

---

## 16. Future Roadmap

*   **SSO Integration**: Support single sign-on (SSO) login with Microsoft Entra ID or Google Workspace.
*   **Multi-Factor Authentication (2FA)**: Integrate TOTP authenticator app support.
*   **Knowledge Base**: Add a searchable database of help articles.

---

## 17. Technical Debt

*   **In-Memory Session Storage**: The initial deployment relies on in-memory caching. High-load deployments should switch to Redis caching.
*   **Local File Uploads**: During development, files are saved locally. Cloud storage integrations should be configured before production rollout.

---

## 18. Project Statistics

*   **Modules**: 6.
*   **APIs**: 48.
*   **Database Tables**: 20.
*   **Security Roles**: 4.
*   **System Permissions**: 19.
*   **Workflows**: User signup, ticket lifecycle, and HOD approvals.

---

## 19. Developer Onboarding Guide

Follow these steps to set up the development environment:
1.  Clone the repository and install the .NET SDK.
2.  Set up a local database connection string in user secrets.
3.  Install the EF Core CLI tool and run database migrations:
    `dotnet ef database update --project src/ServiceDesk.Persistence`
4.  Run the application: `dotnet run --project src/ServiceDesk.WebApi`.
5.  Access the API documentation at `https://localhost:7124/swagger`.

---

## 20. Final Acceptance Checklist

Verify the following items before project delivery:

- [ ] All automated tests (unit, integration) pass successfully.
- [ ] API endpoints return correct HTTP status codes and responses.
- [ ] Authentication and authorization controls block unauthorized operations.
- [ ] Connection strings and passwords are encrypted in production settings.
- [ ] Checked that health checks return status `200`.

---

## 21. Handover Package Checklist

Confirm the handover package contains the following items:
*   [ ] Source Code Repository.
*   [ ] Database Schema Design Blueprints.
*   [ ] API Design Blueprints.
*   [ ] Auth Planning Blueprints.
*   [ ] Deployment & Maintenance Runbooks.
*   [ ] Release Notes and Migration Scripts.

---

## 22. Document Reference Index

| Document Title | Purpose | When to Use It | Dependencies |
| :--- | :--- | :--- | :--- |
| **Backend_Database_Blueprint.md** | Detailed database schema and relationships. | Database setup and migrations. | None |
| **API_Blueprint.md** | REST API specifications and endpoints. | API implementation and integrations. | Database Blueprint |
| **Authentication_Authorization_Blueprint.md** | Security design and JWT token flows. | Implementing auth policies. | API Blueprint |
| **Project_Architecture_Blueprint.md** | Project layers and clean design. | Project setup and code mapping. | None |
| **Folder_Structure_Blueprint.md** | Solution folders and namespaces. | Code organization and directories. | Architecture Blueprint |
| **Validation_BusinessRules_Blueprint.md** | Input validation and workflow rules. | Validation and service workflows. | API Blueprint |
| **Implementation_Roadmap.md** | Step-by-step implementation order. | Task scheduling and development. | Folder Structure |
| **Testing_Blueprint.md** | Test strategy and execution cases. | Testing and quality assurance. | API Blueprint |
| **Deployment_Blueprint.md** | Hosting, scaling, and deployment. | Deploying to hosting environments. | Architecture Blueprint |
| **Performance_Optimization_Blueprint.md** | Caching and data optimizations. | Performance tuning and profiling. | Database Blueprint |
| **Security_Audit_Blueprint.md** | Threat modeling and security audits. | Security scanning and audit rules. | Auth Blueprint |
| **Maintenance_Blueprint.md** | Backup policies and runbooks. | Ongoing production maintenance. | Deployment Blueprint |

---

## 23. Conclusion

This project handover blueprint provides the complete architectural roadmap for the Service Request Management System backend. By following the Clean Architecture pattern, implementing secure JWT controls, and optimizing database queries, the Web API is structured for stability, security, and performance. Use this document as the master reference guide during system implementation.
