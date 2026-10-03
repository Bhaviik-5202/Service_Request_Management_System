# Step 26 — Final System Architecture, Academic Viva Defense & Examination Guide

## 1. Executive Summary & System Overview

The **Service Request Management System (SRMS)** is an enterprise-grade IT Service Management (ITSM) web platform designed to streamline service requests, approval hierarchies, automated technician assignment, hardware/software asset management, and SLA tracking across an enterprise organization.

Built with **ASP.NET Core 10 Web API** and **React (Vite)**, the system conforms to industry-standard multi-tier architecture, clean separation of concerns, defensive security controls, and enterprise design patterns.

---

## 2. Multi-Tier Architecture & Design Patterns

```
┌────────────────────────────────────────────────────────────────────────┐
│                        PRESENTATION TIER (UI)                          │
│                   React (Vite) + Tailwind CSS + Lucide                 │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ HTTP / HTTPS (RESTful JSON)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                        API / HTTP GATEWAY TIER                         │
│  - RequestLoggingMiddleware (CorrelationId, Latency tracking)          │
│  - GlobalExceptionMiddleware (Centralized Error Schema, Status mapping)│
│  - Authentication Middleware (JWT Bearer Token Validation)             │
│  - Authorization Middleware (Role-Based Access Control - RBAC)         │
│  - Thin API Controllers (DTO Validation, HTTP Status Mapping)          │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Method Invocations (Interfaces)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                         BUSINESS SERVICE TIER                          │
│  - Domain Services (Auth, ServiceRequest, Approval, Asset, etc.)       │
│  - Business Validation & Workflow Rules (SLA, sequential numbering)   │
│  - In-Memory Caching Subsystem (ICacheService for Master lookups)      │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Repository Contracts
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                        DATA ACCESS TIER (DAL)                          │
│  - Generic Repository Pattern (IGenericRepository<T>, CRUD abstraction)│
│  - Unit of Work Pattern (IUnitOfWork, Single Atomic Transaction)       │
│  - Entity Framework Core 10 (AppDbContext, Fluent API, Soft Delete)    │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ TDS Protocol (Port 1433)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                          PERSISTENCE TIER                              │
│            Microsoft SQL Server 2022 (Normalized Relational DB)        │
└────────────────────────────────────────────────────────────────────────┘
```

### Applied Design Patterns:
1. **Repository Pattern (`IGenericRepository<T>`)**: Decouples business logic from EF Core querying mechanisms, centralizing query logic and enabling unit testability with in-memory data mocks.
2. **Unit of Work Pattern (`IUnitOfWork`)**: Manages database transactions across multiple repositories, guaranteeing atomicity (`CommitAsync`) during multi-entity business mutations.
3. **Dependency Injection (DI)**: Follows the Inversion of Control (IoC) principle; services and repositories are injected via scoped/singleton lifecycles.
4. **Middleware Pipeline Pattern**: Intercepts requests for logging (`RequestLoggingMiddleware`) and exceptions (`GlobalExceptionMiddleware`) before reaching application logic.
5. **Cache-Aside Pattern (`ICacheService`)**: Optimizes read-heavy static master data (Departments, Request Types, Statuses) with automatic cache invalidation upon create/update/delete.
6. **Data Transfer Object (DTO) Pattern**: Decouples internal database entities from external API contracts, preventing over-posting and mass-assignment vulnerabilities.

---

## 3. Security & RBAC Claims Matrix

| Feature / Endpoint | Admin | HOD / Approver | Technician | Requestor |
| :--- | :---: | :---: | :---: | :---: |
| **User Management** (`/api/Users/**`) | Full Access | No | No | No |
| **Master Data CRUD** (`/api/Masters/**`) | Full Access | Read Only | Read Only | Read Only |
| **Submit Service Request** (`POST /api/ServiceRequests`) | Yes | Yes | Yes | Yes |
| **Approve / Reject Requests** (`/api/Approvals/**`) | Override | Assigned Dept | No | No |
| **Assign Technician** (`PUT /api/ServiceRequests/{id}/assign`) | Full Access | Dept Requests | No | No |
| **Update Work Status / Resolution** | Yes | No | Assigned Only | No |
| **Reopen / Cancel Own Request** | Yes | No | No | Own Requests |
| **Asset Management** (`/api/Assets/**`) | Full Access | Read Dept Assets | Read/Maintain | Assigned To Me |
| **Audit Logs Inspection** (`/api/AuditLogs/**`) | Full Access | No | No | No |
| **Dashboard & Reports** (`/api/Dashboard/**`) | System-Wide | Departmental | Queue Metrics | Personal Metrics |

---

## 4. Service Request Lifecycle State Machine

```
   [ Draft / New ]
          │
          ▼
   (Submit Request)
          │
          ├─────────────────────────────────────────┐
          ▼ (RequiresApproval == false)             ▼ (RequiresApproval == true)
    [ Submitted ]                              [ Pending Approval ]
          │                                         │
          │                                 ┌───────┴───────┐
          │                                 ▼               ▼
          │                            (Rejected)      (Approved)
          │                                 │               │
          │                           [ Rejected ]          │
          │                                                 │
          └─────────────────────┬───────────────────────────┘
                                ▼
                        [ In Progress ] (Assigned to Technician)
                                │
                                ▼ (Resolution Submitted)
                         [ Resolved ]
                                │
                        ┌───────┴───────┐
                        ▼               ▼
                 (Confirm Close)    (Reopen Request)
                        │               │
                   [ Closed ]     [ In Progress ]
```

---

## 5. Comprehensive Academic Viva Defense Q&A

