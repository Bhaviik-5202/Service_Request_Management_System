# Backend Implementation Roadmap
## Service Request Management System

This document outlines the step-by-step implementation roadmap for building the **Service Request Management System** backend using **ASP.NET Core Web API** and **Entity Framework Core**.

---

## 1. Project Preparation

### Software Requirements
*   **.NET SDK**: Version 8.0 or 9.0 (LTS).
*   **IDE**: Visual Studio 2022 (Community/Professional/Enterprise) or JetBrains Rider.
*   **Database Server**: SQL Server 2022 Developer Edition (or LocalDB).
*   **Database Client**: SQL Server Management Studio (SSMS) or Azure Data Studio.
*   **Version Control**: Git.

### Recommended Extensions (Visual Studio / VS Code)
*   **C# Dev Kit**: Implements code assistance, testing, and debugging.
*   **EF Core Power Tools**: Generates entity diagrams and database configurations.
*   **SonarLint**: Real-time code quality, security, and styling checking.
*   **Rest Client / Postman**: Test API endpoints from the developer workspace.

### Environment Setup
1.  Configure Local database connection string in user secrets.
2.  Install the EF Core CLI tool globally:
    `dotnet tool install --global dotnet-ef`

---

## 2. Development Phases

The project is divided into 11 development phases:

```
[Phase 1: Init] ──> [Phase 2: Database] ──> [Phase 3: Auth] ──> [Phase 4: Masters] 
                                                                       │
                                                                       v
[Phase 7: Alerts] <── [Phase 6: Uploads] <── [Phase 5: Requests] <─────┘
       │
       v
[Phase 8: Dashboard] ──> [Phase 9: Reports] ──> [Phase 10: Tests] ──> [Phase 11: Deploy]
```

---

## 3. Phase-Wise Tasks Specifications

### Phase 1: Project Initialization
*   **Goal**: Set up the Clean Architecture solution and projects structure.
*   **Objectives**: Define assemblies, configurations, and global imports.
*   **Dependencies**: Git setup, .NET SDK.
*   **Completion Criteria**: Solution builds successfully with zero warnings.

### Phase 2: Database & Domain Modeling
*   **Goal**: Create tables, relationships, and seed initial lookup data.
*   **Objectives**: Define DbContext, write entity configurations, and run migrations.
*   **Dependencies**: Phase 1.
*   **Completion Criteria**: Lookup records (Roles, Statuses, Service Types, Departments) are successfully seeded in SQL Server.

### Phase 3: Authentication & Security
*   **Goal**: Implement secure user registration, login, and token generation.
*   **Objectives**: Set up JWT auth middleware, implement password hashing (BCrypt), and configure password policies.
*   **Dependencies**: Phase 2.
*   **Completion Criteria**: Auth endpoints generate valid JWT access tokens and set secure refresh cookies.

### Phase 4: Master Modules
*   **Goal**: Create CRUD APIs for administration master tables.
*   **Objectives**: Implement status, department, and category endpoints.
*   **Dependencies**: Phase 3.
*   **Completion Criteria**: Admins can configure statuses, departments, and technician mappings.

### Phase 5: Service Requests Lifecycle
*   **Goal**: Implement service request workflows and ticket operations.
*   **Objectives**: Create endpoints for ticket creation, assignments, and HOD approvals.
*   **Dependencies**: Phase 4.
*   **Completion Criteria**: Requesters can create tickets, technicians can change statuses, and HODs can approve requests.

### Phase 6: File Upload Integration
*   **Goal**: Set up file upload services to cloud storage.
*   **Objectives**: Implement file validation rules and storage service clients.
*   **Dependencies**: Phase 5.
*   **Completion Criteria**: Users can attach documents and images (up to 10MB) to tickets and replies.

### Phase 7: Real-Time Notifications
*   **Goal**: Set up user notifications and email alerts.
*   **Objectives**: Configure SignalR hubs and queue email alerts.
*   **Dependencies**: Phase 5.
*   **Completion Criteria**: Status updates and assignments trigger real-time UI notifications.

### Phase 8: Dashboard Analytics
*   **Goal**: Implement summary endpoints for role-based dashboards.
*   **Objectives**: Create summary stats and activity feed endpoints.
*   **Dependencies**: Phase 5.
*   **Completion Criteria**: Returns request counts and activity logs based on the user's role.

### Phase 9: Reports & Data Exports
*   **Goal**: Build performance reports and data export endpoints.
*   **Objectives**: Create endpoints for CSV/XLSX downloads and resolution trend metrics.
*   **Dependencies**: Phase 5.
*   **Completion Criteria**: Admins can export ticket logs and view department performance charts.

### Phase 10: Automated Testing
*   **Goal**: Verify business rules and API endpoints using automated tests.
*   **Objectives**: Write unit tests for services and integration tests for API controllers.
*   **Dependencies**: Phase 9.
*   **Completion Criteria**: Passes test suite coverage checks (e.g., minimum 80% business layer coverage).

