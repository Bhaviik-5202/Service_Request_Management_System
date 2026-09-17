// Centralized API Client for ASP.NET Core Web API backend

const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:7001/api";
const STORAGE_KEY = "servicedesk.auth";

function getAuthToken() {
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
    getAll: (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      return api.get(`/users${qs ? `?${qs}` : ""}`);
    },
    getById: (id) => api.get(`/users/${id}`),
    create: (data) => api.post("/users", data),
    update: (id, data) => api.put(`/users/${id}`, data),
    delete: (id) => api.delete(`/users/${id}`),
  },

  serviceRequests: {
    getAll: (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      return api.get(`/servicerequests${qs ? `?${qs}` : ""}`);
    },
    getById: (id) => api.get(`/servicerequests/${id}`),
    create: (data) => api.post("/servicerequests", data),
    update: (id, data) => api.put(`/servicerequests/${id}`, data),
    updateStatus: (id, statusData) => api.put(`/servicerequests/${id}/status`, statusData),
    assign: (id, assigneeUserId) => api.put(`/servicerequests/${id}/assign`, { assigneeUserId }),
    reopen: (id, reason) => api.post(`/servicerequests/${id}/reopen`, { reason }),
    cancel: (id, reason) => api.post(`/servicerequests/${id}/cancel`, { reason }),
    addReply: (id, replyData) => api.post(`/servicerequests/${id}/replies`, replyData),
    addAttachment: (id, attachmentData) => api.post(`/servicerequests/${id}/attachments`, attachmentData),
    delete: (id) => api.delete(`/servicerequests/${id}`),
  },

  masters: {
    departments: () => api.get("/masters/departments"),
    serviceTypes: () => api.get("/masters/servicetypes"),
    requestTypes: () => api.get("/masters/requesttypes"),
    statuses: () => api.get("/masters/statuses"),
    technicians: () => api.get("/masters/technicians"),
    technicianMappings: () => api.get("/masters/technician-mappings"),
  },

  approvals: {
    getAll: (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      return api.get(`/approvals${qs ? `?${qs}` : ""}`);
    },
    getById: (id) => api.get(`/approvals/${id}`),
    decide: (id, decision) => api.put(`/approvals/${id}/decision`, decision),
  },

  assets: {
    getAll: (params) => {
      const qs = new URLSearchParams(params || {}).toString();
      return api.get(`/assets${qs ? `?${qs}` : ""}`);
    },
    getById: (id) => api.get(`/assets/${id}`),
    create: (data) => api.post("/assets", data),
    update: (id, data) => api.put(`/assets/${id}`, data),
    delete: (id) => api.delete(`/assets/${id}`),
  },

  notifications: {
    getAll: () => api.get("/notifications"),
    markRead: (id) => api.put(`/notifications/${id}/read`),
    markAllRead: () => api.put("/notifications/mark-all-read"),
  },

  dashboard: {
    getSummary: () => api.get("/dashboard/summary"),
    getCharts: () => api.get("/dashboard/charts"),
  },

  reports: {
    getOverview: () => api.get("/reports/overview"),
    getSla: () => api.get("/reports/sla"),
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
