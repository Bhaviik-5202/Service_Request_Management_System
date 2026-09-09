# Step 08 — Database Migration & Seed Data Planning

## 1. Objective

Plan EF Core database migrations, SQL database creation, and seed data initialization for baseline departments, service types, request types, statuses, auto-assignment rules, and seed users.

---

## 2. Why This Step Is Required

Creating initial database migrations and seeding master data ensures that when the ASP.NET Core API starts, all required lookup tables (`ServiceRequestStatuses`, `Departments`, `ServiceTypes`, `RequestTypes`) contain valid records matching the frontend dropdowns.

---

## 3. Prerequisites

* Step 07 (DbContext Planning) completed.
* SQL Server instance running and connection string configured.

---

## 4. What Needs To Be Done

1. Plan initial EF Core migration command (`InitialCreate`).
2. Plan database update execution (`Update-Database` or `dotnet ef database update`).
3. Plan Master Seed Data matching `frontend/src/data/mock.js`:
   * **Statuses**: `Open`, `In Progress`, `Pending Approval`, `Resolved`, `Closed`, `Rejected` (with Tailwind badge color codes).
   * **Departments**: `IT`, `Maintenance`, `Housekeeping`, `Admin`, `Security`.
   * **Service Types**: `Technical`, `Facility`, `Administrative`.
   * **Request Types**: `Computer Issue`, `Software Request`, `Hardware Request`, `Network Issue`, `Email Issue`, `Printer Issue`, `Access Request`, `AC Repair`, `Plumbing`, `AV Equipment`, `Security Systems`, `Cleaning`, `Furniture`, `Onboarding`.
   * **Department Personnel**: Map seed users to departments as HODs or Technicians.
   * **Request Type Mappings**: Map Request Types to default technicians for auto-routing.
   * **Seed Users**:
     * Admin (`admin@gmail.com`)
     * Technicians (`tech@gmail.com`, `anita.desai@company.com`, `suresh.kumar@company.com`, `meena.joshi@company.com`)
     * HODs (`hod@gmail.com`, `divya.nair@company.com`)
     * Requestors (`requestor@gmail.com`, `neha.gupta@company.com`)

---

## 5. Project-Specific Requirements

* **Seed Seeding Location**: Infrastructure DB initializer extension class (`DbInitializer.cs`).
* **Idempotent Execution**: Seed methods must check `!context.ServiceRequestStatuses.Any()` before inserting records to avoid duplicate key errors on application restarts.

---

## 6. Important Decisions

* **Seed User Passwords**: Password hashes for seed users must be generated using `BCrypt.HashPassword("123456")` to allow instant testing with the pre-filled credentials from the React login screen.

---

## 7. Dependencies

* **Upstream**: Step 07 (DbContext Planning).
* **Downstream**: Step 09 (Authentication & Identity), Step 10 (Users & Settings).

---

## 8. Expected Result

After completing this step:

✓ Initial migration `InitialCreate` is generated.
✓ SQL Server database `ServiceDeskDb` is created with 16 tables.
✓ Master lookup tables and seed users are populated.

---

## 9. Verification Checklist

- [ ] Planned initial migration command.
- [ ] Confirmed database creation in SQL Server.
- [ ] Planned seed data for 6 statuses matching frontend colors.
- [ ] Planned seed data for 5 departments and 14 request types.
- [ ] Planned seed users with BCrypt password hashes.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [09_Authentication_and_Identity.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/09_Authentication_and_Identity.md)**
