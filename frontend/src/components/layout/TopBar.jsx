import { useState, useMemo, useEffect } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import {
  Bell,
  LogOut,
  Moon,
  Search,
  Settings,
  Sun,
  User,
} from "lucide-react";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useTheme } from "@/lib/theme";
import { getNotifications } from "@/services/api";
import { useAuth } from "@/lib/auth";
import { cn } from "@/lib/utils";

export function TopBar() {
  const { theme, setTheme } = useTheme();
  const navigate = useNavigate();
  const { role, signOut, user } = useAuth();
  const [panelOpen, setPanelOpen] = useState(false);
  const [notifList, setNotifList] = useState([]);

  useEffect(() => {
    async function loadTopBarNotifs() {
      try {
        const apiNotifs = await getNotifications().catch(() => []);
        setNotifList(
          (apiNotifs || []).map((n) => ({
            id: String(n.notificationId || n.id),
            title: n.title || "Notification",
            message: n.message || "",
            read: n.isRead || false,
            type: n.notificationType || "request",
            time: n.createdAt
              ? new Date(n.createdAt).toLocaleTimeString("en-IN", { hour: "2-digit", minute: "2-digit" })
              : "Just now",
          }))
        );
      } catch {
        // Silently fail — notifications are non-critical for TopBar
      }
    }
    loadTopBarNotifs();
  }, []);

  const unread = notifList.filter((n) => !n.read).length;

  const currentUser = useMemo(() => {
    return {
      name: user?.fullName || user?.name || "User",
      email: user?.email || "",
      avatar: (user?.fullName || user?.name || "U")
        .split(" ")
        .map((n) => n[0])
        .join("")
        .substring(0, 2)
        .toUpperCase(),
      department: user?.department || "",
      role: user?.role || role || "Requestor",
    };
  }, [user, role]);

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/80 px-3 backdrop-blur-md sm:gap-3 sm:px-5">
      <SidebarTrigger />
      <div className="relative hidden max-w-sm flex-1 md:block">
        <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input placeholder="Search requests, assets, users…" className="h-9 rounded-xl pl-9" />
      </div>
      <div className="ml-auto flex items-center gap-1.5">
        <Button
          variant="ghost"
          size="icon"
          className="rounded-xl cursor-pointer"
          onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          aria-label="Toggle theme"
        >
          {theme === "dark" ? <Sun className="size-4.5" /> : <Moon className="size-4.5" />}
        </Button>

        <Sheet open={panelOpen} onOpenChange={setPanelOpen}>
          <SheetTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              className="relative rounded-xl cursor-pointer"
              aria-label="Notifications"
            >
              <Bell className="size-4.5" />
              {unread > 0 && (
                <span className="absolute right-1.5 top-1.5 grid size-4 place-items-center rounded-full bg-destructive text-[9px] font-bold text-destructive-foreground">
                  {unread}
                </span>
              )}
            </Button>
          </SheetTrigger>
          <SheetContent className="w-full sm:max-w-md">
            <SheetHeader>
              <SheetTitle className="font-display">Notifications</SheetTitle>
            </SheetHeader>
            <div className="flex-1 space-y-1 overflow-y-auto px-4 pb-4">
              {notifList.length === 0 && (
                <p className="py-8 text-center text-xs text-muted-foreground">No notifications found.</p>
              )}
              {notifList.map((n) => (
                <div
                  key={n.id}
                  className={cn(
                    "flex gap-3 rounded-xl p-3 transition-colors hover:bg-accent",
                    !n.read && "bg-primary/5",
                  )}
                >
                  <div className="grid size-9 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                    <Bell className="size-4" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-semibold leading-tight">
                      {n.title}
                      {!n.read && (
                        <span className="ml-2 inline-block size-2 rounded-full bg-primary" />
                      )}
                    </p>
                    <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">
                      {n.message}
                    </p>
                    <p className="mt-1 text-[11px] text-muted-foreground/70">{n.time}</p>
                  </div>
                </div>
              ))}
              <Button
                variant="outline"
                className="mt-2 w-full rounded-xl cursor-pointer"
                onClick={() => {
                  setPanelOpen(false);
                  navigate({ to: "/notifications" });
                }}
              >
                View all notifications
              </Button>
            </div>
          </SheetContent>
        </Sheet>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="h-9 gap-2 rounded-xl px-2 cursor-pointer">
              <Avatar className="size-6">
                <AvatarFallback className="bg-primary/10 text-[10px] font-bold text-primary">
                  {currentUser.avatar}
                </AvatarFallback>
              </Avatar>
              <span className="hidden text-xs font-semibold sm:inline-block">{currentUser.name}</span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56 rounded-2xl">
            <DropdownMenuLabel className="font-normal">
              <div className="flex flex-col space-y-1">
                <p className="text-sm font-bold leading-none">{currentUser.name}</p>
                <p className="text-xs leading-none text-muted-foreground">{currentUser.email}</p>
                <p className="text-[11px] text-muted-foreground">{currentUser.role}</p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem asChild className="cursor-pointer rounded-xl">
              <Link to="/profile">
                <User className="mr-2 size-4" /> Profile Settings
              </Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild className="cursor-pointer rounded-xl">
              <Link to="/settings">
                <Settings className="mr-2 size-4" /> System Settings
              </Link>
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => {
                signOut();
                navigate({ to: "/login" });
              }}
              className="text-destructive cursor-pointer rounded-xl focus:bg-destructive/10 focus:text-destructive"
            >
              <LogOut className="mr-2 size-4" /> Log out
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
