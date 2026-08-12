# Step 16 — Asset Management Module Planning

## 1. Objective

Plan hardware and equipment inventory management services, asset tag generation, INR book value storage, warranty tracking, and employee asset assignment APIs.

---

## 2. Why This Step Is Required

The Asset Management module enables administrators and technicians to track hardware inventory (laptops, printers, monitors, HVAC, network switches), monitor warranty expiration dates, and assign assets to specific employees.

---

## 3. Prerequisites

* Step 10 (Users & Settings) completed.

---

## 4. What Needs To Be Done

1. Plan `AssetService` in `ServiceDesk.Application`.
2. Plan Asset Endpoints (`/api/v1/assets`):
   * `GET /`: Get paginated assets list (Filters: `category`, `status`, `departmentId`, `assignedToUserId`, `search`).
   * `GET /{id}`: Get asset details.
   * `POST /`: Create asset (`AssetTag`, `AssetName`, `Category`, `SerialNumber`, `DepartmentId`, `Status`, `PurchaseDate`, `WarrantyUntil`, `BookValue`).
   * `PUT /{id}`: Update asset details.
   * `DELETE /{id}`: Soft delete asset (`IsDeleted = 1`).
   * `PUT /{id}/assign`: Assign or unassign asset to employee user (`AssignedToUserId`).

---

## 5. Project-Specific Requirements

* **Unique Constraints**: `AssetTag` (e.g. `AST-IT-0142`) and `SerialNumber` (e.g. `DL7440-98213`) must be unique in database.
* **Currency Formatting**: Store `BookValue` with precision `DECIMAL(18,2)`.
* **Access Control**: Creating, editing, soft-deleting, and assigning assets is restricted to `Admin` and `Technician` roles (`[Authorize(Roles = "Admin,Technician")]`). Requesters can view assets assigned to them via `/api/v1/assets?assignedToUserId={currentUserId}`.
* **Frontend Alignment**: Powers `_shell.assets.index.jsx`, `_shell.assets.$assetId.jsx`, and User Profile assigned assets tab (`_shell.profile.jsx`).

---

## 6. Important Decisions

* **Category Storage**: Category is stored directly as a string or Enum property on `dbo.Assets` (`Laptop`, `Printer`, `Monitor`, `HVAC`, `Network`, `AV Equipment`, `Tablet`, `Desktop`).

---

## 7. Dependencies

* **Upstream**: Step 10 (Users & Settings).
* **Downstream**: Step 17 (Notifications), Step 19 (Dashboard).

---

## 8. Expected Result

After completing this step:

✓ Asset inventory CRUD APIs are planned.
✓ Asset assignment and unassignment endpoints are planned.
✓ Alignment with `_shell.assets.index.jsx` is confirmed.

---

## 9. Verification Checklist

- [ ] Planned paginated asset list endpoint with search and category filtering.
- [ ] Enforced unique `AssetTag` and `SerialNumber` checks.
- [ ] Configured `DECIMAL(18,2)` column type for book value.
- [ ] Restricted management operations to Admin and Technician.
- [ ] Planned asset user assignment/unassignment endpoint.

---

## 10. Move To Next Step

When all checklist items are completed, move to:

**Next Step → [17_Notifications.md](file:///d:/CSE%20Sem-5%20%28A-A2%29%20Subject/%28ASP.NET%29%20ASP.NET%20Core/ASP.NET%20Project/Service_Request_Management_System/Backend%20MD%20File/17_Notifications.md)**
