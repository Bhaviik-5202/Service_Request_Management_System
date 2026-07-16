# Folder Structure & File Blueprint
## Service Request Management System

This document outlines the detailed folder organization, naming conventions, file maps, and project dependencies for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Solution Overview

The backend solution is organized into a **Clean Architecture** (Onion) project structure. It decouples the core domain models and business use cases from outer database providers, email services, background runners, and REST controllers.

The solution comprises five primary project layers and dedicated testing packages:
1.  **ServiceDesk.Domain**: Contains core entity definitions, constants, and custom business exceptions. Has zero dependencies.
2.  **ServiceDesk.Application**: Implements use case orchestration, DTO definitions, request validation rules, mappings, and abstraction interfaces.
3.  **ServiceDesk.Persistence**: Houses the DbContext, migration logs, and SQL Server repositories.
4.  **ServiceDesk.Infrastructure**: Implements file storage integrations, email gateways, and background jobs.
5.  **ServiceDesk.WebApi**: Serves as the API entrypoint containing controllers, middlewares, filters, and configuration bindings.

---

## 2. Complete Folder Tree

```
ServiceDesk/ (Solution Root)
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── README.md
├── ServiceDesk.sln
│
├── src/
│   ├── ServiceDesk.Domain/
│   │   ├── Constants/
│   │   │   ├── PermissionConstants.cs
│   │   │   └── SecurityRoles.cs
│   │   ├── Entities/
│   │   │   ├── Asset.cs
│   │   │   ├── Department.cs
│   │   │   ├── ServiceRequest.cs
│   │   │   └── User.cs
│   │   ├── Enums/
│   │   │   ├── Priority.cs
│   │   │   └── UserStatus.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── RequestLockedException.cs
│   │
│   ├── ServiceDesk.Application/
│   │   ├── Dtos/
│   │   │   ├── RequestDto.cs
│   │   │   └── UserDto.cs
│   │   ├── Interfaces/
│   │   │   ├── IInfrastructure/
│   │   │   │   ├── IBlobStorage.cs
│   │   │   │   └── IEmailService.cs
│   │   │   ├── IPersistence/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   └── IRequestRepository.cs
│   │   │   └── IServices/
│   │   │       └── IRequestService.cs
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs
│   │   ├── UseCases/
│   │   │   └── RequestService.cs
│   │   └── Validators/
│   │       └── CreateRequestValidator.cs
│   │
│   ├── ServiceDesk.Persistence/
│   │   ├── Context/
│   │   │   └── ServiceDeskDbContext.cs
│   │   ├── Migrations/
│   │   │   └── 20260716120000_InitialCreate.cs
│   │   └── Repositories/
│   │       └── RequestRepository.cs
│   │
│   ├── ServiceDesk.Infrastructure/
│   │   ├── BackgroundJobs/
│   │   │   └── HangfireScheduler.cs
│   │   ├── Email/
│   │   │   └── SmtpEmailService.cs
│   │   └── Storage/
│   │       └── AzureBlobStorage.cs
│   │
│   └── ServiceDesk.WebApi/
│       ├── Configurations/
│       │   ├── DependencyInjection.cs
│       │   └── JwtSettings.cs
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   └── RequestsController.cs
│       ├── Filters/
│       │   └── ValidateModelAttribute.cs
│       ├── Middlewares/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Resources/
│       │   └── EmailTemplates/
│       │       └── NotificationTemplate.html
│       ├── wwwroot/
│       │   └── uploads/
│       │       └── .gitkeep
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
├── tests/
│   ├── ServiceDesk.UnitTests/
│   │   ├── Domain/
│   │   └── Application/
│   └── ServiceDesk.IntegrationTests/
│       ├── Controllers/
│       └── TestUtilities/
│           └── DbFixture.cs
└── doc/
    ├── database_schema.pdf
    └── API_specs.md
```

---

## 3. Folder Responsibilities

