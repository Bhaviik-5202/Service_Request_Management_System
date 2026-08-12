# Step 03 — Backend Project Setup & Solution Architecture

## 1. Objective

Set up the ASP.NET Core Web API solution architecture, clean layer structure, dependencies, NuGet packages, and configuration files.

---

## 2. Why This Step Is Required

A clean 4-layer solution architecture enforces Separation of Concerns (SoC). It keeps database entities decoupled from API controllers and isolates business logic, making the backend maintainable and testable.

---

## 3. Prerequisites

* Step 01 and Step 02 completed.
* .NET 8.0 SDK installed on development machine.

---

## 4. What Needs To Be Done

1. Create a solution folder structure (`ServiceDesk.Solution`).
2. Set up a 4-layer architecture:
   * `ServiceDesk.Core` (Domain Entities, Enums, DTOs, Interfaces)
   * `ServiceDesk.Infrastructure` (DbContext, EF Core Repositories, Migrations, Storage, Email Service)
   * `ServiceDesk.Application` (Business Logic Services, Auto-Assignment Engine, JWT Token Generator)
   * `ServiceDesk.API` (Controllers, Middleware, Swagger, CORS, Program.cs)
3. Add project references between layers following strict dependency rules:
   * `API` references `Application` & `Infrastructure`.
   * `Application` references `Core`.
   * `Infrastructure` references `Core` & `Application`.
   * `Core` has ZERO external project dependencies.
4. Plan necessary NuGet packages:
   * `Microsoft.EntityFrameworkCore.SqlServer`
   * `Microsoft.EntityFrameworkCore.Tools`
   * `Microsoft.AspNetCore.Authentication.JwtBearer`
   * `BCrypt.Net-Next` (Password hashing)
   * `FluentValidation.AspNetCore`

---

## 5. Project-Specific Requirements

* **Application Name**: Service Request Management System API
* **Base Namespace**: `ServiceDesk.*`
* **Local API URL**: `https://localhost:7124/api/v1`
* **Static Storage Directory**: Create `wwwroot/uploads` directory in the API project for local attachment storage.

---

## 6. Important Decisions

* **Clean Layering**: Maintain a lightweight 4-project solution structure without over-engineering complex enterprise patterns.
* **CORS Policy**: Configure CORS middleware in `Program.cs` to allow requests from the React frontend development origin (`http://localhost:5173` or `http://localhost:3000`).

---

## 7. Dependencies

* **Upstream**: Step 02 (Final Requirements).
* **Downstream**: Step 04 (Database Design), Step 05 (Models and Entities).

---

## 8. Expected Result

After completing this step:

✓ ASP.NET Core solution structure is created with 4 clean project layers.
✓ Project references and NuGet dependencies are configured.
✓ CORS policy and API versioning parameters are planned.

---

## 9. Verification Checklist

- [ ] Created 4 solution projects (`Core`, `Infrastructure`, `Application`, `API`).
- [ ] Set up project references according to dependency rules.
- [ ] Added required EF Core and JWT Bearer NuGet packages.
- [ ] Planned CORS middleware policy for React SPA origin.
- [ ] Created static uploads directory (`wwwroot/uploads`).

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [04_Database_Design.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/04_Database_Design.md)**
