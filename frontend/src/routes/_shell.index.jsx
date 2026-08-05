import { useMemo, useEffect, useState } from "react";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { motion } from "motion/react";
import {
  Ticket,
  FolderOpen,
  Loader,
  CheckCircle2,
  Plus,
  CheckSquare,
  Boxes,
  BarChart3,
  ArrowRight,
  Clock,
  Award,
  Users,
  Building,
  Check,
  X,
  MessageSquare,
  Play,
  Wrench,
  RefreshCcw,
  FileText,
  Loader2,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatCard } from "@/components/shared/StatCard";
import { StatusBadge, PriorityBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  getServiceRequests,
  getApprovals,
  getUsers,
  getAuditLogs,
  updateServiceRequest,
  updateApproval,
  createServiceRequestReply,
} from "@/services/api";
import { useAuth } from "@/lib/auth";
import { toast } from "sonner";
import { LandingPage } from "@/components/shared/LandingPage";

export const Route = createFileRoute("/_shell/")({
  component: IndexRoute,
});

function IndexRoute() {
  const { signedIn } = useAuth();

  if (!signedIn) {
    return <LandingPage />;
  }

  return <Dashboard />;
}

const PIE_COLORS = [
  "var(--info)",
  "var(--warning)",
  "var(--primary)",
  "var(--success)",
  "var(--muted-foreground)",
];

