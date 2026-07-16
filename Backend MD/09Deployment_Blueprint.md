# Deployment Blueprint
## Service Request Management System

This document outlines the detailed deployment planning, environment profiles, release workflows, backup operations, and production readiness verifications for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Deployment Overview

### Deployment Objectives
Ensure that the ASP.NET Core Web API backend runs in a secure, scalable, and highly available configuration. The deployment model is designed to support zero-downtime rolling updates, automated database schema migrations, and comprehensive application monitoring.

### Recommended Deployment Approach
We recommend a **Containerized Docker Deployment** orchestrated by **Kubernetes** or managed container clusters (e.g., AWS ECS, Azure App Service). The API container acts as a stateless service behind an Nginx reverse proxy, connecting to a managed SQL Server database instance.

---

## 2. Environment Strategy

The deployment lifecycle is divided into 5 distinct environments:

```
[Development] ──> [Testing / Integration] ──> [QA / UAT] ──> [Staging] ──> [Production]
```

*   **Development (Dev)**: Sandbox for developers to write and test code locally against a mock database.
*   **Testing / Integration (Test)**: Run automated integration and API controller tests in a CI pipeline.
*   **QA / User Acceptance (QA/UAT)**: A replica of production where HODs and stakeholders manually verify features and workflows.
*   **Staging (Stage)**: A pre-production staging environment matching the scale of the production environment, used for performance testing and deployment validation.
*   **Production (Prod)**: The live production environment serving real corporate users and transactions.

---

## 3. Server Infrastructure Requirements

Recommended hardware specifications for the production environment:

| Resource | Minimum Specification | Recommended Specification |
| :--- | :--- | :--- |
| **Operating System**| Linux (Ubuntu Server 22.04 LTS / Alpine) | Linux (RedHat Enterprise / AWS Bottlerocket) |
| **CPU Cores** | 2 vCPU cores | 4 vCPU cores |
| **System RAM** | 4 GB | 8 GB |
| **Storage (SSD)** | 50 GB (General Purpose SSD) | 100 GB+ (Provisioned IOPS SSD) |
| **Network Latency**| < 10ms to database instance | < 2ms (same availability zone / virtual network) |
| **Bandwidth** | 100 Mbps (Burst allowed) | 1 Gbps (Dedicated) |
| **SSL/TLS** | TLS 1.2 minimum | TLS 1.3 enforced |

---

## 4. Software Requirements

The following software categories are required on host servers:

*   **API Hosts**: .NET Runtime 8.0/9.0, Docker Engine.
*   **Reverse Proxy**: Nginx or IIS Server.
*   **Database Server**: Microsoft SQL Server 2022.
*   **SSL Certificates**: Let's Encrypt Certbot or Cloud Certificate Manager.
*   **APM Monitoring**: Prometheus & Grafana or Datadog agent.
*   **Log Management**: Serilog console outputs piped to Seq or Elasticsearch.
*   **Backup Workers**: Cron jobs with Azure CLI or AWS CLI dependencies.

---

## 5. Environment Variables & AppSettings Mapping

To keep settings secure, compile-time configurations are overridden using host environment variables:

| AppSettings Key Path | Env Variable Name | Example Values | Purpose / Scope |
| :--- | :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Server=tcp:sql.net;Database=SD_DB;User ID=app;Password=xyz;` | Database connection string. |
| `JwtSettings:Secret` | `JwtSettings__Secret` | `A_SECURE_GENERATED_JWT_SECRET_KEY` | JWT signing secret key. |
| `EmailSettings:Password`| `EmailSettings__Password` | `smtp_password` | SMTP server password credentials. |
| `UploadSettings:ContainerName`| `UploadSettings__ContainerName` | `prod-attachments` | Storage container name. |
| `ASPNETCORE_ENVIRONMENT`| `ASPNETCORE_ENVIRONMENT` | `Production` | Triggers production configuration. |

---

## 6. Configuration Management

*   **Secrets Storage**: Production database connection strings and JWT signing keys are stored in cloud key vaults (e.g., Azure Key Vault, AWS Secrets Manager) and injected into the container environment variables at startup.
*   **No Code Settings**: No production secrets, connection passwords, or external API keys are committed to Git repository files.

---

## 7. Database Deployment Strategy

### Database Initialization
The database schema is created on startup using EF Core migration scripts or run via CI/CD pipelines before container execution.

### Database Migration Workflow
```
[Build CI] ──> [Generate SQL Migration Scripts] ──> [Run Database Backup] ──> [Apply SQL Schema updates]
                                                                                      │
                                                                               (Verification)
                                                                                      │
