import { useState, useMemo } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import {
  Bell,
  LogOut,
  Moon,
  Search,
  Settings,
  Sun,
  User,
  Plus,
  CheckSquare,
  Boxes,
  Cog,
  ShieldCheck,
  UserCog,
  Wrench,
  User as UserIcon,
  Check,
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
import { notifications } from "@/data/mock";
import { useAuth, ROLE_PROFILES } from "@/lib/auth";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const roleIcon = {
  Admin: ShieldCheck,
  HOD: UserCog,
  Technician: Wrench,
  Requestor: UserIcon,
};

const typeIcon = {
  request: Plus,
  approval: CheckSquare,
  asset: Boxes,
  system: Cog,
};

export function TopBar() {
  const { theme, setTheme } = useTheme();
  const navigate = useNavigate();
  const { role, setRole, signOut, user } = useAuth();
  const [panelOpen, setPanelOpen] = useState(false);
  const unread = notifications.filter((n) => !n.read).length;
  const activeRole = role ?? "Admin";

  const currentUser = useMemo(() => {
    if (user) {
      return {
        name: user.name,
        email: user.email,
        avatar: user.name
          .split(" ")
          .map((n) => n[0])
          .join("")
          .toUpperCase(),
        department: user.department,
        role: user.role,
      };
    }
    return { ...ROLE_PROFILES[activeRole], role: activeRole };
  }, [user, activeRole]);

  const RoleIcon = roleIcon[activeRole];

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
          className="rounded-xl"
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
              className="relative rounded-xl"
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
              {notifications.map((n) => {
                const Icon = typeIcon[n.type];
                return (
                  <div
                    key={n.id}
                    className={cn(
                      "flex gap-3 rounded-xl p-3 transition-colors hover:bg-accent",
                      !n.read && "bg-primary/5",
                    )}
                  >
                    <div className="grid size-9 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                      <Icon className="size-4" />
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
                );
              })}
              <Button
                variant="outline"
                className="mt-2 w-full rounded-xl"
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

        <div className="mr-1 hidden items-center gap-1.5 rounded-xl border bg-muted/40 px-2 py-1 sm:flex">
          <RoleIcon className="size-3.5 text-primary" />
          <span className="text-[11px] font-semibold">{activeRole}</span>
        </div>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button className="ml-1 flex items-center gap-2 rounded-xl p-1 transition-colors hover:bg-accent">
              <Avatar className="size-8">
                <AvatarFallback className="bg-primary text-xs font-bold text-primary-foreground">
                  {currentUser.avatar}
                </AvatarFallback>
              </Avatar>
              <div className="hidden text-left lg:block">
                <p className="text-xs font-semibold leading-tight">{currentUser.name}</p>
                <p className="text-[11px] text-muted-foreground">{currentUser.role}</p>
              </div>
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-60 rounded-xl">
            <DropdownMenuLabel>
              <p className="text-sm font-semibold">{currentUser.name}</p>
              <p className="text-xs font-normal text-muted-foreground">{currentUser.email}</p>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem asChild>
              <Link to="/profile">
                <User className="mr-2 size-4" /> Profile
              </Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild>
              <Link to="/settings">
                <Settings className="mr-2 size-4" /> Settings
              </Link>
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => {
                signOut();
                navigate({ to: "/login" });
              }}
              className="cursor-pointer text-destructive focus:text-destructive"
            >
              <LogOut className="mr-2 size-4" /> Sign out
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
