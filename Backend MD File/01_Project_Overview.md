# Step 01 — Project Overview & Architecture

## 1. Objective

Understand the overall scope, purpose, architecture, and technology stack of the **Service Request Management System** backend and its integration with the existing React frontend.

---

## 2. Why This Step Is Required

Before writing any backend code, you must understand what the application builds, who the key users are, how the system is structured, and what technologies will be used. This eliminates architectural confusion later during development.

---

## 3. Prerequisites

* Review the React frontend codebase (`frontend/src/`).
* Review the existing project files and requirements.
* Install .NET 8.0 SDK (or latest LTS) and SQL Server.

---

## 4. What Needs To Be Done

1. Review the application purpose: a multi-role enterprise IT & Facility Service Desk.
2. Confirm the technology stack: ASP.NET Core Web API, C#, Entity Framework Core, SQL Server, JWT Authentication.
3. Identify the 4 core user roles: `Admin`, `HOD`, `Technician`, `Requestor`.
4. Establish the single source of truth rule: **Current React Frontend > Old Planning Docs**.
5. Understand the stateless REST API communication model with the React SPA.

---

## 5. Project-Specific Requirements

* **Primary Function**: Internal corporate ticketing and service request management.
* **Target Audience**:
  * **Requestors**: Employees raising tickets for IT, maintenance, software, hardware, or access needs.
  * **Technicians**: IT and maintenance staff assigned to resolve tickets.
  * **HODs (Heads of Department)**: Department managers responsible for approving high-priority/procurement requests and managing department personnel.
  * **Admins**: System administrators managing master configurations, user accounts, and viewing reports.
* **Core Domains**: Service Requests, Approvals, Assignments, Master Configurations, Assets Inventory, Notifications, Audit Logs, Reports & Analytics.

---

## 6. Important Decisions

* **Stateless API Design**: Use JWT Bearer authentication to keep the ASP.NET Core API stateless and decoupled from the React frontend.
* **No Code in Documentation**: This step-by-step guide provides technical blueprints, data structures, and verification steps. All C# and SQL implementation code will be written by you.
* **REST Standards**: API routes will follow standard REST conventions (`/api/v1/resource`) with standard HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`).

---

## 7. Dependencies

* **Upstream**: None (First step).
* **Downstream**: Step 02 (Final Requirements), Step 03 (Project Setup).

---

## 8. Expected Result

After completing this step:

✓ Project scope and architecture are fully understood.
✓ Technology stack is confirmed.
✓ Four system roles (`Admin`, `HOD`, `Technician`, `Requestor`) are identified.
✓ System boundary between React frontend and ASP.NET Core backend is established.

---

## 9. Verification Checklist

- [ ] Reviewed React frontend structure in `frontend/src/`.
- [ ] Confirmed ASP.NET Core Web API + SQL Server stack choice.
- [ ] Confirmed 4 system roles (`Admin`, `HOD`, `Technician`, `Requestor`).
- [ ] Confirmed JWT authentication strategy for SPA client.
- [ ] Understood that frontend source code is the primary source of truth.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [02_Final_Requirements.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/02_Final_Requirements.md)**
