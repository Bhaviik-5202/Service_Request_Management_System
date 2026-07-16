# Maintenance & Operations Blueprint
## Service Request Management System

This document outlines the detailed system maintenance, log management, database tuning, software dependency lifecycle, incident response runbooks, and task calendars for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Maintenance Overview

### Purpose
To ensure long-term stability, performance, and security of the Web API backend in production.

### Operational Goals
*   Maintain system availability above `99.9%` (excluding scheduled maintenance windows).
*   Enforce security standards by regularly applying patches.
*   Optimize database indexes and clear transactional logs before disk thresholds are breached.

---

## 2. Maintenance Classifications

*   **Corrective Maintenance**: Debugging and resolving runtime failures, database deadlocks, or exception alerts reported by users.
*   **Preventive Maintenance**: Index rebuilds, log rotation, database statistics updates, and resource usage reviews to prevent failures.
*   **Adaptive Maintenance**: Upgrading framework versions (e.g. .NET major version upgrades) and modifying integrations to match updated external API contracts.
*   **Perfective Maintenance**: Refactoring queries, updating cache lifetimes, and optimizing payloads to improve performance.

---

## 3. Monitoring Strategy

Configure monitoring agents to collect metrics across the following components:
*   **API Health**: Track request rates, response times, and error rates (5xx status codes).
*   **Database Health**: Monitor active database connections, lock contention, and slow query executions.
*   **Server Resources**: Set up alerts for high CPU, RAM, and disk utilization on host instances.
*   **Background Jobs**: Track execution times and failure rates of queued background tasks (e.g. email dispatches).

---

## 4. Logging Maintenance

*   **Log Files**: serilog logs are written locally in JSON format before being collected by log agents.
*   **Log Rotation**: Configure log files to roll daily or when file size exceeds 50MB.
*   **Retention Policy**: Keep application logs for 30 days, security and audit logs for 7 years (compliance requirement).
*   **Automatic Cleanup**: Set up background cleanup tasks to delete expired log files automatically.

---

## 5. Backup Strategy

The backup strategy is configured as follows:

| Resource Scope | Backup Category | Frequency | Storage Location | Retention Period | Restore Test |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Primary Database**| Transaction Log | Every 15 minutes | Geo-Replicated S3 | 30 Days | Monthly restore |
| **Primary Database**| Full Backup | Daily (Off-peak) | Geo-Replicated S3 | 1 Year | Monthly restore |
| **Static Attachments**| Incremental sync | Daily | Cold S3 Storage | 7 Years | Quarterly verify |
| **Configurations** | Snapshot | On modification | Secure Vault | Infinite | Verified on change |

---

## 6. Database Maintenance

*   **Index Rebuilds**: Configure weekly tasks to rebuild indexes with fragmentation above 30%, and reorganize indexes with fragmentation between 5% and 30%.
*   **Statistics Updates**: Update database statistics daily to ensure the query optimizer builds efficient execution plans.
*   **Data Archiving**: Move resolved tickets older than 2 years to archive tables to keep primary tables thin.

---

## 7. Dependency Management

*   **Framework Updates**: Perform annual reviews to upgrade to the latest .NET LTS release.
*   **Package Updates**: Upgrade third-party NuGet packages monthly.
*   **Security Patches**: Critical security patches must be applied within 48 hours of release.

---

## 8. Configuration Maintenance

*   **Secret Key Rotation**: Rotate JWT signing keys and database connection passwords annually.
*   **SMTP Credentials**: Update email gateway credentials and verify connection settings during quarterly reviews.
*   **Third-Party Keys**: Manage external integration keys in cloud key vaults and verify connection statuses weekly.

---

## 9. User & Access Maintenance

*   **Account Deactivation**: Set up automated tasks to deactivate user accounts that have been inactive for more than 90 days.
*   **Permissions Audit**: Perform quarterly reviews of user permissions and roles.
*   **Technician Mapping Review**: Update auto-assignment mapping rules when technicians join or leave departments.

---

## 10. Security Maintenance