### 3.1 Domain Project (`src/ServiceDesk.Domain`)
*   `Constants/`: Defines system-wide constant fields (e.g., role definitions, permissions).
*   `Entities/`: Models representing core business objects and state.
*   `Enums/`: Stores static selection types.
*   `Exceptions/`: Holds custom domain exception definitions.

### 3.2 Application Project (`src/ServiceDesk.Application`)
*   `Dtos/`: Data Transfer Objects defining API request and response models.
*   `Interfaces/`: Abstractions decoupling the core from database and infrastructure providers.
*   `Mappings/`: Configuration profiles for mapping between entity models and DTOs.
*   `UseCases/`: Orchestrates business workflows and use cases.
*   `Validators/`: Holds request validation rules (e.g., checks on sizes and formatting).

### 3.3 Persistence Project (`src/ServiceDesk.Persistence`)
*   `Context/`: House configuration details for Entity Framework Core DbContext mapping.
*   `Migrations/`: SQL migration script history tracks database versioning.
*   `Repositories/`: Concrete implementations of database repository interfaces.

### 3.4 Infrastructure Project (`src/ServiceDesk.Infrastructure`)
*   `BackgroundJobs/`: Implements workers and schedulers for async operations.
*   `Email/`: Implements SMTP notification dispatch rules.
*   `Storage/`: Coordinates file uploads and downloads with cloud container systems.

### 3.5 Web API Project (`src/ServiceDesk.WebApi`)
*   `Configurations/`: Extension classes to register dependencies and map appsettings settings.
*   `Controllers/`: Defines REST endpoints, maps routes, and handles requests.
*   `Filters/`: Global action filters (e.g., validating models state).
*   `Middlewares/`: Intercepts request pipelines (e.g., logging requests and handling exceptions globally).
*   `wwwroot/`: Serves static web assets and uploads locally.

---

## 4. File Responsibilities

| Target Component | Expected File Name | Purpose | Responsibility | Dependencies |
| :--- | :--- | :--- | :--- | :--- |
| **Entities** | `ServiceRequest.cs` | Models a ticket entity. | Manages ticket attributes. | None |
| **Enums** | `Priority.cs` | Ticket priority enums. | Restricts priority options. | None |
| **Exceptions** | `DomainException.cs` | Base domain exception. | Identifies business rule breaches. | None |
| **Constants** | `SecurityRoles.cs` | Defines security roles. | Standardizes string keys for roles. | None |
| **Interfaces** | `IRequestRepository.cs` | DB interface abstraction. | Declares request query operations. | `Domain.Entities` |
| **DTOs** | `RequestDto.cs` | Ticket request/response schema. | Sanitizes and maps request structures. | None |
| **Validators** | `CreateRequestValidator.cs` | Fluent validation rules. | Validates request payloads. | `Application.Dtos` |
| **Mappings** | `MappingProfile.cs` | AutoMapper profile. | Maps entities to and from DTOs. | `Domain`, `Application` |
| **Services** | `RequestService.cs` | Business service. | Orchestrates use cases and updates. | `Domain`, `Application.Interfaces` |
| **DbContext** | `ServiceDeskDbContext.cs` | EF database context. | Configures database schema mapping. | `Domain.Entities` |
| **Repositories** | `RequestRepository.cs` | Database repository. | Implements data access queries. | `Persistence.Context` |
| **Email** | `SmtpEmailService.cs` | SMTP email client. | Sends notification emails to users. | `Application.Interfaces` |
| **Controllers** | `RequestsController.cs` | API Controller class. | Handles HTTP request/response. | `Application.Interfaces` |
| **Middlewares** | `JwtMiddleware.cs` | Auth middleware class. | Extracts and validates JWT claims. | `Application.Interfaces` |

---

## 5. Naming Convention

