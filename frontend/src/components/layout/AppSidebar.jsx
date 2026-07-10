import { Link, useRouterState } from "@tanstack/react-router";
import {
  LayoutDashboard,
  Ticket,
  CheckSquare,
  Boxes,
  Users,
  BarChart3,
  Bell,
  Settings,
  LifeBuoy,
  Headset,
  ShieldCheck,
  User,
} from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";
import { notifications } from "@/data/mock";

import { useAuth } from "@/lib/auth";

const mainItems = [
  { title: "Dashboard", url: "/", icon: LayoutDashboard, perm: "dashboard.view" },
  { title: "Service Requests", url: "/requests/", icon: Ticket, perm: "requests.view" },
  { title: "Approvals", url: "/approvals", icon: CheckSquare, perm: "approvals.view" },
  { title: "Assets", url: "/assets/", icon: Boxes, perm: "assets.view" },
  { title: "Users", url: "/users/", icon: Users, perm: "users.view" },
  { title: "Reports", url: "/reports", icon: BarChart3, perm: "reports.view" },
];

const systemItems = [
  { title: "Profile", url: "/profile", icon: User, perm: "dashboard.view" },
  { title: "Notifications", url: "/notifications", icon: Bell, perm: "notifications.view" },
  { title: "Admin Masters", url: "/masters", icon: ShieldCheck, perm: "users.manage" },
  { title: "Settings", url: "/settings", icon: Settings, perm: "settings.view" },
  { title: "Help Center", url: "/help", icon: LifeBuoy, perm: "help.view" },
];

export function AppSidebar() {
  const { state } = useSidebar();
  const { can } = useAuth();
  const collapsed = state === "collapsed";
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const unread = notifications.filter((n) => !n.read).length;

  const visibleMain = mainItems.filter((i) => can(i.perm));
  const visibleSystem = systemItems.filter((i) => can(i.perm));

  const isActive = (url) =>
    url === "/" ? pathname === "/" : pathname === url || pathname.startsWith(url + "/");

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="px-3 py-4">
        <Link to="/" className="flex items-center gap-2.5 px-1">
          <div className="grid size-9 shrink-0 place-items-center rounded-xl bg-sidebar-primary text-sidebar-primary-foreground">
            <Headset className="size-5" />
          </div>
          {!collapsed && (
            <div className="min-w-0">
              <p className="truncate font-display text-sm font-extrabold leading-tight text-sidebar-accent-foreground">
                IT Service Desk
              </p>
              <p className="truncate text-[10px] text-sidebar-foreground/70">Request Management</p>
            </div>
          )}
        </Link>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Main</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {visibleMain.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={isActive(item.url)} tooltip={item.title}>
                    <Link to={item.url}>
                      <item.icon className="size-4" />
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
        <SidebarGroup>
          <SidebarGroupLabel>System</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {visibleSystem.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={isActive(item.url)} tooltip={item.title}>
                    <Link to={item.url}>
                      <item.icon className="size-4" />
                      <span>{item.title}</span>
                      {item.title === "Notifications" && unread > 0 && !collapsed && (
                        <span className="ml-auto grid size-5 place-items-center rounded-full bg-sidebar-primary text-[10px] font-bold text-sidebar-primary-foreground">
                          {unread}
                        </span>
                      )}
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter className="p-3">
        {!collapsed && (
          <div className="rounded-xl bg-sidebar-accent p-3">
            <p className="text-xs font-semibold text-sidebar-accent-foreground">Need help?</p>
            <p className="mt-0.5 text-[11px] leading-snug text-sidebar-foreground/70">
              Visit the help center for guides and support.
            </p>
            <Link
              to="/help"
              className="mt-2 inline-block rounded-lg bg-sidebar-primary px-2.5 py-1 text-[11px] font-semibold text-sidebar-primary-foreground transition-opacity hover:opacity-90"
            >
              Help Center
            </Link>
          </div>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
