# Performance Optimization Blueprint
## Service Request Management System

This document outlines the detailed architectural strategies, caching models, data access optimizations, and performance targets for the **Service Request Management System** backend using **ASP.NET Core Web API**.

---

## 1. Performance Goals

The production environment is configured to meet the following performance targets:

| Metric Category | Target Threshold | Measurement Condition |
| :--- | :--- | :--- |
| **API Response Time** | `< 100ms` (Avg), `< 250ms` (95th Pctl) | Standard operations (excluding bulk reports) |
| **Database Query Latency** | `< 20ms` (Avg) | Read queries utilizing index parameters |
| **Max Concurrent Users** | `1,000` active sessions | No degradation in average response times |
| **System Memory Usage** | `< 1 GB` memory ceiling | Idle Web API container instance |
| **System CPU Usage** | `< 20%` base consumption | Standard API transaction volumes |
| **Application Startup Time**| `< 3 seconds` initialization | Container launch to live health checks |
| **File Upload Performance** | `< 2 seconds` upload time | 5MB file uploads on standard network lines |

---

## 2. Key Performance Indicators (KPIs)

Monitor the following KPIs using APM dashboard integrations:
*   **Request Rate**: Requests per second (RPS).
*   **Error Rate**: Percentage of requests returning 5xx status codes.
*   **Database Query Time**: Execution times for Entity Framework Core queries.
*   **Memory Garbage Collection**: Frequency and duration of garbage collection cycles.
*   **Thread Pool Utilization**: Count of active worker threads.

---

## 3. Database Optimization Strategy

### 3.1 Primary & Foreign Key Strategy
*   Enforce single-column integer identity columns (`INT IDENTITY(1,1)`) as primary keys to keep clustered indexes thin.
*   Index foreign keys (`RoleId`, `DepartmentId`, `StatusId`) to speed up database joins.

### 3.2 Indexing Strategy
*   **Clustered Indexes**: Configured on primary keys (`UserId`, `RequestId`, `AssetId`).
*   **Non-Clustered Indexes**:
    *   `IX_Users_Email`: Speeds up authentication lookups.
    *   `IX_ServiceRequests_StatusId`: Speeds up dashboard filtering queries.
    *   `IX_ServiceRequests_AssigneeUserId`: Speeds up technician ticket queries.
*   **Filtered Indexes**: Define filtered indexes for nullable columns to optimize index size:
    *   *Example*: `CREATE NONCLUSTERED INDEX IX_Requests_Assignee ON dbo.ServiceRequests(AssigneeUserId) WHERE AssigneeUserId IS NOT NULL;`

### 3.3 Database Operations
*   **Connection Pooling**: Configure database connection pooling to reuse connections.
*   **Cursor-Based Pagination**: Use keyset pagination (e.g. tracking `CreatedDate` or `Id` values) instead of offset pagination (`OFFSET...FETCH`) to avoid scanning skipped rows on deep pages.
*   **Batch Writes**: Group multiple entity updates into a single database save operation to reduce database round-trips.

---

## 4. Web API Optimization Strategy

*   **Asynchronous Processing**: Enforce `async` and `await` patterns across all database, file storage, and email gateway integrations to prevent blocking threads.
*   **Payload Reduction**: Compress JSON responses (gzip/brotli) to reduce network payload sizes.
*   **No Tracking Queries**: Use `.AsNoTracking()` in Entity Framework Core for read-only queries to bypass change tracking overhead.
*   **Selective Projections**: Query only the required columns by projecting queries directly into DTOs (`Select(r => new RequestDto { ... })`).

---

## 5. Caching Strategy

Implement a multi-tier caching model to reduce database load:

```
[Incoming Request] ──> [API Response Cache (Memory / Redis)] 
                             │
                      (Cache Miss)
                             │
                             v
                [Query Database] ──> [Update Cache]
```

*   **Memory Cache (In-Memory)**: Store highly static lookup tables (e.g. role definitions, service types, request types). Set cache expiration to 12 hours with automated invalidation when changes are made.
*   **Distributed Cache**: Store user session details and configurations in a shared cache (e.g. Redis) to support stateless scaling.
*   **Response Caching**: Add cache control headers (`Cache-Control: public, max-age=60`) to public, read-only endpoints (e.g. FAQs).

---

## 6. File Handling Optimization

*   **Streaming Uploads**: Stream file uploads directly to cloud storage (e.g. AWS S3, Azure Blob) instead of loading entire files into memory.
*   **Upload Validation**: Enforce file size limits (max 10MB) and validate file signatures (magic bytes) to block invalid file types.
*   **Image Compression**: Compress and resize uploaded image attachments in background tasks before saving them to storage.

