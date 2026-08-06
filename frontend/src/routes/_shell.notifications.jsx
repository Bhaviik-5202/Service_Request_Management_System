import { useState, useEffect } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { motion } from "motion/react";
import { Bell, CheckCheck, Plus, CheckSquare, Boxes, Cog, Loader2 } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import { getNotifications, markNotificationRead, getAuditLogs } from "@/services/api";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { cn } from "@/lib/utils";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/notifications")({
  component: NotificationsPage,
});

const typeIcon = { request: Plus, approval: CheckSquare, asset: Boxes, system: Cog };

function NotificationsPage() {
  const [items, setItems] = useState([]);
  const [activityList, setActivityList] = useState([]);
  const [loading, setLoading] = useState(true);

  const loadData = async () => {
    setLoading(true);
    try {
      const [apiNotifs, apiLogs] = await Promise.all([
        getNotifications().catch(() => []),
        getAuditLogs().catch(() => []),
      ]);

      const mappedNotifs = (apiNotifs || []).map((n) => ({
        id: String(n.notificationId || n.id),
        notificationId: n.notificationId,
        title: n.title || "Notification",
        message: n.message || "",
        read: n.isRead || false,
        type: n.notificationType || "request",
        time: n.createdAt
          ? new Date(n.createdAt).toLocaleTimeString("en-IN", { hour: "2-digit", minute: "2-digit" })
          : "Just now",
      }));

      const mappedLogs = (apiLogs || []).map((l) => ({
        id: String(l.auditLogId || l.id),
        actor: l.actorUser?.fullName || "System",
        action: l.action || "performed operation",
        target: l.targetDisplay || l.targetType || "Record",
        time: l.createdAt
          ? new Date(l.createdAt).toLocaleString("en-IN", { dateStyle: "medium", timeStyle: "short" })
          : "Recently",
      }));

      setItems(mappedNotifs);
      setActivityList(mappedLogs);
    } catch {
      // Gracefully show empty state
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const unread = items.filter((n) => !n.read).length;

  const markAllRead = async () => {
    try {
      const unreadItems = items.filter((n) => !n.read);
      await Promise.all(unreadItems.map((n) => markNotificationRead(n.notificationId || n.id).catch(() => null)));
      setItems((prev) => prev.map((n) => ({ ...n, read: true })));
      toast.success("All notifications marked as read");
    } catch {
      toast.error("Failed to mark notifications read.");
    }
  };

  const handleReadSingle = async (n) => {
    if (!n.read) {
      try {
        await markNotificationRead(n.notificationId || n.id).catch(() => null);
        setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, read: true } : x)));
      } catch {
        // Silently ignore single-read failures
      }
    }
  };

  return (
    <div className="mx-auto max-w-3xl">
      <PageHeader
        title="Notifications"
        description={unread > 0 ? `${unread} unread notifications` : "You're all caught up"}
        crumbs={[{ label: "Notifications" }]}
        actions={
          <Button
            variant="outline"
            className="rounded-xl cursor-pointer"
            onClick={markAllRead}
            disabled={unread === 0}
          >
            <CheckCheck className="mr-1.5 size-4" /> Mark all read
          </Button>
        }
      />

      <Tabs defaultValue="all">
        <TabsList className="rounded-xl">
          <TabsTrigger value="all" className="rounded-lg cursor-pointer">
            All
            {unread > 0 && (
              <span className="ml-1.5 grid size-5 place-items-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground">
                {unread}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="activity" className="rounded-lg cursor-pointer">
            Activity Timeline
          </TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-4 space-y-2">
          {loading && (
            <div className="py-12 text-center text-sm text-muted-foreground">
              <Loader2 className="size-6 animate-spin mx-auto mb-2 text-primary" />
              Loading notifications…
            </div>
          )}

          {!loading && items.length === 0 && (
            <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-12 text-center shadow-card">
              <Bell className="mx-auto size-8 text-muted-foreground" />
              <p className="mt-3 text-sm text-muted-foreground">No notifications yet.</p>
            </div>
          )}

          {!loading && items.map((n, i) => {
            const Icon = typeIcon[n.type] || Bell;
            return (
              <motion.button
                key={n.id}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.04 }}
                onClick={() => handleReadSingle(n)}
                className={cn(
                  "flex w-full gap-3 rounded-2xl border bg-card/40 backdrop-blur-md p-4 text-left shadow-card transition-colors hover:bg-accent/50 cursor-pointer",
                  !n.read && "border-primary/30 bg-primary/5",
                )}
              >
                <div className="grid size-10 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                  <Icon className="size-4.5" />
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <p className="truncate text-sm font-bold">{n.title}</p>
                    {!n.read && <span className="size-2 shrink-0 rounded-full bg-primary" />}
                    <span className="ml-auto shrink-0 text-[11px] text-muted-foreground">
                      {n.time}
                    </span>
                  </div>
                  <p className="mt-0.5 text-sm text-muted-foreground">{n.message}</p>
                </div>
              </motion.button>
            );
          })}
        </TabsContent>

        <TabsContent value="activity" className="mt-4">
          <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card">
            <div className="relative space-y-6 before:absolute before:inset-y-1 before:left-4 before:w-px before:bg-border">
              {loading && (
                <div className="py-12 text-center text-sm text-muted-foreground">
                  <Loader2 className="size-6 animate-spin mx-auto mb-2 text-primary" />
                  Loading activity log…
                </div>
              )}

              {!loading && activityList.length === 0 && (
                <div className="py-8 text-center text-sm text-muted-foreground">
                  No activity log entries found.
                </div>
              )}

              {!loading && activityList.map((a, i) => (
                <motion.div
                  key={a.id}
                  initial={{ opacity: 0, x: -8 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: i * 0.06 }}
                  className="relative flex gap-4 pl-0"
                >
                  <Avatar className="z-10 size-8 shrink-0 ring-4 ring-card">
                    <AvatarFallback className="bg-accent text-[10px] font-bold text-accent-foreground">
                      {a.actor
                        .split(" ")
                        .map((w) => w[0])
                        .join("")}
                    </AvatarFallback>
                  </Avatar>
                  <div className="min-w-0 pb-1">
                    <p className="text-sm leading-snug">
                      <span className="font-semibold">{a.actor}</span>{" "}
                      <span className="text-muted-foreground">{a.action}</span>{" "}
                      <span className="font-medium text-primary">{a.target}</span>
                    </p>
                    <p className="mt-0.5 text-xs text-muted-foreground">{a.time}</p>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
