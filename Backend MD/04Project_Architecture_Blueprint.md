# Project Architecture Blueprint
## Service Request Management System

This document outlines the detailed backend architectural plan for implementing the **Service Request Management System** using **ASP.NET Core Web API** and **Entity Framework Core**. It serves as the master structural blueprint to guide manual implementation.

---

## 1. Project Overview

### Purpose
The Service Request Management System backend is a centralized enterprise-grade service desk API. It enables users to submit facility and IT requests, automates assignment to department-specific technicians, routes high-impact tickets to HODs for sign-offs, and provides management reporting on response SLAs and department performance.

### Goals
*   **Separation of Concerns**: Decouple infrastructure, data access, and API controllers from business rules.
*   **High Testability**: Ensure core logic can be unit-tested without relying on databases or external email gateways.
*   **Maintainability**: Structure code folders and layers consistently to ease ongoing features additions.
*   **Scalability**: Design for database scaling and performance optimizations.

### Functional Requirements
*   Role-based dashboards (Admin, HOD, Technician, Requestor).
*   Dynamic configuration masters (Statuses, Departments, Request Types, Technicians).
*   Automated technician assignment rules.
*   Multi-stage approval workflow with remarks.
*   Audit timelines, commenting systems, and file uploads.
*   Real-time in-app notification triggers and background email alerts.

### Non-Functional Requirements
*   **Response Time**: API endpoints return within < 200ms under standard loads.
*   **Security**: Enforce SSL, CORS whitelist, rate-limiting, and SHA256 hashed password storage.
*   **Availability**: Support stateless horizontal deployment on Docker containers.
*   **Reliability**: Validate inputs at all boundaries to prevent SQL injection and cross-site scripting (XSS).

---

## 2. Recommended ASP.NET Core Architecture

We recommend **Clean Architecture** (Onion Architecture) over a traditional Layered Architecture.

```
       +---------------------------------------------+
       |             Infrastructure Layer            |
       |  (Persistence, EF Core, Blob, Email, Jobs)  |
       |  +---------------------------------------+  |
       |  |           Application Layer           |  |
       |  |  (Services, CQRS, Interfaces, DTOs)   |  |
       |  |  +---------------------------------+  |  |
       |  |  |          Domain Layer           |  |  |
       |  |  |   (Entities, Enums, Constants)  |  |  |
       |  |  +---------------------------------+  |  |
       |  +---------------------------------------+  |
       +---------------------------------------------+
                              ^
                              |
                     Presentation Layer (Web API)
```

### Clean Architecture
*   **Description**: Dependencies point inward. The core Domain Layer defines business entities and interfaces, the Application Layer contains workflow logic, and the outer Infrastructure Layer handles databases, file systems, and external integrations. The Web API acts as the outer gateway.
*   **Advantages**:
    *   *High Decoupling*: The database provider (EF Core / SQL Server) can be swapped out without modifying business rules.
    *   *High Testability*: The domain and application layers have zero dependencies on external systems and can be tested using mock frameworks.
    *   *Flexibility*: Third-party integrations (S3 storage, SMTP client) are placed behind interfaces in the outer layer.
*   **Disadvantages**:
    *   *Complexity*: Requires mapping between domain models and DTOs and introduces additional project projects.

### Layered Architecture
*   **Description**: Traditional database-centric design where the API layer calls the Business Logic Layer, which in turn calls the Data Access Layer.
*   **Advantages**:
    *   *Simplicity*: Lower learning curve, fewer project projects to manage, faster setup for simple prototypes.
*   **Disadvantages**:
    *   *Tight Coupling*: Business logic is tightly coupled to Entity Framework and database structures.

### Recommendation
**Clean Architecture** is recommended for this project. The system includes complex workflows (auto-assignment mappings, multi-department approval matrices, and third-party file uploads) that must remain isolated from database operations. This architecture allows developers to isolate business rules from changes in databases or API protocols.

---

## 3. Solution Structure

We structure the Visual Studio Solution (`.sln`) into logical projects matching Clean Architecture:

