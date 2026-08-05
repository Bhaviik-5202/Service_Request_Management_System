import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { loginUser, getUsers, createUser, updateUser, getRoles, getDepartments } from "@/services/api";

const ALL = [
  "dashboard.view",
  "requests.view",
  "requests.create",
  "requests.edit",
  "requests.delete",
  "approvals.view",
  "approvals.decide",
  "assets.view",
  "assets.manage",
  "users.view",
  "users.manage",
  "reports.view",
  "notifications.view",
  "help.view",
  "settings.view",
];

export const ROLE_PERMISSIONS = {
  Admin: ALL,
  HOD: [
    "dashboard.view",
    "requests.view",
    "requests.edit",
    "reports.view",
    "notifications.view",
    "help.view",
    "settings.view",
  ],
  Technician: [
    "dashboard.view",
    "requests.view",
    "requests.edit",
    "assets.view",
    "assets.manage",
    "notifications.view",
    "help.view",
    "settings.view",
  ],
  Requestor: [
    "dashboard.view",
    "requests.view",
    "requests.create",
    "notifications.view",
    "help.view",
    "settings.view",
  ],
};

const AuthContext = createContext(null);
const STORAGE_KEY = "servicedesk.auth";

// Normalize user object format from API — never includes passwordHash
function normalizeUser(u, rolesList = [], deptsList = []) {
  if (!u) return null;

  let roleName = u.roleName || u.role;
  if (!roleName && u.roleId && rolesList.length > 0) {
    const foundRole = rolesList.find((r) => r.roleId === u.roleId);
    if (foundRole) roleName = foundRole.roleName;
  }
  if (!roleName) roleName = "Requestor";

  let deptName = u.departmentName || u.department;
  if (!deptName && u.departmentId && deptsList.length > 0) {
    const foundDept = deptsList.find((d) => d.departmentId === u.departmentId);
    if (foundDept) deptName = foundDept.departmentName;
  }
  if (!deptName) deptName = "IT";

  const name = u.fullName || u.name || "User";
  const initials = name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .substring(0, 2)
    .toUpperCase();

  return {
    id: String(u.userId || u.id || "1"),
    userId: u.userId || Number(u.id) || 1,
    employeeId: u.employeeId || "",
    name,
    fullName: name,
    email: u.email || "",
    role: roleName,
    roleId: u.roleId || 4,
    department: deptName,
    departmentId: u.departmentId || null,
    phone: u.phone || "",
    status: u.status || "Active",
    avatar: initials,
    joined: u.joinedDate
      ? new Date(u.joinedDate).toISOString().split("T")[0]
      : new Date().toISOString().split("T")[0],
  };
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [role, setRoleState] = useState(null);
  const [signedIn, setSignedIn] = useState(false);
  const [hydrated, setHydrated] = useState(false);

  // Hydrate session on mount
  useEffect(() => {
    async function hydrateSession() {
      try {
        const raw = sessionStorage.getItem(STORAGE_KEY) || localStorage.getItem(STORAGE_KEY);
        if (raw) {
          const parsed = JSON.parse(raw);
          if (parsed.signedIn && parsed.user) {
            setUser(parsed.user);
            setRoleState(parsed.user.role);
            setSignedIn(true);
          }
        }
      } catch {
        // Corrupted storage — clear it
        localStorage.removeItem(STORAGE_KEY);
        sessionStorage.removeItem(STORAGE_KEY);
      } finally {
        setHydrated(true);
      }
    }

    hydrateSession();
  }, []);

  const signIn = useCallback(async (email, password, remember = true) => {
    try {
      // Use server-side login validation
      const userData = await loginUser(email, password);
      if (!userData) return false;

      const [apiRoles, apiDepts] = await Promise.all([
        getRoles().catch(() => []),
        getDepartments().catch(() => []),
      ]);

      const normalized = normalizeUser(userData, apiRoles, apiDepts);
      setUser(normalized);
      setRoleState(normalized.role);
      setSignedIn(true);

      const sessionData = { userId: normalized.userId, role: normalized.role, signedIn: true, user: normalized };
      if (remember) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        sessionStorage.removeItem(STORAGE_KEY);
      } else {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        localStorage.removeItem(STORAGE_KEY);
      }
      return true;
    } catch {
      return false;
    }
  }, []);

  const signUp = useCallback(async (userData) => {
    try {
      const [apiUsers, apiRoles, apiDepts] = await Promise.all([
        getUsers().catch(() => []),
        getRoles().catch(() => []),
        getDepartments().catch(() => []),
      ]);

      const duplicate = apiUsers.find(
        (u) => u.email && u.email.toLowerCase() === userData.email.toLowerCase()
      );
      if (duplicate) return false;

      let roleId = 4;
      const foundRole = apiRoles.find((r) => r.roleName === userData.role);
      if (foundRole) roleId = foundRole.roleId;

      let departmentId = null;
      const foundDept = apiDepts.find((d) => d.departmentName === userData.department);
      if (foundDept) departmentId = foundDept.departmentId;

      const newUserPayload = {
        employeeId: `EMP-${Date.now().toString().slice(-4)}`,
        fullName: userData.name || userData.fullName,
        email: userData.email,
        passwordHash: userData.password,
        roleId,
        departmentId,
        phone: userData.phone || "",
        status: "Active",
        joinedDate: new Date().toISOString(),
      };

      const created = await createUser(newUserPayload);
      const normalized = normalizeUser(created, apiRoles, apiDepts);

      setUser(normalized);
      setRoleState(normalized.role);
      setSignedIn(true);

      localStorage.setItem(
        STORAGE_KEY,
        JSON.stringify({ userId: normalized.userId, role: normalized.role, signedIn: true, user: normalized })
      );
      sessionStorage.removeItem(STORAGE_KEY);

      return true;
    } catch {
      return false;
    }
  }, []);

  const signOut = useCallback(() => {
    setUser(null);
    setRoleState(null);
    setSignedIn(false);
    localStorage.removeItem(STORAGE_KEY);
    sessionStorage.removeItem(STORAGE_KEY);
  }, []);

  // Switch the active role/user (admin dev utility)
  const setRole = useCallback(async (r) => {
    try {
      const [apiUsers, apiRoles, apiDepts] = await Promise.all([
        getUsers().catch(() => []),
        getRoles().catch(() => []),
        getDepartments().catch(() => []),
      ]);

      const defaultEmails = {
        Admin: "admin@gmail.com",
        HOD: "hod@gmail.com",
        Technician: "tech@gmail.com",
        Requestor: "requestor@gmail.com",
      };
      const email = defaultEmails[r];
      const matchedUser = apiUsers.find(
        (u) => u.email && u.email.toLowerCase() === email.toLowerCase()
      );

      if (matchedUser) {
        const normalized = normalizeUser(matchedUser, apiRoles, apiDepts);
        setUser(normalized);
        setRoleState(r);

        const sessionData = { userId: normalized.userId, role: r, signedIn: true, user: normalized };
        if (sessionStorage.getItem(STORAGE_KEY)) {
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        } else {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        }
      }
    } catch {
      // Silently fail — role switcher is not critical
    }
  }, []);

  const can = useCallback((p) => (role ? ROLE_PERMISSIONS[role]?.includes(p) : false), [role]);

  const updateProfile = useCallback(
    async (updatedUserData) => {
      if (!user) return;

      const updatedUserPayload = {
        userId: user.userId,
        employeeId: user.employeeId || "EMP-101",
        fullName: updatedUserData.name || updatedUserData.fullName || user.fullName,
        email: updatedUserData.email || user.email,
        passwordHash: updatedUserData.password || "unchanged",
        roleId: user.roleId || 1,
        departmentId: user.departmentId,
        phone: updatedUserData.phone ?? user.phone,
        status: user.status || "Active",
      };

      await updateUser(user.userId, updatedUserPayload);

      const updatedUser = {
        ...user,
        name: updatedUserPayload.fullName,
        fullName: updatedUserPayload.fullName,
        email: updatedUserPayload.email,
        phone: updatedUserPayload.phone,
      };
      const initials = updatedUser.fullName
        .split(" ")
        .map((n) => n[0])
        .join("")
        .substring(0, 2)
        .toUpperCase();
      updatedUser.avatar = initials;

      setUser(updatedUser);

      const sessionData = { userId: updatedUser.userId, role: updatedUser.role, signedIn: true, user: updatedUser };
      if (sessionStorage.getItem(STORAGE_KEY)) {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
      } else {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
      }
    },
    [user]
  );

  const value = useMemo(
    () => ({ user, role, signedIn, signIn, signUp, signOut, setRole, can, updateProfile }),
    [user, role, signedIn, signIn, signUp, signOut, setRole, can, updateProfile]
  );

  if (!hydrated) return null;

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}

export function Can({ perm, children, fallback = null }) {
  const { can } = useAuth();
  return <>{can(perm) ? children : fallback}</>;
}