| Resource Category | Naming Convention Style | Example |
| :--- | :--- | :--- |
| **Folders** | PascalCase | `BackgroundJobs`, `Repositories` |
| **Files** | PascalCase | `ServiceRequest.cs`, `SmtpEmailService.cs` |
| **Classes** | PascalCase | `RequestsController`, `CreateRequestValidator` |
| **Interfaces** | PascalCase, prefixed with `I` | `IRequestRepository`, `IEmailService` |
| **Methods** | PascalCase | `CreateRequestAsync`, `GetUserByIdAsync` |
| **Properties** | PascalCase | `RequestNumber`, `StatusName` |
| **Variables** | camelCase | `requestDto`, `userId` |
| **DTOs** | PascalCase, suffixed with `Dto` | `ServiceRequestDto`, `UserRegistrationDto` |
| **Controllers** | PascalCase, suffixed with `Controller` | `AuthController`, `RequestsController` |
| **API Routes** | kebab-case | `/api/v1/service-requests` |
| **Enums** | PascalCase | `UserStatus`, `AssetStatus` |
| **Constants** | PascalCase (or UPPERCASE) | `SecurityRoles.Admin`, `ADMIN_ROLE` |
| **Database Tables** | Pluralized PascalCase | `ServiceRequests`, `Users` |
| **Database Columns** | PascalCase | `RequestNo`, `PasswordHash` |
| **Foreign Keys** | Prefix `FK_`, `ChildTable_ParentTable` | `FK_ServiceRequests_Users_RequesterUserId` |
| **Indexes** | Prefix `IX_` (or `UIX_` for unique) | `IX_ServiceRequests_StatusId` |

---

## 6. Dependency Rules

