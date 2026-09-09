# Step 07 — DbContext & EF Core Fluent API Planning

## 1. Objective

Plan the Entity Framework Core `DbContext` configuration, `DbSet<T>` property mappings, Fluent API builder configurations, decimal precision rules, and global query filters in `ServiceDesk.Infrastructure`.

---

## 2. Why This Step Is Required

Explicit EF Core Fluent API configurations prevent default convention errors in SQL Server migrations, such as truncated decimal values, incorrect foreign key cascade behaviors, or unindexed search queries.

---

## 3. Prerequisites

* Step 06 (Relationships and Constraints) completed.

---

## 4. What Needs To Be Done

1. Plan `ServiceDeskDbContext` class inheriting from `DbContext`.
2. Map 16 `DbSet<T>` properties corresponding to the 16 core entities.
3. Plan `OnModelCreating` configuration rules:
   * **Table Mapping**: Configure exact table names (`dbo.Users`, `dbo.ServiceRequests`, etc.).
   * **Decimal Precision**: Configure `HasColumnType("decimal(18,2)")` for `Asset.BookValue`.
   * **Enum Conversions**: Configure `HasConversion<string>()` to store enums (`UserRole`, `Priority`, `AssetStatus`, `ApprovalStatus`, `NotificationType`) as readable strings in SQL Server.
   * **Global Query Filters**: Configure `HasQueryFilter(e => !e.IsDeleted)` on soft-deletable entities (`Users`, `Departments`, `DepartmentPersonnel`, `ServiceTypes`, `RequestTypes`, `RequestTypeTechnicianMappings`, `ServiceRequests`, `Assets`).
   * **Filtered Indexes**: Configure non-clustered filtered indexes on `Email` and `RequestNumber` `WHERE IsDeleted = 0`.

---

## 5. Project-Specific Requirements

* **DbContext Namespace**: `ServiceDesk.Infrastructure.Context`
* **Connection String Key**: `ConnectionStrings:DefaultConnection` in `appsettings.json`.

---

## 6. Important Decisions

* **Global Soft-Delete Filter**: EF Core global query filters ensure soft-deleted records (`IsDeleted = 1`) are automatically omitted from standard LINQ queries across the entire application without needing manual `WHERE IsDeleted = 0` clauses in every repository method.

---

## 7. Dependencies

* **Upstream**: Step 06 (Relationships and Constraints).
* **Downstream**: Step 08 (Database Migration & Seed Data).

---

## 8. Expected Result

After completing this step:

✓ `DbContext` mapping rules for all 16 entities are planned.
✓ Global query filters for soft delete are planned.
✓ Decimal precision and enum string conversions are configured.

---

## 9. Verification Checklist

- [ ] Planned 16 `DbSet<T>` properties.
- [ ] Configured `decimal(18,2)` column type for `Asset.BookValue`.
- [ ] Planned `HasConversion<string>()` for 5 C# string enums.
- [ ] Planned global soft-delete query filters (`!e.IsDeleted`).
- [ ] Planned filtered indexes for unique fields.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [08_Database_Migration_and_Seed.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/08_Database_Migration_and_Seed.md)**
