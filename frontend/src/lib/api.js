// Centralized API Client for ASP.NET Core Web API backend

const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5158/api";
const STORAGE_KEY = "servicedesk.auth";

export function getAuthToken() {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY) || localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      return parsed.token || null;
    }
  } catch {
    /* ignore */
  }
  return null;
}

async function request(endpoint, options = {}) {
  const url = endpoint.startsWith("http") ? endpoint : `${API_BASE_URL}${endpoint}`;
  const token = getAuthToken();

  const headers = {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...options.headers,
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const config = {
    ...options,
    headers,
  };

  try {
    const response = await fetch(url, config);

    if (response.status === 401) {
      // Unauthorized — trigger logout or session expired event
      window.dispatchEvent(new CustomEvent("auth:unauthorized"));
    }

    const data = await response.json().catch(() => null);

    if (!response.ok) {
      const errorMsg = data?.message || `Request failed with status ${response.status}`;
      const err = new Error(errorMsg);
      err.status = response.status;
      err.data = data;
      throw err;
    }

    return data;
  } catch (error) {
    console.error(`API Error on [${options.method || "GET"} ${endpoint}]:`, error);
    throw error;
  }
}

// Model Normalizers bridging Backend DTOs and Frontend UI structures
export function normalizeRequest(r) {
  if (!r) return r;
  const idStr = String(r.requestId ?? r.id ?? "");
  const numStr = r.requestNumber || r.no || (idStr ? `SR-2026-${idStr.padStart(4, "0")}` : "");
  return {
    ...r,
    id: idStr,
    requestId: r.requestId ?? (r.id ? Number(r.id) : undefined),
    no: numStr,
    requestNumber: numStr,
    title: r.title || "",
    description: r.description || "",
    serviceType: r.serviceType || "",
    requestType: r.requestType || "",
    department: r.department || "",
    departmentId: r.departmentId,
    requester: r.requester || "",
    requesterEmail: r.requesterEmail || "",
    assignee: r.assignee || null,
    status: r.status || "Pending",
    priority: typeof r.priority === "number"
      ? (["Low", "Medium", "High", "Critical"][r.priority] || "Medium")
      : (r.priority || "Medium"),
    created: r.createdAt || r.created || new Date().toISOString(),
    createdAt: r.createdAt || r.created || new Date().toISOString(),
    updated: r.updatedAt || r.updated || new Date().toISOString(),
    updatedAt: r.updatedAt || r.updated || new Date().toISOString(),
    resolvedAt: r.resolvedAt,
    replies: (r.replies || []).map((rep, idx) => ({
      ...rep,
      id: rep.replyId ?? rep.id ?? idx + 1,
      replyId: rep.replyId ?? rep.id ?? idx + 1,
      author: rep.author || "User",
      role: rep.role || "User",
      message: rep.message || "",
      date: rep.createdAt || rep.date || new Date().toISOString(),
      createdAt: rep.createdAt || rep.date || new Date().toISOString(),
      status: rep.statusTransition || rep.status,
    })),
    timeline: (r.timeline || []).map((t, idx) => ({
      ...t,
      id: t.timelineId ?? t.id ?? `t${idx + 1}`,
      timelineId: t.timelineId ?? t.id ?? idx + 1,
      status: t.status || "",
      changedBy: t.changedBy || "",
      changedAt: t.changedAt || new Date().toISOString(),
      note: t.note || "",
    })),
    attachments: (r.attachments || []).map((a, idx) => ({
      ...a,
      id: a.attachmentId ?? a.id ?? String(idx + 1),
      attachmentId: a.attachmentId ?? a.id ?? idx + 1,
      name: a.fileName || a.name || "Attachment",
      fileName: a.fileName || a.name || "Attachment",
      size: a.fileSizeKB ? `${a.fileSizeKB} KB` : (a.size || "0 KB"),
      url: a.fileUrl || a.url || "#",
    })),
  };
}

export function normalizeApproval(a) {
  if (!a) return a;
  const idStr = String(a.approvalId ?? a.id ?? "");
  const pMap = ["Low", "Medium", "High", "Critical"];
  const sMap = ["Pending", "Approved", "Rejected"];
  return {
    ...a,
    id: idStr,
    approvalId: a.approvalId ?? (a.id ? Number(a.id) : undefined),
    requestId: String(a.requestId ?? ""),
    requestNo: a.requestNo || "",
    title: a.title || "",
    requester: a.requester || "",
    department: a.department || "",
    priority: typeof a.priority === "number" ? (pMap[a.priority] || "Medium") : (a.priority || "Medium"),
    submitted: a.submittedAt || a.submitted || new Date().toISOString(),
    status: typeof a.status === "number" ? (sMap[a.status] || "Pending") : (a.status || "Pending"),
    decidedBy: a.decidedBy || null,
    decidedAt: a.decidedAt || null,
    remarks: a.remarks || "",
  };
}

export function normalizeAsset(ast) {
  if (!ast) return ast;
  const idStr = String(ast.assetId ?? ast.id ?? "");
  const sMap = ["Available", "In Use", "Maintenance", "Retired"];
  return {
    ...ast,
    id: idStr,
    assetId: ast.assetId ?? (ast.id ? Number(ast.id) : undefined),
    tag: ast.assetTag || ast.tag || "",
    name: ast.assetName || ast.name || "",
    category: ast.category || "",
    department: ast.department || "",
    assignedTo: ast.assignedTo || null,
    serial: ast.serialNumber || ast.serial || "",
    status: typeof ast.status === "number" ? (sMap[ast.status] || "In Service") : (ast.status || "In Service"),
    value: ast.bookValue !== undefined ? Number(ast.bookValue) : (ast.value || 0),
    warranty: ast.warrantyUntil || ast.warranty || "",
    purchaseDate: ast.purchaseDate || "",
  };
}

export function normalizeNotification(n) {
  if (!n) return n;
  return {
    ...n,
    id: String(n.notificationId ?? n.id ?? ""),
    notificationId: n.notificationId ?? (n.id ? Number(n.id) : undefined),
    title: n.title || "",
    message: n.message || "",
    type: n.type || "info",
    read: n.isRead ?? n.read ?? false,
    isRead: n.isRead ?? n.read ?? false,
    timestamp: n.createdAt || n.timestamp || new Date().toISOString(),
    link: n.link || "/requests",
  };
}

export const api = {
  get: (url, options) => request(url, { ...options, method: "GET" }),
  post: (url, body, options) =>
    request(url, { ...options, method: "POST", body: JSON.stringify(body) }),
  put: (url, body, options) =>
    request(url, { ...options, method: "PUT", body: JSON.stringify(body) }),
  delete: (url, options) => request(url, { ...options, method: "DELETE" }),

  // Module Endpoints
  auth: {
    login: (credentials) => api.post("/auth/login", credentials),
    register: (userData) => api.post("/auth/register", userData),
    me: () => api.get("/auth/me"),
    changePassword: (data) => api.post("/auth/change-password", data),
    public: () => api.get("/auth/public"),
  },

  users: {
    getAll: async (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      const res = await api.get(`/users${qs ? `?${qs}` : ""}`);
      return res;
    },
    getById: (id) => api.get(`/users/${id}`),
    create: (data) => api.post("/users", data),
    update: (id, data) => api.put(`/users/${id}`, data),
    delete: (id) => api.delete(`/users/${id}`),
  },

  serviceRequests: {
    getAll: async (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      const res = await api.get(`/servicerequests${qs ? `?${qs}` : ""}`);
      if (res?.success && Array.isArray(res.data)) {
        res.data = res.data.map(normalizeRequest);
      }
      return res;
    },
    getById: async (id) => {
      const res = await api.get(`/servicerequests/${id}`);
      if (res?.success && res.data) {
        res.data = normalizeRequest(res.data);
      }
      return res;
    },
    create: async (data) => {
      const res = await api.post("/servicerequests", data);
      if (res?.success && res.data) {
        res.data = normalizeRequest(res.data);
      }
      return res;
    },
    update: (id, data) => api.put(`/servicerequests/${id}`, data),
    updateStatus: (id, statusData) => api.put(`/servicerequests/${id}/status`, statusData),
    assign: (id, assigneeUserId) => api.put(`/servicerequests/${id}/assign`, { assigneeUserId }),
    reopen: (id) => api.put(`/servicerequests/${id}/reopen`),
    cancel: (id) => api.put(`/servicerequests/${id}/cancel`),
    addReply: (id, replyData) => api.post(`/servicerequests/${id}/replies`, replyData),
    uploadAttachment: (formData) => {
      let token = null;
      try {
        const raw = sessionStorage.getItem("servicedesk.auth") || localStorage.getItem("servicedesk.auth");
        if (raw) token = JSON.parse(raw)?.token;
      } catch {}
      return fetch(`${API_BASE_URL}/servicerequests/attachments/upload`, {
        method: "POST",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        body: formData,
      }).then((res) => res.json());
    },
    delete: (id) => api.delete(`/servicerequests/${id}`),
  },

  masters: {
    departments: () => api.get("/masters/departments"),
    departmentById: (id) => api.get(`/masters/departments/${id}`),
    createDepartment: (dto) => api.post("/masters/departments", dto),
    updateDepartment: (id, dto) => api.put(`/masters/departments/${id}`, dto),
    deleteDepartment: (id) => api.delete(`/masters/departments/${id}`),
    serviceTypes: () => api.get("/masters/service-types"),
    serviceTypeById: (id) => api.get(`/masters/service-types/${id}`),
    createServiceType: (dto) => api.post("/masters/service-types", dto),
    updateServiceType: (id, dto) => api.put(`/masters/service-types/${id}`, dto),
    deleteServiceType: (id) => api.delete(`/masters/service-types/${id}`),
    requestTypes: (serviceTypeId) =>
      api.get(`/masters/request-types${serviceTypeId ? `?serviceTypeId=${serviceTypeId}` : ""}`),
    requestTypeById: (id) => api.get(`/masters/request-types/${id}`),
    createRequestType: (dto) => api.post("/masters/request-types", dto),
    updateRequestType: (id, dto) => api.put(`/masters/request-types/${id}`, dto),
    deleteRequestType: (id) => api.delete(`/masters/request-types/${id}`),
    statuses: () => api.get("/masters/statuses"),
    statusById: (id) => api.get(`/masters/statuses/${id}`),
    createStatus: (dto) => api.post("/masters/statuses", dto),
    updateStatus: (id, dto) => api.put(`/masters/statuses/${id}`, dto),
    deleteStatus: (id) => api.delete(`/masters/statuses/${id}`),
    personnel: (departmentId) => api.get(`/masters/personnel?departmentId=${departmentId}`),
    mappings: (requestTypeId) => api.get(`/masters/mappings?requestTypeId=${requestTypeId}`),
  },

  approvals: {
    getAll: async (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      const res = await api.get(`/approvals${qs ? `?${qs}` : ""}`);
      if (res?.success && Array.isArray(res.data)) {
        res.data = res.data.map(normalizeApproval);
      }
      return res;
    },
    getById: async (id) => {
      const res = await api.get(`/approvals/${id}`);
      if (res?.success && res.data) {
        res.data = normalizeApproval(res.data);
      }
      return res;
    },
    decide: (id, decision) => api.put(`/approvals/${id}/decision`, decision),
  },

  assets: {
    getAll: async (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      const res = await api.get(`/assets${qs ? `?${qs}` : ""}`);
      if (res?.success && Array.isArray(res.data)) {
        res.data = res.data.map(normalizeAsset);
      }
      return res;
    },
    getById: async (id) => {
      const res = await api.get(`/assets/${id}`);
      if (res?.success && res.data) {
        res.data = normalizeAsset(res.data);
      }
      return res;
    },
    create: (data) => api.post("/assets", data),
    update: (id, data) => api.put(`/assets/${id}`, data),
    delete: (id) => api.delete(`/assets/${id}`),
  },

  notifications: {
    getAll: async (userId) => {
      let uid = userId;
      if (!uid) {
        try {
          const raw = sessionStorage.getItem("servicedesk.auth") || localStorage.getItem("servicedesk.auth");
          if (raw) uid = JSON.parse(raw)?.userId;
        } catch {}
      }
      const res = await api.get(`/notifications?userId=${uid || 1}`);
      if (res?.success && Array.isArray(res.data)) {
        res.data = res.data.map(normalizeNotification);
      }
      return res;
    },
    getUnreadCount: (userId) => {
      let uid = userId;
      if (!uid) {
        try {
          const raw = sessionStorage.getItem("servicedesk.auth") || localStorage.getItem("servicedesk.auth");
          if (raw) uid = JSON.parse(raw)?.userId;
        } catch {}
      }
      return api.get(`/notifications/unread-count?userId=${uid || 1}`);
    },
    markRead: (id, userId) => {
      let uid = userId;
      if (!uid) {
        try {
          const raw = sessionStorage.getItem("servicedesk.auth") || localStorage.getItem("servicedesk.auth");
          if (raw) uid = JSON.parse(raw)?.userId;
        } catch {}
      }
      return api.put(`/notifications/${id}/read?userId=${uid || 1}`);
    },
    markAllRead: (userId) => {
      let uid = userId;
      if (!uid) {
        try {
          const raw = sessionStorage.getItem("servicedesk.auth") || localStorage.getItem("servicedesk.auth");
          if (raw) uid = JSON.parse(raw)?.userId;
        } catch {}
      }
      return api.put(`/notifications/read-all?userId=${uid || 1}`);
    },
  },

  dashboard: {
    getSummary: (userId) => {
      const qs = userId ? `?userId=${userId}` : "";
      return api.get(`/dashboard/summary${qs}`);
    },
    getCharts: () => api.get("/reports/trends"),
  },

  reports: {
    getOverview: () => api.get("/reports/departments"),
    getDepartments: () => api.get("/reports/departments"),
    getSla: () => api.get("/reports/sla"),
    getTrends: () => api.get("/reports/trends"),
  },

  auditLogs: {
    getAll: (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      return api.get(`/auditlogs${qs ? `?${qs}` : ""}`);
    },
  },

  userSettings: {
    get: (userId) => api.get(`/usersettings/${userId}`),
    update: (userId, data) => api.put(`/usersettings/${userId}`, data),
  },
};

export default api;