All project dependencies flow inward. Projects in outer layers must not reference other outer layers (e.g., Web API cannot reference Persistence directly; it must go through the Application layer's interfaces):

```
+-------------------------------------------------------------+
|                     ServiceDesk.WebApi                      |
+-------------------------------------------------------------+
       │                             │
       │ (References)                │ (References)
       v                             v
+-----------------------+     +-------------------------------+
| ServiceDesk.Persistence| --> |    ServiceDesk.Application    |
+-----------------------+     +-------------------------------+
       │                                     │
       │ (References)                        │ (References)
       v                                     v
+-------------------------------------------------------------+
|                      ServiceDesk.Domain                     |
+-------------------------------------------------------------+
```

---

## 7. Configuration Files

1.  `appsettings.json`: Stores standard configuration settings (e.g. connection strings, JWT settings, email settings).
2.  `appsettings.Development.json`: Stores development-specific overrides (e.g., logging levels, local connection strings).
3.  `launchSettings.json`: Configures IIS Express and Kestrel startup profiles.
4.  `Program.cs`: Solution entrypoint. Configures middlewares and registers dependency injection services.
5.  `README.md`: Explains developer environment setup, build requirements, and migration guidelines.
6.  `.gitignore`: Prevents checking in user-specific or build directories (e.g., `/bin`, `/obj`, `.user`).
7.  `Directory.Build.props`: Enables global settings (e.g., nullable checks, warning treatment) across all projects in the solution.
8.  `.editorconfig`: Enforces code formatting rules (e.g., indent size, line endings, naming conventions).

---

## 8. Resource Organization

*   **Email Templates**: HTML email templates are stored in `src/ServiceDesk.WebApi/Resources/EmailTemplates/`.
*   **Static Uploads**: Files uploaded during development are saved locally in the `src/ServiceDesk.WebApi/wwwroot/uploads/` directory.
*   **System Logs**: Logs generated during execution are stored in the `/logs` directory at the solution root.

---

## 9. Documentation Structure

Maintain the following documentation files under the `doc/` directory:
*   `database_design.md`: Tracks the database schema design, index suggestions, and soft delete rules.
*   `api_endpoints.md`: Lists available REST endpoints, request/response payloads, and authentication rules.
*   `deployment_guide.md`: Details deployment pipelines, docker settings, and database migration steps.

---

## 10. Test Project Structure

```
tests/
├── ServiceDesk.UnitTests/
│   ├── Domain/
│   │   └── ServiceRequestTests.cs   # Verifies domain-specific logic and rules
│   └── Application/
│       └── RequestServiceTests.cs   # Verifies application services and mappings using mock dependencies
└── ServiceDesk.IntegrationTests/
    ├── Controllers/
    │   └── RequestsControllerTests.cs # Verifies API controller integration and responses
    └── TestUtilities/
        ├── DbFixture.cs             # Sets up test databases
        └── SeedData.cs              # Seeds lookup and mock data for tests
```

---

## 11. Git Organization

### Branching Strategy
*   `main`: Holds production-ready code.
*   `develop`: The main integration branch for development.
*   `feature/*`: Feature development branches (e.g., `feature/auth-login`, `feature/auto-assign`).
*   `bugfix/*`: Bug fix branches (e.g., `bugfix/attachment-size-limit`).

### Commit Message Format
Commit messages must follow the Conventional Commits specification:
`<type>(<scope>): <description>`
*   `feat`: A new feature (e.g., `feat(requests): add request creation endpoint`).
*   `fix`: A bug fix (e.g., `fix(auth): resolve token refresh expiration`).
*   `docs`: Documentation changes (e.g., `docs(readme): add environment variables setup`).
*   `test`: Adding or updating tests.

---

## 12. Recommended NuGet Package Categories

Use the following categories of NuGet packages to build the API backend:
1.  **Authentication**: JSON Web Token handler packages (e.g., JWT Bearer integration).
2.  **Logging**: Structured logging providers (e.g., Serilog integration).
3.  **Validation**: Model validation libraries (e.g., FluentValidation integration).
4.  **ORM**: Object-Relational Mapper packages (e.g., Entity Framework Core SQL Server provider).
5.  **API Documentation**: OpenAPI generators (e.g., Swagger integration).
6.  **Background Jobs**: Background worker managers (e.g., Hangfire).

---

## 13. Folder Creation Order

Follow this sequence to set up the solution folder structure during development:

1.  Create the solution root folder and initialize git: `git init`.
2.  Add global configuration files: `.gitignore`, `.editorconfig`, `Directory.Build.props`.
3.  Create the `/src` and `/tests` directories.
4.  Create the `ServiceDesk.Domain` project and add `/Entities`, `/Enums`, and `/Exceptions` folders.
5.  Create the `ServiceDesk.Application` project and add `/Interfaces`, `/Dtos`, `/Validators`, and `/UseCases` folders.
6.  Create the `ServiceDesk.Persistence` project and add `/Context` and `/Repositories` folders.
7.  Create the `ServiceDesk.Infrastructure` project and add `/Storage`, `/Email`, and `/BackgroundJobs` folders.
8.  Create the `ServiceDesk.WebApi` project and add `/Controllers`, `/Middlewares`, and `/Filters` folders.
9.  Set up unit and integration test projects under the `/tests` folder.

---

## 14. Development Checklist

Verify the following items before writing code:

- `[ ]` **Directory.Build.props**: Enabled global nullable context checks (`<Nullable>enable</Nullable>`).
- `[ ]` **Dependency Restrictions**: Checked that persistence projects do not directly reference Web API controllers, and domain projects reference no other projects.
- `[ ]` **Configurations**: Bound JWT settings and connection string structures to options classes in the Web API layer.
- `[ ]` **Tests Setup**: Configured the integration test project to run against a separate database.
- `[ ]` **EditorConfig**: Checked that the indentation styling rules match across the solution.
- `[ ]` **Git Check**: Verified that a `.gitignore` exists at the root to prevent checking in `/bin` and `/obj` build folders.
- `[ ]` **Static Uploads Directory**: Checked that the local uploads folder is ignored by git (`.gitkeep` added, `/uploads/*` added to `.gitignore`).
- `[ ]` **Template Organization**: Verified that placeholder templates exist in the resource folders.
- `[ ]` **Global Namespace Imports**: Added global imports (`Usings.cs`) to prevent duplicate references across files.