*   **Vulnerability Scanning**: Run automated vulnerability scans (e.g., OWASP ZAP) weekly on staging environments.
*   **Certificate Renewal**: SSL/TLS certificates must be configured for auto-renewal (e.g., Let's Encrypt).
*   **Credential Audits**: Audit administrator accounts and credentials quarterly.

---

## 11. Performance Maintenance

*   **Slow Query Review**: Identify and optimize database queries executing in over 100ms.
*   **Cache Utilization**: Review Redis/Memory cache hit ratios and adjust cache lifetimes accordingly.
*   **Connection Audits**: Verify database connection pooling settings to prevent connection leaks.

---

## 12. Incident Management

### Incident Classification
*   `Severity 1 (Critical)`: API down, database inaccessible, or security breach.
*   `Severity 2 (High)`: Critical feature failure (e.g. ticket creation fails) without workarounds.
*   `Severity 3 (Medium)`: Non-blocking feature failure (e.g. comments/replies fail).
*   `Severity 4 (Low)`: Minor UI inconsistencies or performance warnings.

### Response Procedure
```
[Identify Incident] ──> [Classify Severity] ──> [Notify On-Call Engineer]
                                                       │
                                                (Investigation)
                                                       │
[Incident Review] <── [Verify Resolution] <── [Apply Fix / Rollback]
```

---

## 13. Release Management

*   **Versioning**: Use semantic versioning (e.g. `v1.2.1`).
*   **Change Log**: Maintain a `CHANGELOG.md` file documenting changes in each release.
*   **Rollback Plan**: Prepare rollback scripts (e.g. SQL rollback migrations and container rollback commands) for all releases.

---

## 14. Documentation Maintenance

Update project documentation files (Swagger UI, README, database design, API design blueprints) concurrently with code changes.

---

## 15. Operational Runbook

### Runbook: Restarting Containerized API Services
1.  Verify the status of active containers: `docker ps` or `kubectl get pods`.
2.  Trigger a rolling restart: `kubectl rollout restart deployment/servicedesk-api`.
3.  Check startup logs: `docker logs --tail 100 <container-id>`.
4.  Verify that health checks `/health/live` return `200 Healthy`.

### Runbook: Database Backup Restoration
1.  Isolate the database server by blocking new connections.
2.  Restore the latest full database backup:
    `RESTORE DATABASE ServiceDeskDB FROM DISK = '/backups/full_db.bak' WITH NORECOVERY;`
3.  Apply subsequent transaction log updates:
    `RESTORE LOG ServiceDeskDB FROM DISK = '/backups/log_1.trn' WITH RECOVERY;`
4.  Verify database integrity and re-enable connections.

---

## 16. Maintenance Schedule Tasks

### Daily Tasks
*   [ ] Verify that automated backups completed successfully.
*   [ ] Check application error logs for critical exceptions.
*   [ ] Monitor server CPU, RAM, and disk utilization.

### Weekly Tasks
*   [ ] Run database statistics updates.
*   [ ] Check log storage usage and verify log rotation.
*   [ ] Review slow API response alerts.

### Monthly Tasks
*   [ ] Rebuild/reorganize database indexes.
*   [ ] Run test database restorations to verify backup integrity.
*   [ ] Update third-party NuGet packages.

### Quarterly Tasks
*   [ ] Audit user roles and permissions.
*   [ ] Rotate API secrets and database connection passwords.
*   [ ] Run full security vulnerability scans.

### Yearly Tasks
*   [ ] Perform major framework upgrades (.NET major version upgrades).
*   [ ] Run disaster recovery failover tests.
*   [ ] Archive resolved tickets older than 2 years.

---

## 17. Maintenance Risks & Mitigations

*   **Risk: Lockup during migration**
    *   *Mitigation*: Run migrations during off-peak hours and keep transactions short.
*   **Risk: Storage exhaustion**
    *   *Mitigation*: Set up automated alerts at 80% disk utilization and configure log cleanup.

---

## 18. Maintenance KPIs

*   **Availability**: System uptime percentage (Target: >99.9%).
*   **Time to Resolve (TTR)**: Average time to resolve Severity 1 incidents (Target: <2 hours).
*   **Backup Success Rate**: Percentage of scheduled backups that completed successfully (Target: 100%).
*   **Patching Latency**: Average time to apply critical security patches (Target: <48 hours).

---

## 19. Future Maintenance Improvements

*   **Automated Runbooks**: Implement automated failover procedures.
*   **Centralized Dashboards**: Combine server, database, and application metrics into a unified dashboard.
*   **Proactive Alerts**: Implement machine learning models to detect anomaly spikes in error rates.

---

## 20. Ongoing Operations Checklist

Verify the following items on a recurring basis:

- [ ] Database backup files are generated and stored in geo-replicated storage.
- [ ] Database restore tests are executed monthly.
- [ ] Log rotation tasks run daily and clean up expired logs.
- [ ] Database statistics and index fragmentation levels are reviewed weekly.
- [ ] User role permissions and active administrator profiles are audited quarterly.
- [ ] Vulnerability scans are run weekly on staging environments.
- [ ] API health checks (`/health/live`) return `200`.
- [ ] Server CPU, memory, and disk usage remain within safe parameters.
- [ ] Dynamic connection strings and passwords are encrypted in storage vault systems.
- [ ] SLA triggers and email notification dispatches operate without delays.
