# IT Service Request Management System

A full-featured IT Service Desk web application with role-based access control for Admins, HODs, Technicians, and Requestors.

## Project Structure

```
/
├── frontend/   # React + Vite + TailwindCSS v4 + TanStack Router
└── backend/    # ASP.NET Core 9 Web API + Entity Framework Core + SQL Server
```

## Tech Stack

### Frontend
- **Framework**: React 19 + Vite 8
- **Routing**: TanStack Router (file-based, auto-generated route tree)
- **Data Fetching**: TanStack Query v5
- **Styling**: TailwindCSS v4 + shadcn/ui (Radix UI primitives)
- **Forms**: React Hook Form + Zod
- **Charts**: Recharts
- **Package Manager**: npm (package-lock.json present)

### Backend
- **Framework**: ASP.NET Core 9 Web API
- **ORM**: Entity Framework Core (SQL Server)
- **Pattern**: Controller → DbContext (direct access; no service/repository layer yet)
- **Auth**: Custom login endpoint (JWT not yet implemented)

## Running the Project

### Frontend (standalone — uses mock data via `services/api.js`)
```bash
cd frontend
npm install
npm run dev
# App available at http://localhost:3000
```

### Backend (requires SQL Server)
```bash
cd backend
dotnet run
# API at http://localhost:5083
```
The backend connection string in `appsettings.json` points to a local Windows SQLEXPRESS instance. Update it for Replit (e.g. PostgreSQL or a cloud SQL Server).

## Demo Accounts (frontend mock data)

| Role       | Email                    | Password     |
|------------|--------------------------|--------------|
| Admin      | admin@gmail.com          | admin123     |
| HOD        | hod@gmail.com            | hod123       |
| Technician | tech@gmail.com           | tech123      |
| Requestor  | requestor@gmail.com      | requestor123 |

## Current State (as of analysis)

### Frontend
- `services/api.js` — 50+ API functions targeting `http://localhost:5083/api`; pages are largely wired to real endpoints
- `src/data/mock.js` — in-memory mock store (localStorage-backed); still used as fallback / seed data
- `components/shared/LandingPage.jsx` — fully static marketing page (hardcoded arrays)
- `lib/auth.jsx` — role permissions hardcoded in `ROLE_PERMISSIONS`; includes a dev-only `setRole` utility
- `components/requests/RequestForm.jsx` — file attachment does not upload to server (sets `URL: "#"`)

### Backend
- 20 controllers covering all entities (Users, Requests, Assets, Approvals, Notifications, etc.)
- **Missing**: JWT authentication (login returns plain user object; passwords compared in plain text)
- **Missing**: DTOs (entities returned directly from controllers)
- **Missing**: Service / repository layer (all logic in controllers)
- **Missing**: `[Authorize]` attributes on protected endpoints
- Soft-delete implemented inconsistently across controllers

## Planned Work (10-Phase Plan)

See attached `Pasted-You-are-a-Senior-Full-Stack-Software-Engineer...txt` for the full specification. High-level:

1. Analysis — dependency graph, dead code, unused assets
2. Cleanup — remove unused files, imports, variables, packages
3. Frontend Refactor — folder structure, reusable components, API layer
4. Backend Completion — service layer, DTOs, validation, middleware
5. Database — schema review, indexes, constraints, migrations
6. Remove Mock Data — all frontend pages must fetch from real APIs
7. API Integration — connect every CRUD operation to backend
8. Bug Fixes — console errors, runtime errors, auth/CORS issues
9. Code Quality — SOLID, DRY, clean architecture
10. Testing — verify builds, auth, CRUD, role access

## User Preferences

- Do NOT redesign the UI
- Do NOT change business logic
- Do NOT remove existing features
- Do NOT add unnecessary libraries
- Do NOT create dummy/mock data
- Preserve the existing project structure and stack