---

## 7. Background Processing

Offload long-running operations to background tasks (e.g., Hangfire, Quartz.NET) to keep API responses fast:
*   **Email Dispatches**: Queue outbound notification emails.
*   **Daily Activity Summaries**: Compile and send daily activity reports to HODs.
*   **Data Cleanup Tasks**: Permanently delete soft-deleted records that are older than the retention period limit (e.g. 7 years).

---

## 8. Logging Optimization

*   **Log Level**: Set the production log level to `Warning` to avoid logging noise and reduce disk write overhead.
*   **Asynchronous Logging**: Use Serilog's asynchronous sinks (`WriteTo.Async`) to write log files on background threads.
*   **Log Filtering**: Avoid logging sensitive personal data (e.g., user passwords, authentication tokens).

---

## 9. Authentication Performance

*   **Local JWT Validation**: Validate JWT access tokens locally using public/private key verification instead of calling auth databases on every request.
*   **Short Token Lifetimes**: Configure access token expiration to 15 minutes, and use refresh tokens in secure cookies for session renewal.
*   **Permission Cache**: Cache user permissions in memory (e.g., using a 5-minute cache) to avoid querying user-role permissions on every API request.

---

## 10. Query Performance Strategy

*   **Prevent N+1 Queries**: Always load related data using eager loading (`Include()`) or explicit projections to avoid executing separate database queries for child entities.
*   **Limit Search Scope**: Limit search queries to a minimum of 3 characters and restrict search fields to indexed columns.
*   **Sort Whitelist**: Restrict sort columns to indexed fields to prevent sorting operations from scanning full tables.

---

## 11. Scalability Strategy

*   **Stateless API Design**: Keep Web API instances stateless by storing session details and cache states externally (e.g., in Redis), allowing instances to scale horizontally.
*   **Read Replicas**: Configure the database client to route write queries to the primary database and read queries to read replicas.
*   **Load Balancing**: Use load balancers to distribute traffic evenly across active API container instances.

---

## 12. Resource Management

*   **Garbage Collection Configuration**: Enforce Server Garbage Collection (Server GC) in configuration files to optimize memory management on multi-core host servers.
*   **Connection Pools**: Configure connection pool limits for database and HTTP clients to prevent resource starvation.

---

## 13. Monitoring & Profiling Tools

Configure the following tools in the staging and production environments:
*   **Database Profiling**: Use tools like SQL Server Profiler or MiniProfiler to identify slow database queries during testing.
*   **Metrics Collection**: Configure Prometheus and Grafana dashboards to monitor CPU, RAM, and request rates.

---

## 14. Performance Testing Strategy

*   **Load Testing**: Verify that average response times remain under 100ms with 100 concurrent users.
*   **Stress Testing**: Scale load up to 1000 concurrent users to identify database connection bottle-necks.
*   **Spike Testing**: Simulate sudden spikes in traffic (e.g., when shift schedules change) to verify load balancer configurations.
*   **Endurance Testing**: Run tests for 24 hours under a sustained load of 200 concurrent users to check for memory leaks.

---

## 15. Performance Optimization Checklist

Verify the following items before production release:

- [ ] Configured response compression (Brotli/Gzip) on the Web API host.
- [ ] Added `AsNoTracking()` to all read-only database queries in repositories.
- [ ] Projected database queries directly into DTOs instead of loading full entities.
- [ ] Configured filtered indexes for nullable columns.
- [ ] Configured local JWT validation and verified that token signatures are verified without database queries.
- [ ] Set up asynchronous logging sinks (`WriteTo.Async`) in Serilog configurations.
- [ ] Set the production log level to `Warning` to reduce disk write overhead.
- [ ] Set up in-memory caching for lookup tables and verified invalidation rules.
- [ ] Verified that all database connections use DbContext connection pooling.
- [ ] Configured Server Garbage Collection (Server GC) in application runtime properties.

---

## 16. Future Optimization Opportunities

*   **Distributed Caching (Redis)**: Scale caching capabilities across multiple API container instances.
*   **CDN File Delivery**: Serve uploaded attachments using Content Delivery Networks (CDNs) to reduce API server bandwidth usage.
*   **Message Queues**: Integrate message brokers (e.g. RabbitMQ) to manage background processes.
*   **Database Read Replicas**: Distribute database read queries across read replicas.
