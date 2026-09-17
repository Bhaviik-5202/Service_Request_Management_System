import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { users, syncLocalStorage, addUser, updateUser } from "@/data/mock";
import api from "@/lib/api";

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

export const ROLE_PROFILES = {
  Admin: {
    name: "Bhaviik Parmar",
    email: "admin@gmail.com",
    avatar: "BP",
    department: "IT Services",
  },
  HOD: { name: "Karan Patel", email: "hod@gmail.com", avatar: "KP", department: "Operations" },
  Technician: {
    name: "Ronak",
    email: "tech@gmail.com",
    avatar: "RK",
    department: "IT",
  },
  Requestor: {
    name: "Priya Sharma",
    email: "requestor@gmail.com",
    avatar: "PS",
    department: "Sales",
  },
};

const AuthContext = createContext(null);
const STORAGE_KEY = "servicedesk.auth";

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [role, setRoleState] = useState(null);
  const [token, setToken] = useState(null);
  const [signedIn, setSignedIn] = useState(false);
  const [hydrated, setHydrated] = useState(false);

  // Hydrate session on mount
  useEffect(() => {
    async function hydrate() {
      try {
        syncLocalStorage();

        let raw = sessionStorage.getItem(STORAGE_KEY) || localStorage.getItem(STORAGE_KEY);

        if (raw) {
          const parsed = JSON.parse(raw);
          if (parsed.signedIn) {
            if (parsed.token) {
              setToken(parsed.token);
              try {
                const res = await api.auth.me();
                if (res?.success && res.data) {
                  const dbUser = {
                    id: String(res.data.userId),
                    userId: res.data.userId,
                    name: res.data.fullName,
                    email: res.data.email,
                    role: res.data.role,
                    department: res.data.departmentName || "IT",
                    departmentId: res.data.departmentId,
                    phone: res.data.phone || "",
                    avatar: res.data.fullName
                      ?.split(" ")
                      .map((n) => n[0])
                      .join("")
                      .toUpperCase() || "US",
                  };
                  setUser(dbUser);
                  setRoleState(res.data.role);
                  setSignedIn(true);
                  setHydrated(true);
                  return;
                }
              } catch {
                // Fall back to local cache if offline
              }
            }

            let matchedUser;
            if (parsed.userId) {
              matchedUser = users.find((u) => u.id === parsed.userId);
            } else {
              const defaultEmails = {
                Admin: "admin@gmail.com",
                HOD: "hod@gmail.com",
                Technician: "tech@gmail.com",
                Requestor: "requestor@gmail.com",
              };
              const email = defaultEmails[parsed.role];
              matchedUser = users.find((u) => u.email.toLowerCase() === email.toLowerCase());
            }

            if (matchedUser) {
              setUser(matchedUser);
              setRoleState(matchedUser.role);
              setSignedIn(true);
            }
          }
        }
      } catch {
        /* ignore */
      }
      setHydrated(true);
    }

    hydrate();

    const handleUnauthorized = () => {
      signOut();
    };
    window.addEventListener("auth:unauthorized", handleUnauthorized);
    return () => window.removeEventListener("auth:unauthorized", handleUnauthorized);
  }, []);

  const signIn = useCallback(async (emailOrRole, password, remember = true) => {
    syncLocalStorage();

    const roles = ["Admin", "HOD", "Technician", "Requestor"];
    const isRole = roles.includes(emailOrRole);

    let email = emailOrRole;
    let pwd = password || "admin123";

    if (isRole) {
      const defaultEmails = {
        Admin: "admin@gmail.com",
        HOD: "hod@gmail.com",
        Technician: "tech@gmail.com",
        Requestor: "requestor@gmail.com",
      };
      email = defaultEmails[emailOrRole];
      pwd = "admin123";
    }

    // Try backend authentication first
    try {
      const res = await api.auth.login({ email, password: pwd });
      if (res?.success && res.data?.token) {
        const authData = res.data;
        const loggedUser = {
          id: String(authData.userId),
          userId: authData.userId,
          name: authData.fullName,
          email: authData.email,
          role: authData.role,
          department: authData.departmentName || "IT",
          departmentId: authData.departmentId,
          avatar: authData.fullName
            ?.split(" ")
            .map((n) => n[0])
            .join("")
            .toUpperCase() || "US",
        };

        setUser(loggedUser);
        setRoleState(authData.role);
        setToken(authData.token);
        setSignedIn(true);

        const sessionData = {
          userId: loggedUser.id,
          role: loggedUser.role,
          token: authData.token,
          signedIn: true,
        };

        if (remember) {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
          sessionStorage.removeItem(STORAGE_KEY);
        } else {
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
          localStorage.removeItem(STORAGE_KEY);
        }
        return true;
      }
    } catch {
      // If API server is unreachable, fall back to local store
    }

    // Local Mock fallback
    let matchedUser;
    if (isRole) {
      matchedUser = users.find((u) => u.email.toLowerCase() === email.toLowerCase());
    } else if (password) {
      matchedUser = users.find(
        (u) =>
          u.email.toLowerCase() === emailOrRole.trim().toLowerCase() && u.password === password,
      );
    }

    if (matchedUser) {
      setUser(matchedUser);
      setRoleState(matchedUser.role);
      setSignedIn(true);

      const sessionData = { userId: matchedUser.id, role: matchedUser.role, signedIn: true };
      if (remember) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        sessionStorage.removeItem(STORAGE_KEY);
      } else {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        localStorage.removeItem(STORAGE_KEY);
      }
      return true;
    }

    return false;
  }, []);

  const signUp = useCallback(async (userData) => {
    syncLocalStorage();

    // Try backend registration
    try {
      const res = await api.auth.register({
        fullName: userData.name || userData.fullName,
        email: userData.email,
        password: userData.password || "Password@123",
        role: userData.role || "Requestor",
        phone: userData.phone || "",
      });

      if (res?.success && res.data?.token) {
        const authData = res.data;
        const newUser = {
          id: String(authData.userId),
          userId: authData.userId,
          name: authData.fullName,
          email: authData.email,
          role: authData.role,
          department: authData.departmentName || "IT",
          avatar: authData.fullName
            ?.split(" ")
            .map((n) => n[0])
            .join("")
            .toUpperCase() || "US",
        };

        setUser(newUser);
        setRoleState(newUser.role);
        setToken(authData.token);
        setSignedIn(true);

        const sessionData = {
          userId: newUser.id,
          role: newUser.role,
          token: authData.token,
          signedIn: true,
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
        sessionStorage.removeItem(STORAGE_KEY);
        return true;
      }
    } catch {
      // Fallback to local
    }

    const duplicate = users.find((u) => u.email.toLowerCase() === userData.email.toLowerCase());
    if (duplicate) {
      return false;
    }

    const newUser = {
      ...userData,
      id: String(Date.now()),
      joined: new Date().toISOString().split("T")[0],
      requestsRaised: 0,
      requestsResolved: 0,
    };

    addUser(newUser);
    setUser(newUser);
    setRoleState(newUser.role);
    setSignedIn(true);

    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ userId: newUser.id, role: newUser.role, signedIn: true }),
    );
    sessionStorage.removeItem(STORAGE_KEY);

    return true;
  }, []);

  const signOut = useCallback(() => {
    setUser(null);
    setRoleState(null);
    setToken(null);
    setSignedIn(false);
    localStorage.removeItem(STORAGE_KEY);
    sessionStorage.removeItem(STORAGE_KEY);
  }, []);

  const setRole = useCallback((r) => {
    syncLocalStorage();
    const defaultEmails = {
      Admin: "admin@gmail.com",
      HOD: "hod@gmail.com",
      Technician: "tech@gmail.com",
      Requestor: "requestor@gmail.com",
    };
    const email = defaultEmails[r];
    const matchedUser = users.find((u) => u.email.toLowerCase() === email.toLowerCase());

    if (matchedUser) {
      setUser(matchedUser);
      setRoleState(r);

      const sessionData = { userId: matchedUser.id, role: r, signedIn: true };
      if (sessionStorage.getItem(STORAGE_KEY)) {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
      } else {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
      }
    }
  }, []);

  const can = useCallback((p) => (role ? ROLE_PERMISSIONS[role]?.includes(p) || false : false), [role]);

  const updateProfile = useCallback((updatedUserData) => {
    syncLocalStorage();
    const updatedUser = { ...user, ...updatedUserData };
    setUser(updatedUser);

    const sessionData = { userId: updatedUser.id, role: updatedUser.role, token, signedIn: true };
    if (sessionStorage.getItem(STORAGE_KEY)) {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
    } else {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionData));
    }

    updateUser(updatedUser);
  }, [user, token]);

  const value = useMemo(
    () => ({ user, role, token, signedIn, signIn, signUp, signOut, setRole, can, updateProfile }),
    [user, role, token, signedIn, signIn, signUp, signOut, setRole, can, updateProfile],
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