### Q1: Why did you choose ASP.NET Core Web API instead of traditional MVC?
> **Answer**: ASP.NET Core Web API allows complete separation of concerns between backend services and the frontend client. The backend functions as a pure RESTful JSON API, allowing any client (React web app, mobile app, or third-party service) to consume the same endpoints. This also facilitates independent deployment, caching, stateless horizontal scaling, and microservices readiness.

### Q2: What is the purpose of the Repository and Unit of Work patterns?
> **Answer**:
> - **Repository Pattern**: Abstracts data access mechanics away from business logic. Services never invoke `AppDbContext.Users.Add(...)` directly; they interact with `_unitOfWork.Users.AddAsync(...)`.
> - **Unit of Work Pattern**: Ensures that multiple repository operations share the same `DbContext` transaction. If an operation consists of creating a service request, assigning a technician, and logging an audit trail, calling `_unitOfWork.CommitAsync()` guarantees that either all operations succeed or all are rolled back.
> - **Testability**: It allows us to unit-test services and controllers completely isolated from SQL Server by mocking repository interfaces using `Moq`.

### Q3: How is Authentication and Role-Based Authorization implemented?
> **Answer**:
> - Authentication uses **JSON Web Tokens (JWT)** with the `HmacSha256` signing algorithm.
> - Upon login (`POST /api/Auth/login`), the backend validates credentials using `BCrypt` password hashing. If valid, `TokenService` issues a signed JWT containing claims: `NameIdentifier` (UserId), `Email`, `Name`, and `Role` (`Admin`, `HOD`, `Technician`, `Requestor`).
> - The ASP.NET Core `JwtBearerHandler` validates token integrity, expiration, issuer, and audience on each request.
> - Authorization is enforced declaratively using `[Authorize(Roles = "Admin,HOD")]` at the controller or action level.

### Q4: How do you handle unhandled exceptions across the application?
> **Answer**: We implemented a centralized `GlobalExceptionMiddleware`. Instead of messy try-catch blocks in every controller action, any unhandled domain or system exception bubbles up to this middleware. The middleware inspects the exception type:
> - `NotFoundException` -> HTTP 404
> - `ConflictException` -> HTTP 409
> - `UnauthorizedException` -> HTTP 401
> - `ForbiddenException` -> HTTP 403
> - `BusinessValidationException` -> HTTP 400
> - Generic `Exception` -> HTTP 500
> It returns a standardized `ApiResponseDto<T>` payload containing the error message, timestamp, and unique `CorrelationId` for error tracing.

### Q5: What is Correlation ID and why is it important in enterprise systems?
> **Answer**: A Correlation ID (`X-Correlation-ID`) is a unique UUID generated per HTTP request by our `RequestLoggingMiddleware`. It is attached to the HTTP response header and written to all structured log entries for that request. If a user encounters an error in production, they can share their Correlation ID, enabling DevOps/engineers to locate the exact log trail and root cause across all distributed layers instantly.

### Q6: How does caching improve performance in your application?
> **Answer**: Master data tables (such as Departments, Service Types, and Request Statuses) change infrequently but are read dozens of times per user session. We implemented `ICacheService` backed by `IMemoryCache` (Cache-Aside pattern). On reading master lookups, the service checks memory cache first. When an administrator creates, updates, or deletes a department, the cache key or prefix (`masters:departments`) is invalidated immediately, ensuring read performance without stale data.

### Q7: What are Global Query Filters in EF Core and how are they used?
> **Answer**: Global Query Filters are LINQ query predicates applied automatically to Entity types in `OnModelCreating`. In SRMS, we implemented soft delete via `builder.Entity<ServiceRequest>().HasQueryFilter(e => !e.IsDeleted)`. Any standard `GetAllAsync()` or LINQ query automatically appends `WHERE IsDeleted = 0` without needing manual filtering in every query. If an admin specifically requires deleted records, EF Core provides `.IgnoreQueryFilters()`.

### Q8: How is database concurrency and atomicity maintained?
> **Answer**: Atomicity is maintained through `IUnitOfWork.CommitAsync()`, which wraps entity changes in a single SQL Server transaction. Concurrency conflicts can be detected using EF Core concurrency tokens (or row versioning timestamps).

### Q9: How does the CI/CD pipeline ensure code quality?
> **Answer**: We configured a GitHub Actions workflow (`.github/workflows/ci.yml`). On every code push or pull request to the `main`/`master` branch, a clean Linux container automatically restores dependencies, builds the solution in `Release` mode, and executes all 39 automated unit and integration tests. If any test fails or compilation breaks, the pipeline fails and prevents deployment.

### Q10: How does Docker containerization benefit this system?
> **Answer**: Docker eliminates the "it works on my machine" problem. Our multi-stage `Dockerfile` produces a lightweight, secure production runtime container. `docker-compose.yml` orchestrates both the ASP.NET Core API and SQL Server 2022 database with automatic health checking, persistent volumes (`mssql_data`), and isolated network configuration, making system deployment repeatable in any environment with a single command (`docker compose up -d`).

---

## 6. Project Verification & Metric Summary

- **Total Academic Development Weeks**: 16 Weeks
- **Architecture**: N-Tier Clean Architecture with Repository & Unit of Work Patterns
- **Database Tables**: 16 Normalized Relational Entities with Soft Delete & Auditing
- **Automated Tests**: 39 Tests (Repositories, Services, Controllers, Middlewares, Caching, Health Checks) — **100% Pass Rate**
- **Security**: JWT Bearer + Role-Based Access Control + BCrypt Password Hashing
- **Containerization**: Multi-stage Docker + Docker Compose + Health Probes
- **CI/CD**: GitHub Actions Automated Build & Test Pipeline
