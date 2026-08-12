# Step 15 — Replies, Timeline & Attachments Module Planning

## 1. Objective

Plan ticket discussion reply APIs, ticket timeline audit tracking, file upload handling (`wwwroot/uploads`), file type validations, and file metadata storage in `dbo.ServiceRequestAttachments`.

---

## 2. Why This Step Is Required

Tickets require interactive communications (discussion thread between Requester and Technician), historical audit logs (status transition timeline), and attachment capabilities for screenshots and error logs.

---

## 3. Prerequisites

* Step 12 (Service Requests) completed.
* Static `wwwroot/uploads` directory created.

---

## 4. What Needs To Be Done

1. Plan Replies Endpoints (`/api/v1/requests/{id}/replies`):
   * `GET /{id}/replies`: Get discussion comments for a ticket.
   * `POST /{id}/replies`: Add new comment message. Support optional status transition payload (e.g. changing status to `In Progress` while commenting).
2. Plan Timeline Endpoints (`/api/v1/requests/{id}/timeline`):
   * `GET /{id}/timeline`: Get chronological audit events for a ticket.
   * Automatic insertion into `dbo.ServiceRequestTimeline` whenever:
     * Ticket is raised
     * Approval is requested/decided
     * Technician is assigned
     * Status is updated
     * Ticket is closed, reopened, or cancelled.
3. Plan File Attachment Endpoints (`/api/v1/attachments`):
   * `POST /upload`: Upload single/multiple files (`multipart/form-data`).
     * Validate file extension (Allowed: `.png`, `.jpg`, `.pdf`, `.txt`, `.docx`).
     * Validate maximum size (10 MB per file).
     * Generate unique filename GUID (`5f89c3db-24b5-4b1f-9ae2-581ef5f72da0.png`).
     * Save physical file to `wwwroot/uploads/`.
     * Insert record in `dbo.ServiceRequestAttachments` (`FileName`, `FileSizeKB`, `FileUrl`, `UploadedByUserId`).
   * `GET /{id}/download`: Stream file download.

---

## 5. Project-Specific Requirements

* **File Storage Strategy**: Physical file BLOBs are stored on local server disk (`wwwroot/uploads`), while database tables store lightweight file metadata.
* **Frontend Alignment**: Powers discussion comments, timeline log, and attachment box on `_shell.requests.$requestId.index.jsx`.

---

## 6. Important Decisions

* **Cascade Attachment Deletion**: If a reply or request is hard deleted, delete corresponding physical files from disk storage.

---

## 7. Dependencies

* **Upstream**: Step 12 (Service Requests).
* **Downstream**: Step 17 (Notifications), Step 21 (Frontend Integration).

---

## 8. Expected Result

After completing this step:

✓ Ticket reply discussion thread APIs are planned.
✓ Automatic timeline log creation is planned.
✓ File upload service and metadata storage are established.

---

## 9. Verification Checklist

- [ ] Planned `POST /{id}/replies` endpoint.
- [ ] Planned automatic `ServiceRequestTimeline` insertion on status changes.
- [ ] Enforced 10 MB file size limit and file type whitelist.
- [ ] Configured physical file saving to `wwwroot/uploads/`.
- [ ] Confirmed alignment with ticket detail page.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [16_Asset_Management.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/16_Asset_Management.md)**