function Dashboard() {
  const { role, user } = useAuth();
  const navigate = useNavigate();

  const [requestsList, setRequestsList] = useState([]);
  const [approvalsList, setApprovalsList] = useState([]);
  const [usersList, setUsersList] = useState([]);
  const [auditLogsList, setAuditLogsList] = useState([]);
  const [loading, setLoading] = useState(true);

  const activeProfile = useMemo(() => {
    if (user) {
      return {
        name: user.name || user.fullName,
        email: user.email,
        department: user.department,
        role: user.role,
      };
    }
    return role ? { name: user?.fullName || user?.name || "", email: user?.email || "", department: user?.department || "", role } : { name: "System User", email: "user@company.com", department: "IT", role: "Admin" };
  }, [user, role]);

  const loadDashboardData = async () => {
    setLoading(true);
    try {
      const [apiRequests, apiApprovals, apiUsers, apiLogs] = await Promise.all([
        getServiceRequests().catch(() => []),
        getApprovals().catch(() => []),
        getUsers().catch(() => []),
        getAuditLogs().catch(() => []),
      ]);

      const mappedReqs = (apiRequests || []).map((sr) => ({
        id: String(sr.requestId),
        no: sr.requestNumber || `SR-${sr.requestId}`,
        title: sr.title,
        description: sr.description,
        serviceType: sr.serviceType?.serviceTypeName || "Technical",
        requestType: sr.requestType?.requestTypeName || "Support",
        department: sr.department?.departmentName || "IT",
        requester: sr.requesterUser ? (sr.requesterUser.fullName || sr.requesterUser.name) : "Requester",
        requesterEmail: sr.requesterUser?.email || "",
        assignee: sr.assigneeUser ? (sr.assigneeUser.fullName || sr.assigneeUser.name) : null,
        status: sr.status?.statusName || "Pending",
        priority: sr.priority || "Medium",
        created: sr.createdAt ? new Date(sr.createdAt).toISOString() : new Date().toISOString(),
        updated: sr.updatedAt ? new Date(sr.updatedAt).toISOString() : new Date().toISOString(),
        replies: (sr.replies || []).map((rep) => ({
          id: String(rep.replyId || rep.id),
          author: rep.authorUser ? (rep.authorUser.fullName || rep.authorUser.name) : (rep.author || "User"),
          role: rep.authorUser?.role?.roleName || rep.role || "User",
          message: rep.message,
          date: rep.createdAt ? new Date(rep.createdAt).toISOString() : new Date().toISOString(),
        })),
        raw: sr,
      }));

      const mappedApps = (apiApprovals || []).map((a) => ({
        id: String(a.approvalId),
        approvalId: a.approvalId,
        requestId: String(a.requestId),
        requestNo: a.serviceRequest?.requestNumber || `SR-${a.requestId}`,
        title: a.serviceRequest?.title || "Service Request",
        requester: a.serviceRequest?.requesterUser?.fullName || "Requester",
        department: a.serviceRequest?.department?.departmentName || "IT",
        priority: a.serviceRequest?.priority || "Medium",
        status: a.status || "Pending",
        decidedBy: a.decidedByUser ? (a.decidedByUser.fullName || a.decidedByUser.name) : "HOD",
        submitted: a.submittedAt ? new Date(a.submittedAt).toISOString() : new Date().toISOString(),
        remarks: a.remarks || "",
        raw: a,
      }));

      setRequestsList(mappedReqs);
      setApprovalsList(mappedApps);
      setUsersList(apiUsers || []);
      setAuditLogsList(apiLogs || []);
    } catch (err) {
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDashboardData();
  }, []);

  // Filter requests based on user role
  const userRequests = useMemo(() => {
    if (role === "Requestor") {
      return requestsList.filter((r) => activeProfile.email && r.requesterEmail.toLowerCase() === activeProfile.email.toLowerCase());
    }
    if (role === "Technician") {
      return requestsList.filter((r) => r.assignee && r.assignee.toLowerCase() === activeProfile.name.toLowerCase());
    }
    if (role === "HOD") {
      return requestsList.filter((r) => r.department === activeProfile.department);
    }
    return requestsList;
  }, [role, activeProfile, requestsList]);

  // Compute stats dynamically
  const totalCount = userRequests.length;
  const pendingCount = userRequests.filter(
    (r) => r.status === "Pending" || r.status === "Assigned" || r.status === "In Progress",
  ).length;
  const completedCount = userRequests.filter(
    (r) => r.status === "Completed" || r.status === "Closed",
  ).length;
  const highPriorityCount = userRequests.filter(
    (r) => r.priority === "High" || r.priority === "Critical",
  ).length;

  const dynamicKpis = [
    {
      title: "Total Requests",
      value: totalCount,
      icon: Ticket,
      trend: "All tickets in your scope",
      trendUp: true,
      iconClass: "bg-primary/10 text-primary",
    },
    {
      title: "Pending Requests",
      value: pendingCount,
      icon: Loader,
      trend: "Awaiting action",
      trendUp: false,
      iconClass: "bg-warning/15 text-warning-foreground dark:text-warning",
    },
    {
      title: "Completed Requests",
      value: completedCount,
      icon: CheckCircle2,
      trend: "Completed & Closed",
      trendUp: true,
      iconClass: "bg-success/10 text-success",
    },
    {
      title: "High Priority",
      value: highPriorityCount,
      icon: FolderOpen,
      trend: "Critical & High priority",
      trendUp: true,
      iconClass: "bg-destructive/10 text-destructive",
    },
  ];

  // Dynamic status distribution pie chart data
  const dynamicStatusDistribution = useMemo(() => {
    const counts = {
      Pending: 0,
      Assigned: 0,
      "In Progress": 0,
      Completed: 0,
      Closed: 0,
      Cancelled: 0,
    };
    userRequests.forEach((r) => {
      if (counts[r.status] !== undefined) {
        counts[r.status]++;
      }
    });
    return Object.keys(counts)
      .map((key) => ({
        name: key,
        value: counts[key],
      }))
      .filter((item) => item.value > 0);
  }, [userRequests]);

  // Dynamic recent requests list
  const recent = useMemo(() => {
    return userRequests.slice(0, 5);
  }, [userRequests]);

  // Filter recent activities from audit logs
  const filteredActivity = useMemo(() => {
    return (auditLogsList || []).map((l) => ({
      id: String(l.auditLogId || l.id),
      actor: l.user?.fullName || l.userName || "User",
      action: l.action || "performed action",
      target: l.entityName || "Record",
      detail: l.details || "",
      time: l.timestamp ? new Date(l.timestamp).toLocaleTimeString("en-IN", { hour: "2-digit", minute: "2-digit" }) : "Recently",
    })).slice(0, 5);
  }, [auditLogsList]);

  const quickActions = useMemo(() => {
    const list = [];
    if (role === "Requestor" || role === "Admin") {
      list.push({
        title: "New Request",
        desc: "Raise a service request",
        icon: Plus,
        to: "/requests/new",
      });
    }
    if (role === "HOD" || role === "Admin") {
      list.push({
        title: "Approvals",
        desc: "Decide pending requests",
        icon: CheckSquare,
        to: "/approvals",
      });
    }
    if (role === "Admin" || role === "HOD" || role === "Technician") {
      list.push({ title: "Assets", desc: "Browse asset inventory", icon: Boxes, to: "/assets" });
    }
    if (role === "Admin" || role === "HOD") {
      list.push({ title: "Reports", desc: "Analytics & exports", icon: BarChart3, to: "/reports" });
    }
    return list;
  }, [role]);

  const greetingDate = new Date().toLocaleDateString("en-IN", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const [time, setTime] = useState(new Date());
  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);
  const formattedTime = time.toLocaleTimeString("en-IN", {
    hour: "numeric",
    minute: "2-digit",
    second: "2-digit",
    hour12: true,
  });
  const hour = time.getHours();
  const timeGreeting = useMemo(() => {
    if (hour < 12) return "Good Morning";
    if (hour < 18) return "Good Afternoon";
    return "Good Evening";
  }, [hour]);

  const stats = useMemo(() => {
    const total = requestsList.length;
    const pending = requestsList.filter((r) => r.status === "Pending").length;
    const assigned = requestsList.filter((r) => r.status === "Assigned").length;
    const inProgress = requestsList.filter((r) => r.status === "In Progress").length;
    const completed = requestsList.filter((r) => r.status === "Completed").length;
    const closed = requestsList.filter((r) => r.status === "Closed").length;
    const activeUsersCount = usersList.filter((u) => u.status === "Active" || !u.isDeleted).length;

    return {
      total,
      pending: pending + assigned,
      inProgress,
      completed,
      closed,
      activeUsersCount,
    };
  }, [requestsList, usersList]);

  const [remarks, setRemarks] = useState("");
  const [hodApprovalDialog, setHodApprovalDialog] = useState(null);

  const deptApprovals = useMemo(() => {
    return approvalsList.filter(
      (a) => a.status === "Pending" && a.department === activeProfile.department,
    );
  }, [approvalsList, activeProfile]);

  const handleHodDecide = async (action) => {
    if (!hodApprovalDialog) return;
    try {
      await updateApproval(hodApprovalDialog.approval.approvalId, {
        approvalId: Number(hodApprovalDialog.approval.approvalId),
        requestId: Number(hodApprovalDialog.approval.requestId),
        status: action,
        decidedByUserId: user?.userId || 1,
        decidedAt: new Date().toISOString(),
        remarks: remarks || "",
      });
      toast.success(`Request ${hodApprovalDialog.approval.requestNo} ${action.toLowerCase()}`);
      await loadDashboardData();
    } catch (err) {
      toast.error(err.message || "Approval decision failed.");
    } finally {
      setHodApprovalDialog(null);
      setRemarks("");
    }
  };

  const handleClaim = async (req) => {
    try {
      await updateServiceRequest(req.id, {
        ...req.raw,
        assigneeUserId: user?.userId || 1,
        statusId: 2, // Assigned
        updatedByUserId: user?.userId || 1,
      });

      await createServiceRequestReply({
        requestId: Number(req.id),
        authorUserId: user?.userId || 1,
        message: `Technician ${activeProfile.name} claimed this request.`,
      }).catch(() => null);

      toast.success(`Claimed ticket ${req.no}!`);
      await loadDashboardData();
    } catch (err) {
      toast.error(err.message || "Failed to claim request.");
    }
  };

  const handleTechStartWork = async (req) => {
    try {
      await updateServiceRequest(req.id, {
        ...req.raw,
        statusId: 3, // In Progress
        updatedByUserId: user?.userId || 1,
      });

      await createServiceRequestReply({
        requestId: Number(req.id),
        authorUserId: user?.userId || 1,
        message: "Started working on this request.",
      }).catch(() => null);

      toast.success(`Started work on ${req.no}`);
      await loadDashboardData();
    } catch (err) {
      toast.error(err.message || "Failed to start work.");
    }
  };

  const handleTechComplete = async (req) => {
    try {
      await updateServiceRequest(req.id, {
        ...req.raw,
        statusId: 4, // Completed
        updatedByUserId: user?.userId || 1,
      });

      await createServiceRequestReply({
        requestId: Number(req.id),
        authorUserId: user?.userId || 1,
        message: "Marked this request as Completed.",
      }).catch(() => null);

      toast.success(`Marked ${req.no} as Completed!`);
      await loadDashboardData();
    } catch (err) {
      toast.error(err.message || "Failed to complete request.");
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <PageHeader
            title={`${timeGreeting}, ${activeProfile.name}! 👋`}
            description={`${role} Dashboard · ${greetingDate}`}
          />
        </div>
        <div className="flex items-center gap-2 self-start sm:self-auto">
          <div className="hidden sm:flex items-center gap-2 rounded-xl bg-card/60 border px-3 py-1.5 backdrop-blur-md shadow-card">
            <Clock className="size-4 text-primary animate-pulse" />
            <span className="font-mono text-xs font-bold">{formattedTime}</span>
          </div>
          {quickActions.map((act) => (
            <Button
              key={act.title}
              asChild
              className="rounded-xl gap-1.5 shadow-sm hover:shadow transition-all cursor-pointer"
            >
              <Link to={act.to}>
                <act.icon className="size-4" />
                <span className="hidden sm:inline">{act.title}</span>
              </Link>
            </Button>
          ))}
        </div>
      </div>

      {loading && (
        <div className="py-12 text-center text-sm text-muted-foreground">
          <Loader2 className="size-8 animate-spin mx-auto mb-2 text-primary" />
          Loading dashboard metrics from server...
        </div>
      )}

      {!loading && (
        <>
          {/* Dynamic KPI Cards */}
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            {dynamicKpis.map((kpi, i) => (
              <StatCard key={kpi.title} index={i} {...kpi} />
            ))}
          </div>

          {/* Quick Actions Bar */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            {quickActions.map((act) => (
              <Link
                key={act.title}
                to={act.to}
                className="group flex items-center gap-3 rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card transition-all hover:bg-card/70 hover:shadow-lg hover:border-primary/30"
              >
                <div className="grid size-10 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary transition-transform group-hover:scale-110">
                  <act.icon className="size-5" />
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-extrabold text-foreground group-hover:text-primary transition-colors">
                    {act.title}
                  </p>
                  <p className="truncate text-[11px] text-muted-foreground">{act.desc}</p>
                </div>
              </Link>
            ))}
          </div>

          {/* Main Dashboard Content */}
          <div className="grid gap-6 lg:grid-cols-3">
            {/* Status Distribution Pie Chart */}
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card lg:col-span-1"
            >
              <h3 className="font-display text-base font-bold">Request Status Overview</h3>
              <p className="text-xs text-muted-foreground">Distribution across status categories</p>
              <div className="mt-4 h-64 w-full">
                {dynamicStatusDistribution.length === 0 ? (
                  <div className="h-full flex items-center justify-center text-xs text-muted-foreground">
                    No request data available
                  </div>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={dynamicStatusDistribution}
                        cx="50%"
                        cy="50%"
                        innerRadius={50}
                        outerRadius={80}
                        paddingAngle={4}
                        dataKey="value"
                      >
                        {dynamicStatusDistribution.map((entry, index) => (
                          <Cell
                            key={`cell-${index}`}
                            fill={PIE_COLORS[index % PIE_COLORS.length]}
                          />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "var(--card)",
                          borderColor: "var(--border)",
                          borderRadius: "0.75rem",
                          fontSize: "12px",
                        }}
                      />
                      <Legend
                        verticalAlign="bottom"
                        height={36}
                        iconType="circle"
                        formatter={(value) => (
                          <span className="text-xs font-medium text-foreground">{value}</span>
                        )}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                )}
              </div>
            </motion.div>

            {/* Recent Requests Table */}
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card lg:col-span-2 flex flex-col"
            >
              <div className="flex items-center justify-between border-b p-5 pb-4">
                <div>
                  <h3 className="font-display text-base font-bold">Recent Service Requests</h3>
                  <p className="text-xs text-muted-foreground">Latest tickets in your scope</p>
                </div>
                <Button asChild variant="ghost" size="sm" className="rounded-xl text-xs gap-1 cursor-pointer">
                  <Link to="/requests">
                    View All <ArrowRight className="size-3" />
                  </Link>
                </Button>
              </div>

              <div className="overflow-x-auto flex-1">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-24">ID</TableHead>
                      <TableHead>Title</TableHead>
                      <TableHead className="hidden sm:table-cell">Priority</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {recent.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={4} className="py-10 text-center text-xs text-muted-foreground">
                          No requests found.
                        </TableCell>
                      </TableRow>
                    ) : (
                      recent.map((r) => (
                        <TableRow key={r.id} className="group">
                          <TableCell className="font-mono text-xs font-semibold text-primary">
                            <Link to="/requests/$requestId" params={{ requestId: r.id }}>
                              {r.no}
                            </Link>
                          </TableCell>
                          <TableCell>
                            <Link to="/requests/$requestId" params={{ requestId: r.id }} className="block">
                              <p className="font-semibold text-sm group-hover:text-primary transition-colors truncate max-w-[200px] sm:max-w-xs">
                                {r.title}
                              </p>
                              <p className="text-[11px] text-muted-foreground sm:hidden">{r.requester}</p>
                            </Link>
                          </TableCell>
                          <TableCell className="hidden sm:table-cell">
                            <PriorityBadge priority={r.priority} />
                          </TableCell>
                          <TableCell>
                            <StatusBadge status={r.status} />
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </div>
            </motion.div>
          </div>

          {/* Activity Log Feed */}
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <div className="flex items-center justify-between border-b pb-4 mb-4">
              <div>
                <h3 className="font-display text-base font-bold">Activity Log</h3>
                <p className="text-xs text-muted-foreground">Recent system activities and audit trail</p>
              </div>
              <Button asChild variant="ghost" size="sm" className="rounded-xl text-xs gap-1 cursor-pointer">
                <Link to="/notifications">
                  View Timeline <ArrowRight className="size-3" />
                </Link>
              </Button>
            </div>

            <div className="space-y-3">
              {filteredActivity.length === 0 ? (
                <p className="py-6 text-center text-xs text-muted-foreground">No recent activities.</p>
              ) : (
                filteredActivity.map((act) => (
                  <div key={act.id} className="flex items-center justify-between rounded-xl bg-muted/30 p-3 text-xs">
                    <div className="flex items-center gap-2.5 min-w-0">
                      <Avatar className="size-7 shrink-0">
                        <AvatarFallback className="bg-primary/10 text-[10px] font-bold text-primary">
                          {act.actor.split(" ").map((n) => n[0]).join("")}
                        </AvatarFallback>
                      </Avatar>
                      <div className="min-w-0 truncate">
                        <span className="font-bold text-foreground">{act.actor}</span>{" "}
                        <span className="text-muted-foreground">{act.action}</span>{" "}
                        <span className="font-semibold text-primary">{act.target}</span>
                      </div>
                    </div>
                    <span className="text-[11px] text-muted-foreground/70 shrink-0 ml-2">{act.time}</span>
                  </div>
                ))
              )}
            </div>
          </motion.div>
        </>
      )}

      {/* Decision Dialog */}
      <Dialog open={!!hodApprovalDialog} onOpenChange={(o) => !o && setHodApprovalDialog(null)}>
        <DialogContent className="rounded-2xl max-w-md">
          <DialogHeader>
            <DialogTitle>Decide Approval Request</DialogTitle>
            <DialogDescription>
              {hodApprovalDialog?.approval.requestNo} — {hodApprovalDialog?.approval.title}
            </DialogDescription>
          </DialogHeader>
          <div className="py-2.5 space-y-3">
            <div className="space-y-1.5">
              <Label>Remarks</Label>
              <Textarea
                placeholder="Decision comments..."
                value={remarks}
                onChange={(e) => setRemarks(e.target.value)}
                rows={3}
                className="rounded-xl"
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:gap-0">
            <Button variant="outline" onClick={() => setHodApprovalDialog(null)} className="rounded-xl">Cancel</Button>
            <Button onClick={() => handleHodDecide("Rejected")} variant="destructive" className="rounded-xl">Reject</Button>
            <Button onClick={() => handleHodDecide("Approved")} className="rounded-xl bg-emerald-600 text-white hover:bg-emerald-500">Approve</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
