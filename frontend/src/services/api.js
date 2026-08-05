const API_BASE_URL = import.meta.env?.VITE_API_BASE_URL || "http://localhost:5083/api";

async function request(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const config = {
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
    ...options,
  };

  const response = await fetch(url, config);

  if (response.status === 401) {
    // Clear session and redirect to login on unauthorized
    localStorage.removeItem("servicedesk.auth");
    sessionStorage.removeItem("servicedesk.auth");
    window.location.href = "/login";
    throw new Error("Unauthorized. Please sign in again.");
  }

  if (!response.ok) {
    const errorText = await response.text();
    let errorMessage = `HTTP error! Status: ${response.status}`;
    try {
      const errJson = JSON.parse(errorText);
      errorMessage = errJson.message || errJson.title || errorText || errorMessage;
    } catch {
      if (errorText) errorMessage = errorText;
    }
    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return null;
  }

  return await response.json();
}

// ----------------------------------------------------
// AUTH API
// ----------------------------------------------------
export async function loginUser(email, password) {
  return await request("/Users/Login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

// ----------------------------------------------------
// USERS API
// ----------------------------------------------------
export async function getUsers() {
  return await request("/Users/GetUsers");
}

export async function getUserById(id) {
  return await request(`/Users/GetUser/${id}`);
}

export async function createUser(userData) {
  return await request("/Users/PostUser", {
    method: "POST",
    body: JSON.stringify(userData),
  });
}

export async function updateUser(id, userData) {
  return await request(`/Users/PutUser/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...userData, userId: Number(id) }),
  });
}

export async function deleteUser(id) {
  return await request(`/Users/DeleteUser/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// ROLES API
// ----------------------------------------------------
export async function getRoles() {
  return await request("/Roles/GetRoles");
}

export async function getRoleById(id) {
  return await request(`/Roles/GetRole/${id}`);
}

export async function createRole(roleData) {
  return await request("/Roles/PostRole", {
    method: "POST",
    body: JSON.stringify(roleData),
  });
}

export async function updateRole(id, roleData) {
  return await request(`/Roles/PutRole/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...roleData, roleId: Number(id) }),
  });
}

export async function deleteRole(id) {
  return await request(`/Roles/DeleteRole/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// PERMISSIONS API
// ----------------------------------------------------
export async function getPermissions() {
  return await request("/Permissions/GetPermissions");
}

export async function getPermissionById(id) {
  return await request(`/Permissions/GetPermission/${id}`);
}

export async function createPermission(permissionData) {
  return await request("/Permissions/PostPermission", {
    method: "POST",
    body: JSON.stringify(permissionData),
  });
}

export async function updatePermission(id, permissionData) {
  return await request(`/Permissions/PutPermission/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...permissionData, permissionId: Number(id) }),
  });
}

export async function deletePermission(id) {
  return await request(`/Permissions/DeletePermission/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// ROLE PERMISSIONS API
// ----------------------------------------------------
export async function getRolePermissions() {
  return await request("/RolePermissions/GetRolePermissions");
}

export async function createRolePermission(data) {
  return await request("/RolePermissions/PostRolePermission", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function deleteRolePermission(id) {
  return await request(`/RolePermissions/DeleteRolePermission/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// DEPARTMENTS API
// ----------------------------------------------------
export async function getDepartments() {
  return await request("/Departments/GetDepartments");
}

export async function getDepartmentById(id) {
  return await request(`/Departments/GetDepartment/${id}`);
}

export async function createDepartment(departmentData) {
  return await request("/Departments/PostDepartment", {
    method: "POST",
    body: JSON.stringify(departmentData),
  });
}

export async function updateDepartment(id, departmentData) {
  return await request(`/Departments/PutDepartment/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...departmentData, departmentId: Number(id) }),
  });
}

export async function deleteDepartment(id) {
  return await request(`/Departments/DeleteDepartment/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// ASSET CATEGORIES API
// ----------------------------------------------------
export async function getAssetCategories() {
  return await request("/AssetCategories/GetAssetCategories");
}

export async function getAssetCategoryById(id) {
  return await request(`/AssetCategories/GetAssetCategory/${id}`);
}

export async function createAssetCategory(categoryData) {
  return await request("/AssetCategories/PostAssetCategory", {
    method: "POST",
    body: JSON.stringify(categoryData),
  });
}

export async function updateAssetCategory(id, categoryData) {
  return await request(`/AssetCategories/PutAssetCategory/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...categoryData, assetCategoryId: Number(id) }),
  });
}

export async function deleteAssetCategory(id) {
  return await request(`/AssetCategories/DeleteAssetCategory/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// ASSETS API
// ----------------------------------------------------
export async function getAssets() {
  return await request("/Assets/GetAssets");
}

export async function getAssetById(id) {
  return await request(`/Assets/GetAsset/${id}`);
}

export async function createAsset(assetData) {
  return await request("/Assets/PostAsset", {
    method: "POST",
    body: JSON.stringify(assetData),
  });
}

export async function updateAsset(id, assetData) {
  return await request(`/Assets/PutAsset/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...assetData, assetId: Number(id) }),
  });
}

export async function deleteAsset(id) {
  return await request(`/Assets/DeleteAsset/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// REQUEST TYPES API
// ----------------------------------------------------
export async function getRequestTypes() {
  return await request("/RequestTypes/GetRequestTypes");
}

export async function getRequestTypeById(id) {
  return await request(`/RequestTypes/GetRequestType/${id}`);
}

export async function createRequestType(data) {
  return await request("/RequestTypes/PostRequestType", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateRequestType(id, data) {
  return await request(`/RequestTypes/PutRequestType/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...data, requestTypeId: Number(id) }),
  });
}

