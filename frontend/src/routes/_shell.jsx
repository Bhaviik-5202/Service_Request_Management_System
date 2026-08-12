import { useEffect } from "react";
import { createFileRoute, Outlet, useNavigate, useRouterState } from "@tanstack/react-router";
import { SidebarProvider } from "@/components/ui/sidebar";
import { AppSidebar } from "@/components/layout/AppSidebar";
import { TopBar } from "@/components/layout/TopBar";
import { useAuth } from "@/lib/auth";
import { ShieldAlert } from "lucide-react";

export const Route = createFileRoute("/_shell")({
  component: ShellLayout,
});

// Map route prefixes to required permissions
const ROUTE_PERMS = [
  { prefix: "/approvals", perm: "approvals.view" },
  { prefix: "/assets", perm: "assets.view" },
  { prefix: "/users", perm: "users.view" },
  { prefix: "/reports", perm: "reports.view" },
  { prefix: "/requests/new", perm: "requests.create" },
  { prefix: "/requests", perm: "requests.view" },
  { prefix: "/notifications", perm: "notifications.view" },
  { prefix: "/help", perm: "help.view" },
  { prefix: "/settings", perm: "settings.view" },
  { prefix: "/masters", perm: "users.manage" },
];

function ShellLayout() {
  const { signedIn, can } = useAuth();
  const navigate = useNavigate();
  const pathname = useRouterState({ select: (s) => s.location.pathname });

  useEffect(() => {
    if (!signedIn && pathname !== "/") {
      navigate({ to: "/login", replace: true });
    }
  }, [signedIn, pathname, navigate]);

  const match = ROUTE_PERMS.find(
    (r) => pathname === r.prefix || pathname.startsWith(r.prefix + "/"),
  );
  const allowed = !match || can(match.perm);

  useEffect(() => {
    if (signedIn && !allowed && pathname !== "/unauthorized") {
      navigate({ to: "/unauthorized", replace: true });
    }
  }, [signedIn, allowed, pathname, navigate]);

  if (!signedIn) {
    if (pathname === "/") {
      return <Outlet />;
    }
    return null;
  }

  return (
    <SidebarProvider>
      <div className="flex min-h-screen w-full">
        <AppSidebar />
        <div className="flex min-w-0 flex-1 flex-col">
          <TopBar />
          <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
            <div className="mx-auto w-full max-w-7xl">
              {allowed ? (
                <Outlet />
              ) : (
                <div className="text-center py-20 text-muted-foreground">Redirecting...</div>
              )}
            </div>
          </main>
        </div>
      </div>
    </SidebarProvider>
  );
}

function Forbidden() {
  return (
    <div className="grid place-items-center py-20 text-center">
      <div className="max-w-md">
        <div className="mx-auto grid size-14 place-items-center rounded-2xl bg-destructive/10 text-destructive">
          <ShieldAlert className="size-7" />
        </div>
        <h1 className="mt-4 font-display text-2xl font-bold">Access restricted</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Your role does not have permission to view this page. Contact your administrator or switch
          roles from the top bar.
        </p>
      </div>
    </div>
  );
}