[Confirm Update Success] <─── [API Rollout] <─── [Seed Master Data] <─────────────────┘
```

1.  **Backup**: Run a transaction log backup before applying updates.
2.  **Migration**: Run migrations in single-transaction SQL blocks to enable rollbacks on failure.
3.  **Seed Data**: Run seed scripts to insert new lookup values.
4.  **Rollback**: If migration fails, restore the database from the backup.

---

## 8. Web API Deployment Strategy

*   **Docker Containerization**: Build API project using a multi-stage Dockerfile to optimize image sizes:
    *   *Stage 1*: Restore dependencies and build binaries using the SDK image.
    *   *Stage 2*: Copy built files to an Alpine runtime image for execution.
*   **Rolling Updates**: Configure Kubernetes rolling updates to replace old containers one-by-one to avoid downtime.
*   **Health Checks**: Configure `/health/live` and `/health/ready` endpoints to monitor container status.

---

## 9. Static File Deployment Strategy

*   **Uploads**: Save uploaded files to cloud storage (e.g. AWS S3, Azure Blob) rather than local server directories.
*   **Email Templates**: Embed HTML email templates in the application project resources.
*   **Logs**: Configure log writers to log to stdout, allowing container log agents to collect and route logs.

---

## 10. Security Configuration

*   **Reverse Proxy**: Configure Nginx to handle SSL termination, rate limit requests, and block large header packets.
*   **CORS Whitelist**: Restrict API access to authorized frontend domains.
*   **Security Headers**: Enforce security headers (HSTS, Content-Security-Policy, X-Frame-Options) on all API responses.
*   **Firewalls**: Restrict database access to Web API container IP ranges.

---

## 11. Production Logging Strategy

*   **Output Format**: Write logs in JSON format to ease parsing by log aggregation tools.
*   **Log Level**: Default log level set to `Warning` to avoid log storage bloat.
*   **Log Categories**: Log audit events to a dedicated database table, and write application exceptions to files.

---

## 12. Monitoring Strategy

*   **Server Health**: Monitor CPU, RAM, and disk utilization on host instances.
*   **API Metrics**: Track response times, error rates (5xx status codes), and concurrent request counts.
*   **Database Metrics**: Monitor active connections, query latencies, and deadlock counts.

---

## 13. Backup Strategy

The backup strategy is configured as follows:

| Target Scope | Backup Frequency | Storage Target | Retention Period |
| :--- | :--- | :--- | :--- |
| **Database Transaction Log**| Every 15 minutes | Geo-Replicated Blob | 30 Days |
| **Database Full Backup** | Daily | Geo-Replicated Blob | 1 Year |
| **Uploaded Attachments** | Daily incremental | Cold S3 Storage | 7 Years (Audit Compliance) |
| **Application Image** | On release | Container Registry | Last 10 releases |

---

## 14. Disaster Recovery Plan

### Scenario: Database Corruption / Server Loss
1.  Spin up a new database instance in the secondary region.
2.  Restore the latest database backup and apply transaction log updates.
3.  Update connection strings in Web API configuration profiles.

### Scenario: Cloud Storage Access Failure
1.  Configure storage clients to failover to a read-only secondary replica.
2.  Alert users that uploads are temporarily unavailable.

---

## 15. Release Strategy

*   **Versioning**: Use semantic versioning (e.g. `v1.2.0`).
*   **Approval**: Deployments require approval from QA and project leads.
*   **Rollback**: If deployment fails, route traffic back to the previous stable container image.

---

## 16. Deployment Validation Checklist

Verify the following items after deployment:
*   [ ] Checked that health check endpoints (`/health/live`) return `200 Healthy`.
*   [ ] Verified that database tables are updated and lookup data is seeded.
*   [ ] Checked that SSL certificates are active and valid.
*   [ ] Verified that CORS policies block unauthorized domains.
*   [ ] Checked that rate-limiting policies reject request bursts.

---

## 17. Post-Deployment Verification (Smoke Tests)

*   **Authentication**: Create a test user, log in, and verify JWT token generation.
*   **Workflows**: Submit a test ticket and verify auto-assignment rules.
*   **Notifications**: Verify that status updates trigger SignalR and email alerts.
*   **Uploads**: Test uploading a sample PDF attachment and verify it is accessible from cloud storage.

---

## 18. Maintenance Window

*   **Timing**: Perform updates during off-peak hours (e.g., Saturday 2 AM – 4 AM IST).
*   **Alert**: Display a maintenance warning banner in the client UI 24 hours prior to deployment.
*   **Rollback**: If issues are not resolved within the maintenance window, roll back the deployment.

---

## 19. Deployment Risks & Mitigations

*   **Risk: Database Lock Lockups**
    *   *Mitigation*: Run migrations during low-traffic hours and keep migration transactions short.
*   **Risk: Connection Pool Starvation**
    *   *Mitigation*: Configure connection pool size limits and monitor database query latencies.

---

## 20. Production Readiness Checklist

Verify the following before the project goes live:

- [ ] Changed all default secrets and passwords in production environment configurations.
- [ ] Configured SSL/TLS certificates and verified auto-renewal schedules.
- [ ] Configured automated database backups and tested restoration procedures.
- [ ] Set up log management and verified exception logging configurations.
- [ ] Verified that CORS policies restrict access to authorized frontend domains.
- [ ] Configured rate-limiting policies to prevent DoS attacks.
- [ ] Set up server health monitoring and configured alerts.
- [ ] Checked that health checks return status `200`.
- [ ] Prepared rollback procedures for database migrations and container deployments.
- [ ] Checked that database tables are populated with seeded master lookup data.