```
ServiceDesk.sln
│
├── src/
│   ├── ServiceDesk.Domain/           # Core Enterprise Entities (No dependencies)
│   ├── ServiceDesk.Application/      # Business Rules, Interfaces, DTOs, Use Cases
│   ├── ServiceDesk.Persistence/      # Entity Framework Core, SQL Server DB Context
│   ├── ServiceDesk.Infrastructure/   # File storage, SMTP, Hangfire backgrounds
│   └── ServiceDesk.WebApi/           # Controllers, Middleware, Auth Setup, Entrypoint
│
└── tests/
    ├── ServiceDesk.UnitTests/        # Application and Domain logic unit tests
    └── ServiceDesk.IntegrationTests/ # Controller integration tests
```

---

## 4. Folder Structure Tree

```
ServiceDesk/
│
├── ServiceDesk.Domain/
│   ├── Entities/             # ServiceRequest.cs, User.cs, Asset.cs, Department.cs
│   ├── Enums/                # Priority.cs, UserStatus.cs, AssetStatus.cs
│   ├── Exceptions/           # DomainException.cs, InvalidWorkflowException.cs
│   └── Constants/            # PermissionConstants.cs, SecurityRoles.cs
│
├── ServiceDesk.Application/
│   ├── Interfaces/
│   │   ├── IPersistence/     # IApplicationDbContext.cs, IUserRepository.cs
│   │   ├── IInfrastructure/  # IEmailService.cs, IBlobStorageService.cs
│   │   └── IServices/        # IRequestService.cs, INotificationService.cs
│   ├── Dtos/                 # RequestDto.cs, UserDto.cs, ReplyDto.cs
│   ├── Mappings/             # MappingProfile.cs (AutoMapper configuration)
│   ├── Validators/           # CreateRequestValidator.cs (FluentValidation rules)
│   └── UseCases/             # Core business service implementations
│
├── ServiceDesk.Persistence/
│   ├── Context/              # ServiceDeskDbContext.cs
│   ├── Migrations/           # EF Core SQL Database migrations
│   └── Repositories/         # UserRepository.cs, RequestRepository.cs
│
├── ServiceDesk.Infrastructure/
│   ├── Storage/              # AzureBlobStorageService.cs
│   ├── Email/                # SmtpEmailService.cs
│   └── BackgroundJobs/       # HangfireScheduler.cs
│
└── ServiceDesk.WebApi/
    ├── Controllers/          # AuthController.cs, RequestsController.cs, MastersController.cs
    ├── Middlewares/          # ExceptionHandlingMiddleware.cs, JwtMiddleware.cs
    ├── Filters/              # ValidateModelAttribute.cs
    ├── Configurations/       # DependencyInjection.cs, JwtSettings.cs
    └── appsettings.json      # Connection strings and configuration profiles
```

---

## 5. Layer Responsibilities

### Presentation Layer (`ServiceDesk.WebApi`)
*   **Controllers**: Handle HTTP routes, bind input parameters, and return uniform JSON responses.
*   **Middlewares**: Intercept HTTP lifecycles to handle global exceptions, validate JWT payloads, and log requests.
*   **Filters**: Enforce model state validations before controller execution.
*   **Configurations**: Bind settings files (`appsettings.json`) to C# options and register dependencies.

### Application Layer (`ServiceDesk.Application`)
*   **Services**: Implement business use cases, orchestrate database transactions, and dispatch notifications.
*   **DTOs**: Represent data contracts sent to and from the client UI.
*   **Interfaces**: Define contracts for databases and infrastructure components, decoupling the core from implementation details.
*   **Validators**: Implement request validation rules (e.g., minimum character length, valid formats).

### Domain Layer (`ServiceDesk.Domain`)
*   **Entities**: Represent domain models (e.g., `ServiceRequest`) containing business rules and attributes.
*   **Enums**: Define restricted option sets (e.g., `Priority`, `AssetStatus`).
*   **Constants**: Store system-wide immutable values (e.g., security roles and system permission keys).

### Persistence Layer (`ServiceDesk.Persistence`)
*   **Context**: Defines database context mapping to database tables.
*   **Repositories**: Implement data access operations (e.g., tracking changes, compiling queries, and sorting results).

### Infrastructure Layer (`ServiceDesk.Infrastructure`)
*   **File Storage**: Interacts with cloud containers (e.g., AWS S3, Azure Blob) to save and retrieve attachments.
*   **Email**: Connects to SMTP gateways to dispatch system notification emails.
*   **Background Jobs**: Manages background worker processes and scheduled tasks.

