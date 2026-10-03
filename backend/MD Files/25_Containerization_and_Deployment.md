# Step 25 — Containerization, Health Checks & CI/CD Pipeline

## 1. Objective

Provide a production-ready containerization and automated deployment configuration for the **Service Request Management System (SRMS)**, including:
1. Multi-stage Docker containerization for the ASP.NET Core Web API.
2. Full-stack Docker Compose orchestration for API and Microsoft SQL Server 2022.
3. Automated ASP.NET Core Health Check monitoring via `/health`.
4. Automated GitHub Actions Continuous Integration (CI) pipeline executing automated build and test runs.

---

## 2. Architectural Blueprint

```
                      [ GitHub Repository ]
                                │
                    (Git Push / Pull Request)
                                ▼
                   ┌─────────────────────────┐
                   │  GitHub Actions CI/CD   │
                   │  - Setup .NET 10        │
                   │  - Cache NuGet packages │
                   │  - dotnet restore       │
                   │  - dotnet build Release │
                   │  - dotnet test (39 P.)  │
                   └────────────┬────────────┘
                                │ (Pass)
                                ▼
                 ┌─────────────────────────────┐
                 │  Docker Compose Environment │
                 │                             │
                 │  ┌───────────────────────┐  │
                 │  │      srms-api         │  │
                 │  │ (ASP.NET Core 10 Web) │  │
                 │  │ Port: 5158 / 8080     │  │
                 │  │ Healthcheck: /health  │  │
                 │  └───────────┬───────────┘  │
                 │              │              │
                 │     (Private Bridge Net)    │
                 │              │              │
                 │  ┌───────────▼───────────┐  │
                 │  │     srms-sqlserver    │  │
                 │  │ (MS SQL Server 2022)  │  │
                 │  │ Port: 1433            │  │
                 │  │ Volume: mssql_data    │  │
                 │  └───────────────────────┘  │
                 └─────────────────────────────┘
```

---

## 3. Container Specifications

### 3.1 ASP.NET Core Multi-Stage Dockerfile (`backend/Dockerfile`)
The Dockerfile uses a 4-stage build to achieve minimal image size and fast build layer caching:

```dockerfile
# Stage 1: Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 5158
ENV ASPNETCORE_URLS=http://+:5158;http://+:8080

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ServiceRequestManagementSystem.API/ServiceRequestManagementSystem.API.csproj", "ServiceRequestManagementSystem.API/"]
RUN dotnet restore "ServiceRequestManagementSystem.API/ServiceRequestManagementSystem.API.csproj"

COPY . .
WORKDIR "/src/ServiceRequestManagementSystem.API"
RUN dotnet build "ServiceRequestManagementSystem.API.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "ServiceRequestManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceRequestManagementSystem.API.dll"]
```

### 3.2 Docker Compose Orchestration (`docker-compose.yml`)
Orchestrates the backend API alongside SQL Server 2022 with automatic dependency ordering and health checking:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: srms-sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "YourStrong@Password123!"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Password123!' -Q 'SELECT 1' -C || /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Password123!' -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 15s
    networks:
      - srms-network

  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: srms-api
    restart: unless-stopped
    ports:
      - "5158:5158"
      - "8080:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: "Production"
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=ServiceRequestDB;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;"
      Jwt__Key: "SuperSecretKeyForServiceRequestManagementSystem2026!#*"
      Jwt__Issuer: "ServiceRequestManagementSystem"
      Jwt__Audience: "ServiceRequestManagementSystemUsers"
      Jwt__ExpiryMinutes: "120"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - srms-network

networks:
  srms-network:
    driver: bridge

volumes:
  mssql_data:
    driver: local
```

---

## 4. Health Checks Configuration

- **Service Registration** (`Program.cs`):
  ```csharp
  builder.Services.AddHealthChecks();
  ```
- **Endpoint Route**:
  ```csharp
  app.MapHealthChecks("/health");
  ```
- **Behavior**:
  - Unauthenticated access permitted for Docker daemon, Kubernetes kubelet, and reverse proxies (NGINX/Traefik).
  - Returns HTTP `200 OK` with text `"Healthy"` when the process and its core subsystems are functioning.
  - Automated test coverage verified in `Helpers/HealthCheckTests.cs`.

---

## 5. Continuous Integration (CI/CD Pipeline)

Workflow configuration at `.github/workflows/ci.yml`:

```yaml
name: SRMS Backend CI Pipeline

on:
  push:
    branches: [ "main", "master" ]
  pull_request:
    branches: [ "main", "master" ]

jobs:
  build-and-test:
    name: Build & Run Automated Tests
    runs-on: ubuntu-latest

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        dotnet-quality: 'preview'

    - name: Cache NuGet Packages
      uses: actions/cache@v4
      with:
        path: ~/.nuget/packages
        key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
        restore-keys: |
          ${{ runner.os }}-nuget-

    - name: Restore Dependencies
      run: dotnet restore backend/ServiceRequestManagementSystem.sln

    - name: Build Solution
      run: dotnet build backend/ServiceRequestManagementSystem.sln --no-restore -c Release

    - name: Execute Automated Test Suite
      run: dotnet test backend/ServiceRequestManagementSystem.sln --no-build -c Release --verbosity normal --collect:"XPlat Code Coverage"
```

---

## 6. Deployment Commands

### Spin up full environment:
```bash
docker compose up -d --build
```

### Inspect container health and logs:
```bash
docker compose ps
docker compose logs -f api
```

### Verify API Health endpoint:
```bash
curl http://localhost:5158/health
# Response: Healthy
```

### Shut down and persist volume data:
```bash
docker compose down
```