export async function deleteRequestType(id) {
  return await request(`/RequestTypes/DeleteRequestType/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// SERVICE TYPES API
// ----------------------------------------------------
export async function getServiceTypes() {
  return await request("/ServiceTypes/GetServiceTypes");
}

export async function getServiceTypeById(id) {
  return await request(`/ServiceTypes/GetServiceType/${id}`);
}

export async function createServiceType(data) {
  return await request("/ServiceTypes/PostServiceType", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateServiceType(id, data) {
  return await request(`/ServiceTypes/PutServiceType/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...data, serviceTypeId: Number(id) }),
  });
}

export async function deleteServiceType(id) {
  return await request(`/ServiceTypes/DeleteServiceType/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// SERVICE REQUEST STATUSES API
// ----------------------------------------------------
export async function getServiceRequestStatuses() {
  return await request("/ServiceRequestStatuses/GetServiceRequestStatuses");
}

export async function getServiceRequestStatusById(id) {
  return await request(`/ServiceRequestStatuses/GetServiceRequestStatus/${id}`);
}

export async function createServiceRequestStatus(data) {
  return await request("/ServiceRequestStatuses/PostServiceRequestStatus", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateServiceRequestStatus(id, data) {
  return await request(`/ServiceRequestStatuses/PutServiceRequestStatus/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...data, statusId: Number(id) }),
  });
}

export async function deleteServiceRequestStatus(id) {
  return await request(`/ServiceRequestStatuses/DeleteServiceRequestStatus/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// SERVICE REQUESTS API
// ----------------------------------------------------
export async function getServiceRequests() {
  return await request("/ServiceRequests/GetServiceRequests");
}

export async function getServiceRequestById(id) {
  return await request(`/ServiceRequests/GetServiceRequest/${id}`);
}

export async function createServiceRequest(requestData) {
  return await request("/ServiceRequests/PostServiceRequest", {
    method: "POST",
    body: JSON.stringify(requestData),
  });
}

export async function updateServiceRequest(id, requestData) {
  return await request(`/ServiceRequests/PutServiceRequest/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...requestData, requestId: Number(id) }),
  });
}

export async function deleteServiceRequest(id) {
  return await request(`/ServiceRequests/DeleteServiceRequest/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// SERVICE REQUEST REPLIES API
// ----------------------------------------------------
export async function getServiceRequestReplies() {
  return await request("/ServiceRequestReplies/GetServiceRequestReplies");
}

export async function createServiceRequestReply(replyData) {
  return await request("/ServiceRequestReplies/PostServiceRequestReply", {
    method: "POST",
    body: JSON.stringify(replyData),
  });
}

// ----------------------------------------------------
// SERVICE REQUEST ATTACHMENTS API
// ----------------------------------------------------
export async function getServiceRequestAttachments() {
  return await request("/ServiceRequestAttachments/GetServiceRequestAttachments");
}

export async function createServiceRequestAttachment(attachmentData) {
  return await request("/ServiceRequestAttachments/PostServiceRequestAttachment", {
    method: "POST",
    body: JSON.stringify(attachmentData),
  });
}

// ----------------------------------------------------
// SERVICE REQUEST TIMELINES API
// ----------------------------------------------------
export async function getServiceRequestTimelines() {
  return await request("/ServiceRequestTimelines/GetServiceRequestTimelines");
}

export async function createServiceRequestTimeline(timelineData) {
  return await request("/ServiceRequestTimelines/PostServiceRequestTimeline", {
    method: "POST",
    body: JSON.stringify(timelineData),
  });
}

// ----------------------------------------------------
// APPROVALS API
// ----------------------------------------------------
export async function getApprovals() {
  return await request("/Approvals/GetApprovals");
}

export async function getApprovalById(id) {
  return await request(`/Approvals/GetApproval/${id}`);
}

export async function createApproval(approvalData) {
  return await request("/Approvals/PostApproval", {
    method: "POST",
    body: JSON.stringify(approvalData),
  });
}

export async function updateApproval(id, approvalData) {
  return await request(`/Approvals/PutApproval/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...approvalData, approvalId: Number(id) }),
  });
}