### Phase 11: Deployment Preparation
*   **Goal**: Containerize and prepare the API for production deployment.
*   **Objectives**: Create Dockerfiles and set up CI/CD pipelines.
*   **Dependencies**: Phase 10.
*   **Completion Criteria**: Docker containers build successfully and run on staging environments.

---

## 4. Development Implementation Order

The recommended sequence for manual development:

```
Create Solution & Projects -> Create Folder Structures -> Define Domain Entities
                                                                  │
                                                                  v
Seed Seed Lookup Data <------- Run Migrations <--------- Configure DbContext
          │
          v
Implement Auth Logic -------> Configure JWT Middleware -> Create Master CRUD APIs
                                                                  │
                                                                  v
Verify Use Case Tests <------- Implement API Controllers <--- Build Request Services
```

---

## 5. Estimated Effort & Complexity Matrix

| Phase | Description | Difficulty | Complexity | Priority | Risk Level |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Phase 1** | Project Initialization | Low | Low | Critical | Low |
| **Phase 2** | Database & Domain | Medium | Medium | Critical | Medium |
| **Phase 3** | Authentication | High | High | Critical | High |
| **Phase 4** | Master Modules | Low | Low | High | Low |
| **Phase 5** | Requests Lifecycle | High | High | Critical | High |
| **Phase 6** | File Upload | Medium | Medium | Medium | Medium |
| **Phase 7** | Notifications | Medium | High | Medium | Medium |
| **Phase 8** | Dashboard | Low | Medium | High | Low |
| **Phase 9** | Reports | Medium | Medium | Medium | Low |
| **Phase 10**| Testing | Medium | High | High | Low |
| **Phase 11**| Deployment | Medium | Medium | High | Medium |

---

## 6. Module Dependency Map

The diagram below maps dependencies between core modules:

```
    [Authentication]
           │
           v
    [Master Lookups] <─── [Admin Mappings]
           │
           v
    [Service Requests] <─── [Approvals]
           │
           +─────────────────+─────────────────+
           │                                   │
           v                                   v
     [Attachments]                      [Notifications]
```

*   **Authentication** secures subsequent API endpoints.
*   **Master Lookups** provide category and status parameters for requests.
*   **Service Requests** require **Approvals** for restricted categories.
*   **Attachments** and **Notifications** depend on request entities.

---

## 7. Project Milestones

*   **Milestone 1 (Solution Foundation)**: Clean architecture projects configured and building successfully.
*   **Milestone 2 (Database Schema)**: Database migrations completed and initial data seeded.
*   **Milestone 3 (Secured Identity)**: User registration and JWT login endpoints functional.
*   **Milestone 4 (Operational Ticket Workflows)**: Request creation, assignment, and status transitions functional.
*   **Milestone 5 (Approvals & Files)**: HOD approval workflows and file upload integration completed.
*   **Milestone 6 (Analytics & Logs)**: Dashboard counters and CSV export endpoints functional.
*   **Milestone 7 (Production Ready)**: Docker container testing completed.

---

## 8. Definition of Done (DoD)

A phase is considered complete when it meets the following criteria:
1.  **Build**: Code compiles successfully with zero warnings.
2.  **Linting**: Static analysis rules pass without code smell flags.
3.  **Tests**: Unit and integration tests pass with minimum 80% coverage.
4.  **Security**: Vulnerability scans show no open threats.
5.  **Review**: Code review approved by peer developer.
6.  **Docs**: Swagger UI documents all endpoints.

---

## 9. Quality Checklist

Verify the following items before moving to the next phase:
*   [ ] Checked that no business logic exists inside API controllers.
*   [ ] Verified that database connection passwords are not hardcoded.
*   [ ] Checked that all asynchronous operations use `async` and `await`.
*   [ ] Verified that FluentValidation models validate incoming payload fields.
*   [ ] Checked that all entities implement soft delete parameters.

---

## 10. Common Mistakes to Avoid

1.  **Tight Coupling**: Avoid referencing database contexts directly inside controllers. Enforce boundaries using the Application layer interfaces.
2.  **Hardcoded Strings**: Avoid hardcoding roles and permissions. Use static constant definitions (e.g., `SecurityRoles.Admin`).
3.  **Sync Database Queries**: Avoid using synchronous database methods (e.g., using `ToList()` instead of `ToListAsync()`), which can block the thread pool under load.
4.  **Inefficient Queries**: Avoid fetching full database rows when only key fields are needed. Use projections (`Select()`) to optimize queries.

---

## 11. Technical Risk Management

### Risk: Multiple Database Cascade Paths
*   *Detail*: SQL Server returns an error when multiple foreign keys attempt to cascade deletes down to a child table.
*   *Mitigation*: Enforce `ON DELETE NO ACTION` for foreign keys pointing to parent tables and rely on soft deletes.

### Risk: File Upload Vulnerabilities
*   *Detail*: Users could upload malicious files (e.g. executable scripts) to the server.
*   *Mitigation*: Restrict file extensions, scan uploads, and store files securely in cloud storage.