---

## 6. Dependency Flow

All dependency vectors point inward toward the Application and Domain projects. Outer infrastructure and Web API projects have no compile-time dependencies on concrete persistence libraries:

```
[ServiceDesk.WebApi] ──────> [ServiceDesk.Application] <────── [ServiceDesk.Infrastructure]
         │                               │                                 │
         v                               v                                 v
[ServiceDesk.Persistence] ──> [ServiceDesk.Domain] <───────────────────────┘
```

*   **Rule 1**: `Domain` is independent and references no other project.
*   **Rule 2**: `Application` references `Domain` and defines database and infrastructure interfaces.
*   **Rule 3**: `Persistence` and `Infrastructure` reference `Application` and implement those interfaces.
*   **Rule 4**: `WebApi` references `Application` and registers implementations at startup.

---

## 7. Request Processing Flow

The sequence diagram below shows how requests pass through the system:

```
Client         Controller         Middleware         Service         Repository         Database
  │                │                  │                 │                │                 │
  │── HTTP POST ──>│                  │                 │                │                 │
  │                │── Send to MW ───>│                 │                │                 │
  │                │                  │── Validate JWT ─│                │                 │
  │                │                  │                 │── Validate DTO │                 │
  │                │                  │                 │── Transaction ─│                 │
  │                │                  │                 │                │── Save changes ─│
  │                │                  │                 │                │<── Confirm ─────│
  │                │                  │                 │<── Return Data─│                 │
  │                │<── 201 Created ──│                 │                │                 │
  │<── JSON Res ───│                  │                 │                │                 │
```

---

## 8. Configuration Strategy

Configuration profiles are managed across environments using `appsettings.json` templates:

### JSON Configuration Example
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:sql.server.net;Database=ServiceDeskDB;User ID=app;Password=SecurePassword;"
  },
  "JwtSettings": {
    "Secret": "A_COMPLEX_SHA256_JWT_KEY_MINIMUM_32_BYTES_LONG",
    "Issuer": "https://api.servicedesk.company.com",
    "Audience": "https://servicedesk.company.com",
    "DurationMinutes": 15
  },
  "EmailSettings": {
    "Host": "smtp.mailtrap.io",
    "Port": 587,
    "Username": "smtp_user",
    "Password": "smtp_password",
    "SenderAddress": "no-reply@company.com"
  },
  "UploadSettings": {
    "ContainerName": "attachments",
    "MaxFileBytes": 10485760
  }
}
```

*   **Production Secrets**: Production database passwords and JWT secrets are injected using **Environment Variables** (e.g., `ConnectionStrings__DefaultConnection`) or cloud key managers (e.g., AWS Secrets Manager, Azure Key Vault).

---

## 9. Exception Handling Strategy

Implement a centralized exception handling middleware to catch errors globally and return standardized JSON responses:

```csharp
// Conceptual global error mapping logic
public async Task Invoke(HttpContext context)
{
    try {
        await _next(context);
    }
    catch (Exception ex)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        
        switch (ex)
        {
            case ValidationException valEx:
                response.StatusCode = 400;
                // Return errors list
                break;
            case UnauthorizedAccessException:
                response.StatusCode = 401;
                break;
            case KeyNotFoundException:
                response.StatusCode = 404;
                break;
            default:
                response.StatusCode = 500;
                // Log and return internal error message
                break;
        }
    }
}
```

---

## 10. Logging Strategy

Integrate **Serilog** as the logging provider, configured to output structured logs to both local files and centralized log consolidators (e.g., Elasticsearch, Seq, Datadog):

1.  **Application Logs**: Trace startup events, configuration binding status, and background thread activity.
2.  **Error Logs**: Capture stack traces, exception details, and related parameters for all `500 Internal Server Error` responses.
3.  **Audit Logs**: Log security-sensitive actions (e.g., login attempts, password updates, permissions changes) to the database audit table.
4.  **API Logs**: Log request paths, execution times, and return status codes.

---

## 11. Validation Strategy

Validation is enforced across multiple boundaries to ensure data integrity and security:

*   **Client Validation**: Implemented in React forms to check input types, formats, and required fields before sending requests.
*   **Server DTO Validation**: Enforced in the API controller using libraries like **FluentValidation** to check data formats, string lengths, and required fields.
*   **Business Rules Validation**: Enforced in the service layer (e.g., validating that HOD approvals are submitted by the correct department manager, or that open requests are not marked as completed without an assignee).
*   **Database Constraints**: Enforced by SQL Server database schema rules (e.g., unique constraints on email addresses and check constraints on enum fields).

---

## 12. File Upload Architecture

```
[React Client] ──(Stream file bytes)──> [API Controller]
                                               │
                                       (Security Check)
                                               │