export async function decideApproval(id, decision) {
  return await request(`/Approvals/DecideApproval/${id}/decide`, {
    method: "PUT",
    body: JSON.stringify(decision),
  });
}

export async function deleteApproval(id) {
  return await request(`/Approvals/DeleteApproval/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// NOTIFICATIONS API
// ----------------------------------------------------
export async function getNotifications() {
  return await request("/Notifications/GetNotifications");
}

export async function getNotificationById(id) {
  return await request(`/Notifications/GetNotification/${id}`);
}

export async function getNotificationsByUser(userId) {
  return await request(`/Notifications/GetNotificationsByUser/user/${userId}`);
}

export async function createNotification(notificationData) {
  return await request("/Notifications/PostNotification", {
    method: "POST",
    body: JSON.stringify(notificationData),
  });
}

export async function updateNotification(id, notificationData) {
  return await request(`/Notifications/PutNotification/${id}`, {
    method: "PUT",
    body: JSON.stringify({ ...notificationData, notificationId: Number(id) }),
  });
}

export async function deleteNotification(id) {
  return await request(`/Notifications/DeleteNotification/${id}`, {
    method: "DELETE",
  });
}

// Fixed URL: matches the actual backend route [HttpPut("{id}/mark-read")]
export async function markNotificationRead(id) {
  return await request(`/Notifications/MarkNotificationAsRead/${id}/mark-read`, {
    method: "PUT",
  });
}

export async function markAllNotificationsRead(userId) {
  return await request(`/Notifications/MarkAllNotificationsAsRead/user/${userId}/mark-all-read`, {
    method: "PUT",
  });
}

// ----------------------------------------------------
// USER SETTINGS API
// ----------------------------------------------------
export async function getUserSettings() {
  return await request("/UserSettings/GetUserSettings");
}

export async function getUserSettingById(id) {
  return await request(`/UserSettings/GetUserSetting/${id}`);
}

export async function getUserSettingsByUserId(userId) {
  return await request(`/UserSettings/GetUserSettingsByUserId/user/${userId}`);
}

export async function updateUserSettings(userId, settingData) {
  return await request(`/UserSettings/PutUserSetting/${userId}`, {
    method: "PUT",
    body: JSON.stringify({ ...settingData, userId: Number(userId) }),
  });
}

// ----------------------------------------------------
// DEPARTMENT PERSONNEL API
// ----------------------------------------------------
export async function getDepartmentPersonnel() {
  return await request("/DepartmentPersonnel/GetDepartmentPersonnel");
}

export async function createDepartmentPersonnel(data) {
  return await request("/DepartmentPersonnel/PostDepartmentPersonnel", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function deleteDepartmentPersonnel(id) {
  return await request(`/DepartmentPersonnel/DeleteDepartmentPersonnel/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// REQUEST TYPE TECHNICIAN MAPPINGS API
// ----------------------------------------------------
export async function getRequestTypeTechnicianMappings() {
  return await request("/RequestTypeTechnicianMappings/GetMappings");
}

export async function createRequestTypeTechnicianMapping(data) {
  return await request("/RequestTypeTechnicianMappings/PostMapping", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function deleteRequestTypeTechnicianMapping(id) {
  return await request(`/RequestTypeTechnicianMappings/DeleteMapping/${id}`, {
    method: "DELETE",
  });
}

// ----------------------------------------------------
// AUDIT LOGS API
// ----------------------------------------------------
export async function getAuditLogs() {
  return await request("/AuditLogs/GetAuditLogs");
}
