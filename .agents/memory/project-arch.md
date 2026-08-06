---
name: Project architecture
description: Tech stack, ports, entry points, and package manager for the SRMS project
---

## Frontend
- React 19 + Vite + TailwindCSS v4 + TanStack Router (file-based) + TanStack Query
- Entry: `frontend/src/main.jsx`
- Dev port: 5000 (Replit webview)
- Package manager: npm (run from `frontend/`)
- Workflow name: "Start application"

## Backend
- ASP.NET Core 9 Web API + Entity Framework Core + SQL Server
- Entry: `backend/Program.cs`
- Port: 5083
- Target framework: net9.0
- Workflow name: "Backend API"

## Key notes
- SQL Server connection string points to a local Windows SQLEXPRESS instance — requires a real SQL Server (cloud or local) to function
- CORS is set to AllowAnyOrigin (no credentials) — safe for JWT-based auth
- .NET module to install: dotnet-9.0