[React Client] <──(Return Attachment URL)──────┴───> [Cloud Storage Container]
```

1.  **Security Filtering**: Restrict file uploads to allowed extensions (e.g., PNG, JPG, JPEG, PDF) and block executable files (e.g., EXE, MSI, DLL).
2.  **Name Sanitization**: Rename uploaded files to random GUIDs (e.g., `8f9b2d83.pdf`) to avoid naming collisions and file path injection attacks.
3.  **Storage Target**: Save files to cloud storage (e.g., AWS S3, Azure Blob) rather than local server directories.
4.  **Database Mapping**: Save metadata records (GUID, original filename, file size) to `dbo.ServiceRequestAttachments`.

---

## 13. Notification Architecture

To support in-app notifications and email alerts, the backend implements a split notification architecture:

```
                      +──(SignalR)──> [In-App Alert]
                      |
[Request State Change]
                      |
                      +──(Hangfire)──> [Dispatch Email Queue] ──> SMTP
```

*   **SignalR Integration**: Broadcast real-time alerts to the client UI (e.g., showing a notification bubble when a ticket is assigned to a technician).
*   **Email Queueing**: Queue outbound email alerts to a background worker to avoid blocking the main API thread.

---

## 14. Background Processing

Use **Hangfire** or **Quartz.NET** for background processing to run asynchronous tasks:

*   **Email Dispatching**: Process queued email notifications.
*   **Daily Activity Digests**: Compile daily summaries of service desk activity and send them to managers.
*   **SLA Warning Triggers**: Check for tickets approaching response limits and flag warnings.
*   **Data Retention Maintenance**: Delete soft-deleted records that are older than the retention period limit (e.g., 7 years).

---

## 15. Caching Strategy

Use **Redis** or in-memory caching to optimize performance for static data:

*   **Cache Targets**: Store frequently requested, static lookup tables (e.g., status definitions, service types, request types).
*   **Invalidation Rules**: Clear cached master tables when updates are made via configuration screens.
*   **Recommended Cache Lifetimes**: Set default cache lifetimes (e.g., 2 hours for request types, 24 hours for roles).

---

## 16. Performance Optimization

Enforce the following database and execution rules in Entity Framework Core:

*   **No Tracking Queries**: Use `AsNoTracking()` in read-only queries to bypass EF change tracking overhead.
*   **Explicit Projection**: Project query results directly to DTOs (`Select()`) to avoid fetching unused columns.
*   **Asynchronous Operations**: Use `async`/`await` for all database calls and I/O tasks to optimize thread pool utilization.
*   **Eager Loading**: Use eager loading (`Include()`) for related data to avoid N+1 query execution problems.
*   **Connection Pooling**: Configure DbContext connection pooling to reduce database connection overhead.

---

## 17. Scalability Strategy

The backend is designed for scaling from single instances to distributed cloud clusters:

*   **Stateless API Instances**: Keep API servers stateless by storing sessions and cache states externally (e.g., in Redis).
*   **Read/Write Replica Splitting**: Route database writes to the primary database and load-balance read queries across read replicas.
*   **Asynchronous Scaling**: Offload processing-heavy tasks (e.g., file generation, bulk reporting) to background workers.

---

## 18. Security Architecture Summary

Implement the following security controls at the API layer:

*   **Input Sanitization**: HTML-encode all incoming text fields to protect against XSS injections.
*   **CORS Policies**: Explicitly whitelist authorized frontend domains (e.g., `https://servicedesk.company.com`).
*   **Rate Limiting**: Apply rate-limiting policies to prevent denial-of-service (DoS) attacks.
*   **Secure File Validation**: Validate file signatures (magic bytes) to ensure file extensions match their true format content.

---

## 19. Deployment Architecture