---

## 12. Daily Development Schedule

### Week 1: Foundation & Identity
*   **Day 1**: Set up projects, configure folder structures, and configure dependency injection.
*   **Day 2**: Create domain entities and configure entity properties in DbContext.
*   **Day 3**: Run database migrations and write seed scripts for lookup tables.
*   **Day 4**: Implement registration, password hashing, and JWT login services.
*   **Day 5**: Implement token refresh endpoints and configure JWT authentication middleware.

### Week 2: Workflows & Reports
*   **Day 6**: Implement CRUD APIs for master tables (departments, categories, statuses).
*   **Day 7**: Implement service request creation endpoints and auto-assignment rules.
*   **Day 8**: Implement ticket operations (assignments, status updates, approvals, comments).
*   **Day 9**: Set up file upload services to cloud storage.
*   **Day 10**: Set up SignalR notification hubs.

### Week 3: Dashboard & Deployment
*   **Day 11**: Create dashboard statistics and activity log endpoints.
*   **Day 12**: Create reports and CSV export endpoints.
*   **Day 13**: Write unit and integration tests.
*   **Day 14**: Containerize the API using Docker.
*   **Day 15**: Deploy to staging environments.

---

## 13. Week-by-Week Roadmap

*   **Week 1**: Complete Phase 1 (Init), Phase 2 (Database), and Phase 3 (Auth).
*   **Week 2**: Complete Phase 4 (Masters), Phase 5 (Requests), Phase 6 (Uploads), and Phase 7 (Notifications).
*   **Week 3**: Complete Phase 8 (Dashboard), Phase 9 (Reports), Phase 10 (Testing), and Phase 11 (Deployment).

---

## 14. Prerequisites & Learning Topics

*   **Before Phase 1 & 2**: Read up on Clean Architecture principles and EF Core entity configurations.
*   **Before Phase 3**: Learn JWT token structures, claims configuration, and symmetric encryption keys.
*   **Before Phase 5**: Read up on transaction management in Entity Framework.
*   **Before Phase 7**: Learn SignalR hubs and real-time client communication methods.
*   **Before Phase 11**: Learn Docker multi-stage builds.

---

## 15. Debugging Strategy

*   **Local Debugging**: Set log levels to `Debug` or `Information` in development settings to trace requests in the console.
*   **Query Inspection**: Use logging configurations to output SQL queries generated by EF Core to the console during development.
*   **Error Responses**: Check validation failures in response payloads from Swagger or Postman.

---

## 16. Testing Strategy

*   **Unit Tests**: Test core domain rules and application services using mock frameworks (e.g., NSubstitute, Moq) to isolate dependencies.
*   **Integration Tests**: Run integration tests against a test database instance to verify database queries and transaction states.
*   **Load Testing**: Run load tests on key endpoints (e.g., logins, request creations) to verify response times under load.

---

## 17. Documentation Checklist

During development, update the following documentation:
*   [ ] **Swagger UI**: Ensure XML documentation comments are enabled on all controllers to generate API documentation.
*   [ ] **README.md**: Document steps to run database migrations locally.
*   [ ] **Environment Variables**: Maintain a list of configuration keys needed for staging and production deployments.

---

## 18. Final Readiness Checklist

Verify the following before deploying to production:
*   [ ] Enforced HTTPS connections on all API endpoints.
*   [ ] Configured CORS policies to restrict access to authorized frontend domains.
*   [ ] Changed all default JWT secrets and database passwords in production settings.
*   [ ] Configured database connection pooling and query performance tracking.
*   [ ] Configured automated database backups.
*   [ ] Tested backup recovery procedures.
*   [ ] Set up application performance monitoring (APM) tools.

---

## 19. Future Improvements Roadmap

*   **Multi-Region Database Replication**: Scale read operations globally.
*   **Single Sign-On (SSO)**: Add support for enterprise authentication providers.
*   **Knowledge Base**: Integrate a searchable database of help articles.

---

## 20. Complete Development Timeline

```
Week 1: Foundation & Identity (Milestone 1, 2, 3)
  ├── Day 1: Project setup & configurations
  ├── Day 2: Domain entities & database configurations
  ├── Day 3: Migrations & lookup seed scripts
  ├── Day 4: User registration & JWT logins
  └── Day 5: Token refreshes & auth middleware configuration

Week 2: Workflows & Reports (Milestone 4, 5)
  ├── Day 6: Master table CRUD APIs
  ├── Day 7: Request creation & assignment rules
  ├── Day 8: Ticket status transitions & approvals
  ├── Day 9: File upload integration
  └── Day 10: SignalR notification hubs

Week 3: Dashboard & Deploy (Milestone 6, 7)
  ├── Day 11: Dashboard statistics
  ├── Day 12: Reports & CSV exports
  ├── Day 13: Unit & integration testing
  ├── Day 14: Docker containerization
  └── Day 15: Staging deployment & validation
```