```
[Client App] ──> [Cloudflare / CDN]
                        │
                        v
                 [Load Balancer]
                        │
         +--------------+--------------+
         │                             │
         v                             v
  [Docker Web API 1]            [Docker Web API 2]
         │                             │
         +--------------+--------------+
                        │
                        v
          [Primary SQL] ---> [Read Replica]
```

*   **Development**: Run Web API in Docker containers with local SQL Server instances.
*   **Staging/Production**: Deploy Web API to container services (e.g., AWS ECS, Azure App Service) behind load balancers with managed SQL databases.

---

## 20. Third-Party Integrations

*   **Email Gateway**: Mailgun, SendGrid, or AWS SES for transactional emails.
*   **SMS Service**: Twilio or Plivo for SMS alerts.
*   **Cloud Storage**: AWS S3 or Azure Blob Storage.
*   **Logging Service**: Seq, Datadog, or Elasticsearch.

---

## 21. Future Enhancements

*   **SignalR Integration**: Real-time ticket updates and notifications in the UI.
*   **Docker Containerization**: Containerize services using Docker Compose for easier deployment.
*   **SSO (Single Sign-On)**: Support Microsoft Entra ID or Google Workspace login.

---

## 22. Development Guidelines

*   **Naming Conventions**:
    *   C# Classes: PascalCase (e.g., `ServiceRequestsRepository`).
    *   API Routes: kebab-case (e.g., `/api/v1/service-requests`).
    *   Database Tables: Pluralized PascalCase (e.g., `ServiceRequests`).
*   **Git Branching**: Use a Git Flow workflow. Merge feature branches into `develop`, and deploy to production from `main`.
*   **Commit Message Convention**: Follow conventional commits rules (e.g., `feat: add request creation endpoint`, `fix: resolve JWT mapping exception`).

---

## 23. Complete Architecture Diagrams

### System Architecture
```
              +------------------------------------------+
              |               Cloudflare                 |
              +------------------------------------------+
                                   |
                                   v
              +------------------------------------------+
              |           Nginx Load Balancer            |
              +------------------------------------------+
                                   |
                +------------------+------------------+
                |                                     |
                v                                     v
  +---------------------------+         +---------------------------+
  |    Docker Web API Core    |         |    Docker Web API Core    |
  +---------------------------+         +---------------------------+
                |                                     |
                +------------------+------------------+
                                   |
                                   v
             +-------------------------------------------+
             |             Managed SQL Server            |
             |             (Primary Database)            |
             +-------------------------------------------+
```

### Layer Architecture
```
[ Web API Presentation ] ──> ( Maps / DTOs / Controllers )
         │
         v
[ Application Core ] ──> ( Interfaces / Services / Validators )
         │
         v
[ Persistence / Infra ] ──> ( EF Core Context / Blob Storage / SMTP Client )
         │
         v
[ Domain Model ] ──> ( Entities / Constants / Enums )
```

---

## 24. Backend Development Checklist

Use this checklist to track backend implementation tasks:

- `[ ]` **Phase 1: Project Setup**
  - Create the solution and project structure (`Domain`, `Application`, `Persistence`, `Infrastructure`, `WebApi`).
  - Add Entity Framework Core and database drivers to persistence projects.
  - Set up logging (Serilog) and configuration profiles (`appsettings.json`).
- `[ ]` **Phase 2: Database Migration**
  - Define domain entities and mappings in the EF DbContext.
  - Create and run the initial migration to generate the SQL Server database.
  - Write seed scripts for lookup tables (Roles, Statuses, Service Types, Departments).
- `[ ]` **Phase 3: Security & Auth**
  - Implement password hashing (BCrypt) and JWT token services.
  - Add auth controller endpoints (`/login`, `/signup`, `/refresh`).
  - Configure the CORS whitelist and authorization middleware policies.
- `[ ]` **Phase 4: Core Features**
  - Implement request validation logic using FluentValidation.
  - Implement service request lifecycle controllers and service layers.
  - Implement HOD approval workflows and auto-assignment rules.
  - Set up file upload services to cloud storage.
- `[ ]` **Phase 5: Background Tasks**
  - Integrate Hangfire and configure background email job queues.
  - Implement SignalR hubs for real-time notification broadcasts.
- `[ ]` **Phase 6: Testing & Deploy**
  - Write unit tests for business services and controller integration tests.
  - Containerize the Web API using Docker.
