import { useMemo, useEffect, useState } from "react";
import { createFileRoute, Link, useRouter, useNavigate } from "@tanstack/react-router";
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
  currentUser,
  monthlyRequests,
  recentActivity,
  requests,
  syncLocalStorage,
  users,
  departmentReports,
  technicians,
  updateRequest,
  updateApproval,
  approvals,
} from "@/data/mock";
import { useAuth, ROLE_PROFILES } from "@/lib/auth";
import { toast } from "sonner";
import { LandingPage } from "@/components/shared/LandingPage";

export const Route = createFileRoute("/_shell/")({
  head: () => ({
    meta: [
      { title: "ServiceDesk — IT Service Request Management Platform" },
      {
        name: "description",
        content: "Overview of service request KPIs, trends, activity and quick actions.",
      },
    ],
  }),
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

  useEffect(() => {
    syncLocalStorage();
  }, []);

  const activeProfile = useMemo(() => {
    if (user) {
      return {
        name: user.name,
        email: user.email,
        department: user.department,
        role: user.role,
      };
    }
    return role ? { ...ROLE_PROFILES[role], role } : currentUser;
  }, [user, role]);

  // Filter requests based on user role
  const userRequests = useMemo(() => {
    if (role === "Requestor") {
      return requests.filter((r) => r.requesterEmail === activeProfile.email);
    }
    if (role === "Technician") {
      return requests.filter((r) => r.assignee === activeProfile.name);
    }
    if (role === "HOD") {
      return requests.filter((r) => r.department === activeProfile.department);
    }
    // Admin sees all requests
    return requests;
  }, [role, activeProfile]);

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
      .filter((item) => item.value > 0); // Only show non-zero statuses to keep chart clean
  }, [userRequests]);

  // Dynamic recent requests list
  const recent = useMemo(() => {
    return userRequests.slice(0, 5);
  }, [userRequests]);

  // Filter recent activities based on what the user is allowed to see
  const filteredActivity = useMemo(() => {
    if (role === "Requestor") {
      const myTicketNos = new Set(userRequests.map((r) => r.no));
      return recentActivity.filter(
        (act) => myTicketNos.has(act.target) || act.actor === activeProfile.name,
      );
    }
    if (role === "Technician") {
      const myTicketNos = new Set(userRequests.map((r) => r.no));
      return recentActivity.filter(
        (act) => myTicketNos.has(act.target) || act.actor === activeProfile.name,
      );
    }
    return recentActivity;
  }, [role, userRequests, activeProfile]);

  // Quick action items conditional on permissions
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
    const total = requests.length;
    const pending = requests.filter((r) => r.status === "Pending").length;
    const assigned = requests.filter((r) => r.status === "Assigned").length;
    const inProgress = requests.filter((r) => r.status === "In Progress").length;
    const completed = requests.filter((r) => r.status === "Completed").length;
    const closed = requests.filter((r) => r.status === "Closed").length;
    const deptsCount = 5;
    const techsCount = technicians.length;
    const activeUsersCount = users.filter((u) => u.status === "Active").length;
    const today =
      requests.filter((r) => r.created.startsWith(new Date().toISOString().split("T")[0])).length ||
      3;
    const monthly = requests.length;
    return {
      total,
      pending: pending + assigned,
      inProgress,
      completed,
      closed,
      deptsCount,
      techsCount,
      activeUsersCount,
      today,
      monthly,
    };
  }, []);

  const router = useRouter();
  const [remarks, setRemarks] = useState("");
  const [hodApprovalDialog, setHodApprovalDialog] = useState(null);
  const [hodAssignee, setHodAssignee] = useState("unassigned");

  const deptApprovals = useMemo(() => {
    return approvals.filter(
      (a) => a.status === "Pending" && a.department === activeProfile.department,
    );
  }, [activeProfile]);

  const completedDeptRequests = useMemo(() => {
    return requests
      .filter(
        (r) =>
          r.department === activeProfile.department &&
          (r.status === "Completed" || r.status === "Closed"),
      )
      .slice(0, 3);
  }, [activeProfile]);

  const userRoleCounts = useMemo(() => {
    const counts = { Admin: 0, HOD: 0, Technician: 0, Requestor: 0 };
    users.forEach((u) => {
      if (counts[u.role] !== undefined) {
        counts[u.role]++;
      }
    });
    return counts;
  }, []);

  const activeRequestList = useMemo(() => {
    return userRequests
      .filter((r) => r.status !== "Closed" && r.status !== "Cancelled")
      .slice(0, 3);
  }, [userRequests]);

  const recentRepliesFeed = useMemo(() => {
    const feed = [];
    userRequests.forEach((req) => {
      req.replies.forEach((rep) => {
        if (rep.author !== activeProfile.name) {
          feed.push({ reply: rep, request: req });
        }
      });
    });
    return feed.sort((a, b) => b.reply.date.localeCompare(a.reply.date)).slice(0, 5);
  }, [userRequests, activeProfile]);

  const techPendingWork = useMemo(() => {
    return userRequests.filter((r) => r.status === "Assigned" || r.status === "In Progress");
  }, [userRequests]);

  const deptUnassignedQueue = useMemo(() => {
    return requests.filter(
      (r) => !r.assignee && r.department === activeProfile.department && r.status === "Pending",
    );
  }, [activeProfile]);

  const handleClaim = (req) => {
    const updated = {
      ...req,
      assignee: activeProfile.name,
      status: "Assigned",
      replies: [
        ...req.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: "Technician",
          message: `Technician ${activeProfile.name} claimed this request.`,
          date: new Date().toISOString(),
        },
      ],
    };
    updateRequest(updated, activeProfile.name, "Claimed request");
    toast.success("Request claimed successfully!");
    router.invalidate();
  };

  const handleStartWork = (req) => {
    const updated = {
      ...req,
      status: "In Progress",
      replies: [
        ...req.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: "Technician",
          message: "Started working on this request.",
          date: new Date().toISOString(),
          status: "In Progress",
        },
      ],
    };
    updateRequest(updated, activeProfile.name, "Started work");
    toast.success("Work started!");
    router.invalidate();
  };

  const handleResolve = (req) => {
    const updated = {
      ...req,
      status: "Completed",
      replies: [
        ...req.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: "Technician",
          message: "Marked this request as Completed.",
          date: new Date().toISOString(),
          status: "Completed",
        },
      ],
    };
    updateRequest(updated, activeProfile.name, "Marked completed");
    toast.success("Request completed!");
    router.invalidate();
  };

  const handleHodDecide = () => {
    if (!hodApprovalDialog) return;
    const selectedTech = hodAssignee === "unassigned" ? null : hodAssignee;

    const appRecord = approvals.find((a) => a.id === hodApprovalDialog.approval.id);
    if (appRecord) {
      const updatedApproval = {
        ...appRecord,
        status: hodApprovalDialog.action,
        decidedBy: activeProfile.name,
        decidedOn: new Date().toISOString(),
        remarks: remarks || undefined,
      };

      updateApproval(updatedApproval, activeProfile.name, remarks);

      if (hodApprovalDialog.action === "Approved" && selectedTech) {
        const reqObj = requests.find((r) => r.id === appRecord.requestId);
        if (reqObj) {
          reqObj.assignee = selectedTech;
          reqObj.status = "Assigned";
          reqObj.timeline?.push({
            id: String(Date.now() + Math.random()),
            status: "Assigned",
            changedBy: activeProfile.name,
            changedAt: new Date().toISOString(),
            note: `Technician assigned: ${selectedTech}`,
          });
          reqObj.replies.push({
            id: Date.now(),
            author: activeProfile.name,
            role: "HOD",
            message: `Assigned ticket to technician: ${selectedTech}.`,
            date: new Date().toISOString(),
          });
          localStorage.setItem("servicedesk.requests", JSON.stringify(requests));
        }
      }

      toast.success(`${appRecord.requestNo} ${hodApprovalDialog.action.toLowerCase()}`);
      router.invalidate();
    }

    setHodApprovalDialog(null);
    setRemarks("");
    setHodAssignee("unassigned");
  };
  return (
    <div className="space-y-6">
      {/* Welcome Hero Section */}
      <motion.div
        initial={{ opacity: 0, y: -15 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}
        className="relative overflow-hidden rounded-3xl border border-primary/15 dark:border-primary/10 bg-gradient-to-br from-card via-card to-primary/10 dark:from-sidebar dark:via-sidebar dark:to-primary/20 p-6 md:p-8 shadow-lg shadow-primary/5 dark:shadow-2xl text-foreground dark:text-slate-100"
      >
        {/* Premium Ambient Light/Dark Glows */}
        <div className="absolute -right-16 -top-16 size-48 rounded-full bg-primary/10 dark:bg-primary/20 blur-3xl pointer-events-none" />
        <div className="absolute -left-16 -bottom-16 size-48 rounded-full bg-success/5 dark:bg-success/15 blur-3xl pointer-events-none" />

        <div className="relative z-10 flex flex-col lg:flex-row lg:items-center justify-between gap-6">
          <div className="flex items-center gap-4.5">
            <Avatar className="size-16 border-2 border-primary/20 ring-4 ring-primary/5 shadow-sm">
              <AvatarFallback className="bg-primary text-xl font-black text-primary-foreground">
                {activeProfile.name
                  .split(" ")
                  .map((w) => w[0])
                  .join("")}
              </AvatarFallback>
            </Avatar>
            <div>
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-primary/10 text-primary border border-primary/25 dark:bg-primary/25 dark:text-primary dark:border-primary/30 uppercase tracking-wide">
                {role || "Guest"}
              </span>
              <h1 className="text-2xl md:text-3xl font-extrabold tracking-tight mt-1.5 flex items-center gap-2 text-slate-900 dark:text-slate-100">
                {timeGreeting}, {activeProfile.name.split(" ")[0]} 👋
              </h1>
              <p className="text-xs text-slate-600 dark:text-slate-400 mt-1 flex flex-wrap items-center gap-x-2 gap-y-1 font-semibold">
                <span>{activeProfile.department || "General"} Department</span>
                <span className="text-muted-foreground/30 dark:text-slate-500/50">•</span>
                <span>{greetingDate}</span>
                <span className="text-muted-foreground/30 dark:text-slate-500/50">•</span>
                <span className="font-bold text-primary dark:text-sky-400 bg-primary/5 dark:bg-sky-400/10 px-2 py-0.5 rounded-md border border-primary/10 dark:border-sky-400/20">
                  {formattedTime}
                </span>
              </p>
            </div>
          </div>

          {/* Quick actions hero button row */}
          <div className="flex flex-wrap items-center gap-2.5">
            {(role === "Requestor" || role === "Admin") && (
              <Button
                asChild
                className="rounded-xl font-bold cursor-pointer h-10 shadow-md shadow-primary/15 hover:shadow-lg hover:shadow-primary/35 hover:-translate-y-0.5 transition-all text-xs"
              >
                <Link to="/requests/new">
                  <Plus className="mr-1.5 size-4" /> Raise New Request
                </Link>
              </Button>
            )}
            <Button
              type="button"
              variant="outline"
              className="rounded-xl font-bold cursor-pointer h-10 hover:-translate-y-0.5 transition-all text-xs bg-card/60 backdrop-blur-sm"
              onClick={() => {
                router.invalidate();
                toast.success("Dashboard Refreshed", {
                  description: "Dynamic support desk metrics loaded.",
                });
              }}
            >
              <RefreshCcw className="mr-1.5 size-3.5" /> Refresh
            </Button>
            {(role === "Admin" || role === "HOD") && (
              <>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl font-bold cursor-pointer h-10 hover:-translate-y-0.5 transition-all text-xs bg-card/60 backdrop-blur-sm"
                  onClick={() => {
                    toast.success("Export Initialized", {
                      description: "Your CSV service desk report will download shortly.",
                    });
                  }}
                >
                  <FileText className="mr-1.5 size-3.5" /> Export Report
                </Button>
                <Button
                  asChild
                  variant="outline"
                  className="rounded-xl font-bold cursor-pointer h-10 hover:-translate-y-0.5 transition-all text-xs bg-card/60 backdrop-blur-sm"
                >
                  <Link to="/reports">
                    <BarChart3 className="mr-1.5 size-3.5" /> View Reports
                  </Link>
                </Button>
              </>
            )}
          </div>
        </div>
      </motion.div>

      {/* Admin specific workload grid */}
      {role === "Admin" && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mt-6">
            <StatCard
              title="Total Requests"
              value={stats.total}
              icon={Ticket}
              progress={100}
              colorClass="bg-primary"
              description="System-wide raised requests"
              index={0}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "all", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Pending"
              value={stats.pending}
              icon={Loader}
              progress={40}
              colorClass="bg-warning"
              description="Awaiting assignment/work"
              index={1}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "Pending", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="In Progress"
              value={stats.inProgress}
              icon={Clock}
              progress={20}
              colorClass="bg-info"
              description="Under active resolution"
              index={2}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "In Progress", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Completed"
              value={stats.completed}
              icon={CheckCircle2}
              progress={80}
              colorClass="bg-success"
              description="Resolved by desk staff"
              index={3}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "Completed", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Closed"
              value={stats.closed}
              icon={CheckSquare}
              progress={90}
              colorClass="bg-slate-400"
              description="Archived and finalized"
              index={4}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "Closed", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Departments"
              value={stats.deptsCount}
              icon={Building}
              progress={100}
              colorClass="bg-indigo-500"
              description="Registered user groups"
              index={5}
              onClickDetails={() => navigate({ to: "/masters", search: { tab: "departments" } })}
            />
            <StatCard
              title="Technicians"
              value={stats.techsCount}
              icon={Wrench}
              progress={60}
              colorClass="bg-pink-500"
              description="Assigned desk experts"
              index={6}
              onClickDetails={() => navigate({ to: "/users/", search: { role: "Technician" } })}
            />
            <StatCard
              title="Active Users"
              value={stats.activeUsersCount}
              icon={Users}
              progress={90}
              colorClass="bg-teal-500"
              description="Logged in system accounts"
              index={7}
              onClickDetails={() => navigate({ to: "/users/" })}
            />
            <StatCard
              title="Today's Tasks"
              value={stats.today}
              icon={Play}
              progress={30}
              colorClass="bg-destructive"
              description="New requests raised today"
              index={8}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "all", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Monthly Load"
              value={stats.monthly}
              icon={FolderOpen}
              progress={50}
              colorClass="bg-amber-500"
              description="Total requests in current month"
              index={9}
              onClickDetails={() =>
                navigate({
                  to: "/requests/",
                  search: { status: "all", priority: "all", department: "all" },
                })
              }
            />
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-5">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.15 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card lg:col-span-3"
            >
              <div className="mb-4 flex items-center justify-between">
                <div>
                  <h3 className="font-display text-base font-bold">Monthly Requests</h3>
                  <p className="text-xs text-muted-foreground">
                    System-wide tickets raised vs resolved
                  </p>
                </div>
              </div>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={monthlyRequests} barGap={2}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                    <XAxis
                      dataKey="month"
                      tick={{ fontSize: 12, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                    />

                    <YAxis
                      tick={{ fontSize: 12, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                      width={32}
                    />

                    <Tooltip
                      cursor={{ fill: "var(--accent)" }}
                      contentStyle={{
                        background: "var(--popover)",
                        border: "1px solid var(--border)",
                        borderRadius: 12,
                        fontSize: 12,
                      }}
                      itemStyle={{
                        color: "var(--popover-foreground)",
                      }}
                      labelStyle={{
                        color: "var(--popover-foreground)",
                      }}
                    />

                    <Legend wrapperStyle={{ fontSize: 12 }} />
                    <Bar
                      dataKey="raised"
                      name="Raised"
                      fill="var(--chart-1)"
                      radius={[4, 4, 0, 0]}
                    />

                    <Bar
                      dataKey="resolved"
                      name="Resolved"
                      fill="var(--chart-2)"
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card lg:col-span-2"
            >
              <h3 className="font-display text-base font-bold">Request Status</h3>
              <p className="text-xs text-muted-foreground">Current distribution in your scope</p>
              <div className="h-64 flex items-center justify-center">
                {dynamicStatusDistribution.length === 0 ? (
                  <p className="text-sm text-muted-foreground">
                    No active requests to display distribution.
                  </p>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={dynamicStatusDistribution}
                        dataKey="value"
                        nameKey="name"
                        innerRadius={55}
                        outerRadius={85}
                        paddingAngle={3}
                        strokeWidth={0}
                      >
                        {dynamicStatusDistribution.map((_, i) => (
                          <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          background: "var(--popover)",
                          border: "1px solid var(--border)",
                          borderRadius: 12,
                          fontSize: 12,
                        }}
                        itemStyle={{
                          color: "var(--popover-foreground)",
                        }}
                        labelStyle={{
                          color: "var(--popover-foreground)",
                        }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                )}
              </div>
              <div className="flex flex-wrap justify-center gap-x-4 gap-y-1.5">
                {dynamicStatusDistribution.map((s, i) => (
                  <span
                    key={s.name}
                    className="flex items-center gap-1.5 text-xs text-muted-foreground"
                  >
                    <span
                      className="size-2 rounded-full"
                      style={{ background: PIE_COLORS[i % PIE_COLORS.length] }}
                    />
                    {s.name} ({s.value})
                  </span>
                ))}
              </div>
            </motion.div>
          </div>

          {/* Department & User summary grid */}
          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <Building className="size-4.5 text-primary" /> Department Workload Summary
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5">
                Active vs resolved requests count by department
              </p>
              <div className="space-y-4 mt-4">
                {departmentReports.map((dept) => {
                  const completionRate = Math.round((dept.resolved / dept.total) * 100);
                  return (
                    <div key={dept.department} className="space-y-1.5">
                      <div className="flex justify-between text-xs font-semibold">
                        <span>{dept.department} Support</span>
                        <span className="text-muted-foreground">
                          {dept.resolved} / {dept.total} resolved ({completionRate}%)
                        </span>
                      </div>
                      <div className="h-2 w-full bg-muted rounded-full overflow-hidden">
                        <div
                          className="h-full bg-primary rounded-full"
                          style={{ width: `${completionRate}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <Users className="size-4.5 text-primary" /> User Directory Overview
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5">
                User accounts distributed by access role
              </p>
              <div className="grid grid-cols-2 gap-4 mt-5">
                {[
                  { label: "Administrators", count: userRoleCounts.Admin, color: "text-primary" },
                  { label: "Department Heads", count: userRoleCounts.HOD, color: "text-info" },
                  {
                    label: "Service Technicians",
                    count: userRoleCounts.Technician,
                    color: "text-warning",
                  },
                  {
                    label: "End Requestors",
                    count: userRoleCounts.Requestor,
                    color: "text-success",
                  },
                ].map((item) => (
                  <div
                    key={item.label}
                    className="rounded-xl border bg-accent/20 p-3.5 text-center"
                  >
                    <p className="text-2xl font-bold leading-none">{item.count}</p>
                    <p className="text-xs text-muted-foreground mt-1 font-semibold">{item.label}</p>
                  </div>
                ))}
              </div>
              <Button asChild variant="outline" className="w-full mt-6 rounded-xl text-xs h-9">
                <Link to="/users">
                  Manage User Accounts <ArrowRight className="ml-1 size-3.5" />
                </Link>
              </Button>
            </motion.div>
          </div>

          {/* Recent list & activities */}
          <div className="mt-6 grid gap-4 xl:grid-cols-3">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.25 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card xl:col-span-2"
            >
              <div className="flex items-center justify-between border-b p-5 pb-4">
                <div>
                  <h3 className="font-display text-base font-bold">Recent Requests</h3>
                  <p className="text-xs text-muted-foreground">Latest tickets in your view</p>
                </div>
                <Button asChild variant="outline" size="sm" className="rounded-xl">
                  <Link to="/requests">
                    View all <ArrowRight className="ml-1 size-3.5" />
                  </Link>
                </Button>
              </div>
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Request</TableHead>
                      <TableHead className="hidden md:table-cell">Department</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {recent.length === 0 ? (
                      <TableRow>
                        <TableCell
                          colSpan={4}
                          className="py-12 text-center text-sm text-muted-foreground"
                        >
                          No recent requests found.
                        </TableCell>
                      </TableRow>
                    ) : (
                      recent.map((r) => (
                        <TableRow key={r.id} className="group">
                          <TableCell>
                            <Link
                              to="/requests/$requestId"
                              params={{ requestId: r.id }}
                              className="block min-w-0"
                            >
                              <p className="max-w-56 truncate text-sm font-semibold group-hover:text-primary sm:max-w-72">
                                {r.title}
                              </p>
                              <p className="text-xs text-muted-foreground">{r.no}</p>
                            </Link>
                          </TableCell>
                          <TableCell className="hidden text-sm text-muted-foreground md:table-cell">
                            {r.department}
                          </TableCell>
                          <TableCell>
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

            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <BarChart3 className="size-4.5 text-primary" /> Recent Activity
              </h3>
              <div className="mt-4 space-y-4">
                {filteredActivity.length === 0 ? (
                  <p className="py-8 text-center text-xs text-muted-foreground">
                    No recent activity.
                  </p>
                ) : (
                  filteredActivity.map((a) => (
                    <div key={a.id} className="flex gap-3">
                      <Avatar className="size-8 shrink-0">
                        <AvatarFallback className="bg-accent text-[10px] font-bold text-accent-foreground">
                          {a.actor
                            .split(" ")
                            .map((w) => w[0])
                            .join("")}
                        </AvatarFallback>
                      </Avatar>
                      <div className="min-w-0">
                        <p className="text-sm leading-snug">
                          <span className="font-semibold">{a.actor}</span>{" "}
                          <span className="text-muted-foreground">{a.action}</span>{" "}
                          <span className="font-medium text-primary">{a.target}</span>
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {a.detail} · {a.time}
                        </p>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </motion.div>
          </div>
        </>
      )}

      {/* Requestor Dashboard layout */}
      {role === "Requestor" && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
            <StatCard
              title="My Requests"
              value={stats.total}
              icon={Ticket}
              progress={100}
              colorClass="bg-primary"
              description="Total raised by you"
              index={0}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "all", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Pending Desk"
              value={stats.pending}
              icon={Loader}
              progress={40}
              colorClass="bg-warning"
              description="Awaiting staff pickup"
              index={1}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "Pending", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="In Resolution"
              value={stats.inProgress}
              icon={Clock}
              progress={20}
              colorClass="bg-info"
              description="Technician working"
              index={2}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "In Progress", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Resolved"
              value={stats.completed}
              icon={CheckCircle2}
              progress={80}
              colorClass="bg-success"
              description="Completed tickets"
              index={3}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "Completed", priority: "all", department: "all" },
                })
              }
            />
          </div>

          <div className="mt-6 space-y-4">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold">Active Ticket Status Trackers</h3>
              <p className="text-xs text-muted-foreground mt-0.5">
                Real-time status steps of your pending requests
              </p>

              <div className="space-y-6 mt-5">
                {activeRequestList.length === 0 ? (
                  <p className="text-sm text-muted-foreground py-8 text-center">
                    No active requests currently tracking.
                  </p>
                ) : (
                  activeRequestList.map((req) => {
                    const steps = ["Pending", "Assigned", "In Progress", "Completed"];
                    const currentIdx = steps.indexOf(req.status);
                    return (
                      <div key={req.id} className="border bg-accent/5 rounded-xl p-4 space-y-4">
                        <div className="flex justify-between items-start">
                          <div>
                            <Link
                              to="/requests/$requestId"
                              params={{ requestId: req.id }}
                              className="text-sm font-bold hover:text-primary leading-tight block"
                            >
                              {req.title}
                            </Link>
                            <p className="text-xs text-muted-foreground mt-0.5">
                              {req.no} · {req.requestType}
                            </p>
                          </div>
                          <PriorityBadge priority={req.priority} />
                        </div>

                        <div className="relative flex justify-between items-center w-full mt-2 px-2">
                          <div className="absolute top-[14px] left-4 right-4 h-0.5 bg-muted z-0" />
                          <div
                            className="absolute top-[14px] left-4 h-0.5 bg-primary z-0 transition-all duration-500"
                            style={{
                              width: `${Math.max(0, Math.min(100, (currentIdx / (steps.length - 1)) * 90))}%`,
                            }}
                          />

                          {steps.map((st, idx) => {
                            const active = idx <= currentIdx;
                            const current = idx === currentIdx;
                            return (
                              <div key={st} className="flex flex-col items-center z-10">
                                <div
                                  className={`size-7 rounded-full border grid place-items-center text-xs font-bold transition-colors ${
                                    current
                                      ? "bg-primary border-primary text-primary-foreground animate-pulse"
                                      : active
                                        ? "bg-primary border-primary text-primary-foreground"
                                        : "bg-background border-border text-muted-foreground"
                                  }`}
                                >
                                  {idx + 1}
                                </div>
                                <span
                                  className={`text-[10px] font-semibold mt-1.5 ${active ? "text-foreground" : "text-muted-foreground"}`}
                                >
                                  {st}
                                </span>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
            </motion.div>
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-2">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <MessageSquare className="size-4 text-primary" /> Recent Technician Replies
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5">
                Updates on your tickets from support desk staff
              </p>

              <div className="space-y-4 mt-4">
                {recentRepliesFeed.length === 0 ? (
                  <p className="text-xs text-muted-foreground py-8 text-center bg-muted/20 rounded-xl">
                    No replies on your tickets yet.
                  </p>
                ) : (
                  recentRepliesFeed.map(({ reply, request }) => (
                    <Link
                      key={reply.id}
                      to="/requests/$requestId"
                      params={{ requestId: request.id }}
                      className="block rounded-xl border bg-accent/10 p-3 hover:border-primary/30 transition-colors"
                    >
                      <div className="flex justify-between items-center">
                        <span className="text-xs font-bold">
                          {reply.author}{" "}
                          <span className="text-[10px] text-muted-foreground">({reply.role})</span>
                        </span>
                        <span className="text-[10px] text-muted-foreground">
                          {new Date(reply.date).toLocaleDateString("en-IN", {
                            day: "numeric",
                            month: "short",
                          })}
                        </span>
                      </div>
                      <p className="text-[11px] text-muted-foreground mt-0.5 truncate font-semibold">
                        Re: {request.title}
                      </p>
                      <p className="text-xs leading-relaxed mt-1 text-foreground font-normal italic">
                        "
                        {reply.message.length > 80
                          ? reply.message.slice(0, 80) + "..."
                          : reply.message}
                        "
                      </p>
                    </Link>
                  ))
                )}
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
            >
              <div className="flex items-center justify-between border-b p-5 pb-4">
                <div>
                  <h3 className="font-display text-base font-bold">My Recent Requests</h3>
                  <p className="text-xs text-muted-foreground">
                    List of your latest raised tickets
                  </p>
                </div>
                <Button asChild variant="outline" size="sm" className="rounded-xl">
                  <Link to="/requests">
                    View all <ArrowRight className="ml-1 size-3.5" />
                  </Link>
                </Button>
              </div>
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Request</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {recent.length === 0 ? (
                      <TableRow>
                        <TableCell
                          colSpan={3}
                          className="py-12 text-center text-sm text-muted-foreground"
                        >
                          No requests found. Click "New Request" to create one.
                        </TableCell>
                      </TableRow>
                    ) : (
                      recent.map((r) => (
                        <TableRow key={r.id} className="group">
                          <TableCell>
                            <Link
                              to="/requests/$requestId"
                              params={{ requestId: r.id }}
                              className="block"
                            >
                              <p className="max-w-40 truncate text-sm font-semibold group-hover:text-primary sm:max-w-64">
                                {r.title}
                              </p>
                              <p className="text-xs text-muted-foreground">{r.no}</p>
                            </Link>
                          </TableCell>
                          <TableCell>
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
        </>
      )}

      {/* Technician Dashboard layout */}
      {role === "Technician" && (
        <>
          <div className="mt-6 grid gap-4 md:grid-cols-3">
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/45 backdrop-blur-md p-5 shadow-card flex flex-col md:col-span-1 border-b-4 border-b-primary"
            >
              <h3 className="font-display text-sm font-bold flex items-center gap-2">
                <Award className="size-4 text-primary" /> Performance Summary
              </h3>
              <p className="text-[11px] text-muted-foreground">
                SLA Compliance & Resolution Rating
              </p>

              <div className="flex-1 flex flex-col items-center justify-center py-4">
                <div className="relative size-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90">
                    <circle
                      cx="64"
                      cy="64"
                      r="54"
                      className="stroke-muted"
                      strokeWidth="8"
                      fill="transparent"
                    />

                    <circle
                      cx="64"
                      cy="64"
                      r="54"
                      className="stroke-primary"
                      strokeWidth="8"
                      fill="transparent"
                      strokeDasharray={2 * Math.PI * 54}
                      strokeDashoffset={2 * Math.PI * 54 * (1 - 0.965)}
                      strokeLinecap="round"
                    />
                  </svg>
                  <div className="absolute text-center">
                    <p className="text-2xl font-bold leading-none">96.5%</p>
                    <p className="text-[9px] text-muted-foreground font-semibold mt-0.5">SLA MET</p>
                  </div>
                </div>
                <p className="text-xs font-semibold mt-2 text-center text-muted-foreground">
                  Avg. Resolution: <span className="text-foreground">8.8 hours</span>
                </p>
              </div>
            </motion.div>

            <div className="md:col-span-2 grid grid-cols-2 gap-4">
              <StatCard
                title="Assigned to Me"
                value={stats.total}
                icon={Ticket}
                progress={100}
                colorClass="bg-primary"
                description="Assigned tasks count"
                index={0}
                onClickDetails={() =>
                  navigate({
                    to: "/requests",
                    search: { status: "all", priority: "all", department: "all" },
                  })
                }
              />
              <StatCard
                title="Active Work"
                value={stats.inProgress}
                icon={Clock}
                progress={40}
                colorClass="bg-info"
                description="Work in progress"
                index={1}
                onClickDetails={() =>
                  navigate({
                    to: "/requests",
                    search: { status: "In Progress", priority: "all", department: "all" },
                  })
                }
              />
              <StatCard
                title="Completed tasks"
                value={stats.completed}
                icon={CheckCircle2}
                progress={80}
                colorClass="bg-success"
                description="Resolved by you"
                index={2}
                onClickDetails={() =>
                  navigate({
                    to: "/requests",
                    search: { status: "Completed", priority: "all", department: "all" },
                  })
                }
              />
              <StatCard
                title="Unassigned Pool"
                value={deptUnassignedQueue.length}
                icon={Loader}
                progress={20}
                colorClass="bg-warning"
                description="Unclaimed department tickets"
                index={3}
                onClickDetails={() =>
                  navigate({
                    to: "/requests",
                    search: { status: "Pending", priority: "all", department: "all" },
                  })
                }
              />
            </div>
          </div>

          {/* Technician active tasks queue */}
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            className="mt-6 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <Clock className="size-4.5 text-primary" /> My Active Work Queue
              </h3>
              <p className="text-xs text-muted-foreground">
                Tasks assigned to you awaiting actions
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Request</TableHead>
                    <TableHead>Department</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Action</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {techPendingWork.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={5}
                        className="py-12 text-center text-sm text-muted-foreground"
                      >
                        No active assigned tasks! Choose a ticket from the unassigned pool below.
                      </TableCell>
                    </TableRow>
                  ) : (
                    techPendingWork.map((r) => (
                      <TableRow key={r.id}>
                        <TableCell>
                          <Link
                            to="/requests/$requestId"
                            params={{ requestId: r.id }}
                            className="block hover:underline"
                          >
                            <p className="max-w-48 truncate text-sm font-semibold">{r.title}</p>
                            <p className="text-xs text-muted-foreground">{r.no}</p>
                          </Link>
                        </TableCell>
                        <TableCell className="text-sm">{r.department}</TableCell>
                        <TableCell>
                          <PriorityBadge priority={r.priority} />
                        </TableCell>
                        <TableCell>
                          <StatusBadge status={r.status} />
                        </TableCell>
                        <TableCell className="text-right">
                          {r.status === "Assigned" && (
                            <Button
                              type="button"
                              size="sm"
                              className="rounded-lg h-7 text-[10px] cursor-pointer"
                              onClick={() => handleStartWork(r)}
                            >
                              <Play className="mr-1 size-3" /> Start Work
                            </Button>
                          )}
                          {r.status === "In Progress" && (
                            <Button
                              type="button"
                              size="sm"
                              className="rounded-lg h-7 text-[10px] bg-success text-success-foreground hover:bg-success/90 cursor-pointer"
                              onClick={() => handleResolve(r)}
                            >
                              <Check className="mr-1 size-3" /> Complete
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </motion.div>

          {/* Department unassigned tasks */}
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            className="mt-6 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <Loader className="size-4.5 text-primary animate-spin" /> Unassigned{" "}
                {activeProfile.department} Queue
              </h3>
              <p className="text-xs text-muted-foreground">
                Unassigned department tickets awaiting technician pickup
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Request</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="text-right">Action</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {deptUnassignedQueue.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={4}
                        className="py-12 text-center text-sm text-muted-foreground"
                      >
                        No unassigned tickets in the department queue.
                      </TableCell>
                    </TableRow>
                  ) : (
                    deptUnassignedQueue.map((r) => (
                      <TableRow key={r.id}>
                        <TableCell>
                          <Link
                            to="/requests/$requestId"
                            params={{ requestId: r.id }}
                            className="block hover:underline"
                          >
                            <p className="max-w-64 truncate text-sm font-semibold">{r.title}</p>
                            <p className="text-xs text-muted-foreground">{r.no}</p>
                          </Link>
                        </TableCell>
                        <TableCell>
                          <PriorityBadge priority={r.priority} />
                        </TableCell>
                        <TableCell className="text-xs text-muted-foreground">
                          {new Date(r.created).toLocaleDateString("en-IN")}
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="rounded-lg h-7 text-[10px] cursor-pointer"
                            onClick={() => handleClaim(r)}
                          >
                            Claim Request
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </motion.div>
        </>
      )}

      {/* HOD Dashboard layout */}
      {role === "HOD" && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
            <StatCard
              title="Dept. Tickets"
              value={stats.total}
              icon={Ticket}
              progress={100}
              colorClass="bg-primary"
              description="Total raised in your department"
              index={0}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "all", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Awaiting Approval"
              value={deptApprovals.length}
              icon={CheckSquare}
              progress={20}
              colorClass="bg-warning"
              description="Pending your decision"
              index={1}
              onClickDetails={() => navigate({ to: "/approvals" })}
            />
            <StatCard
              title="Active Work"
              value={stats.inProgress}
              icon={Clock}
              progress={40}
              colorClass="bg-info"
              description="Currently in progress"
              index={2}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "In Progress", priority: "all", department: "all" },
                })
              }
            />
            <StatCard
              title="Resolved"
              value={stats.completed}
              icon={CheckCircle2}
              progress={80}
              colorClass="bg-success"
              description="Completed department tasks"
              index={3}
              onClickDetails={() =>
                navigate({
                  to: "/requests",
                  search: { status: "Completed", priority: "all", department: "all" },
                })
              }
            />
          </div>

          {/* Department Pending approvals */}
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            className="mt-6 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <CheckSquare className="size-4.5 text-primary" /> Awaiting HOD Authorization
              </h3>
              <p className="text-xs text-muted-foreground">
                Approve or reject requests in your department
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Request</TableHead>
                    <TableHead>Requester</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {deptApprovals.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={4}
                        className="py-12 text-center text-sm text-muted-foreground"
                      >
                        <CheckCircle2 className="mx-auto size-8 text-success stroke-[1.5]" />
                        <p className="mt-2 font-bold text-sm">All caught up!</p>
                        <p className="text-xs text-muted-foreground mt-0.5">
                          No pending approvals for your department.
                        </p>
                      </TableCell>
                    </TableRow>
                  ) : (
                    deptApprovals.map((a) => (
                      <TableRow key={a.id}>
                        <TableCell>
                          <Link
                            to="/requests/$requestId"
                            params={{ requestId: a.requestId }}
                            className="block hover:underline"
                          >
                            <p className="max-w-64 truncate text-sm font-semibold">{a.title}</p>
                            <p className="text-xs text-muted-foreground">{a.requestNo}</p>
                          </Link>
                        </TableCell>
                        <TableCell className="text-sm">{a.requester}</TableCell>
                        <TableCell>
                          <PriorityBadge priority={a.priority} />
                        </TableCell>
                        <TableCell className="text-right flex items-center justify-end gap-1.5 h-[53px]">
                          <Button
                            type="button"
                            size="sm"
                            className="rounded-lg h-7 text-[10px] bg-success text-success-foreground hover:bg-success/90 cursor-pointer"
                            onClick={() => {
                              setHodApprovalDialog({ approval: a, action: "Approved" });
                              setHodAssignee("unassigned");
                            }}
                          >
                            <Check className="mr-0.5 size-3" /> Approve
                          </Button>
                          <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            className="rounded-lg h-7 text-[10px] cursor-pointer"
                            onClick={() =>
                              setHodApprovalDialog({ approval: a, action: "Rejected" })
                            }
                          >
                            <X className="mr-0.5 size-3" /> Reject
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </motion.div>

          {/* Department Completed requests */}
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            className="mt-6 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold flex items-center gap-2">
                <CheckCircle2 className="size-4.5 text-primary" /> Completed Department Requests
              </h3>
              <p className="text-xs text-muted-foreground">
                List of resolved or closed tickets in your department
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Request</TableHead>
                    <TableHead>Assigned Technician</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {completedDeptRequests.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={4}
                        className="py-12 text-center text-sm text-muted-foreground"
                      >
                        No completed department requests found.
                      </TableCell>
                    </TableRow>
                  ) : (
                    completedDeptRequests.map((r) => (
                      <TableRow key={r.id}>
                        <TableCell>
                          <Link
                            to="/requests/$requestId"
                            params={{ requestId: r.id }}
                            className="block hover:underline"
                          >
                            <p className="max-w-64 truncate text-sm font-semibold">{r.title}</p>
                            <p className="text-xs text-muted-foreground">{r.no}</p>
                          </Link>
                        </TableCell>
                        <TableCell className="text-sm font-semibold">
                          {r.assignee ?? "Unassigned"}
                        </TableCell>
                        <TableCell>
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
        </>
      )}

      {/* Quick Action links shortcuts */}
      <div className="mt-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        {quickActions.map((qa, i) => (
          <motion.div
            key={qa.title}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.35 + i * 0.05 }}
          >
            <Link
              to={qa.to}
              className="group flex h-full flex-col rounded-2xl border bg-card/45 backdrop-blur-md p-5 shadow-card transition-all hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-lg hover:bg-card/75 border-b-4 border-b-primary/60"
            >
              <div className="grid size-10 place-items-center rounded-xl bg-primary/10 text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground animate-none">
                <qa.icon className="size-5" />
              </div>
              <p className="mt-3 text-sm font-bold">{qa.title}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">{qa.desc}</p>
            </Link>
          </motion.div>
        ))}
      </div>

      {/* HOD Approvals dialog modal */}
      <Dialog
        open={!!hodApprovalDialog}
        onOpenChange={(open) => !open && setHodApprovalDialog(null)}
      >
        <DialogContent className="rounded-2xl max-w-md bg-card/90 backdrop-blur-xl">
          <DialogHeader>
            <DialogTitle className="font-display">
              {hodApprovalDialog?.action === "Approved" ? "Approve Request" : "Reject Request"}
            </DialogTitle>
            <DialogDescription>
              {hodApprovalDialog?.approval.requestNo} — {hodApprovalDialog?.approval.title}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-2 font-medium">
            {hodApprovalDialog?.action === "Approved" && (
              <div className="space-y-1.5">
                <Label>Assign Technician (Optional)</Label>
                <Select value={hodAssignee} onValueChange={(val) => setHodAssignee(val)}>
                  <SelectTrigger className="w-full h-10 rounded-xl bg-background/50">
                    <SelectValue placeholder="Select Technician" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="unassigned">Unassigned (Awaiting Assign)</SelectItem>
                    {technicians.map((tech) => (
                      <SelectItem key={tech} value={tech}>
                        {tech}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}
            <div className="space-y-1.5">
              <Label>Decision Remarks</Label>
              <Textarea
                placeholder="Type remarks here..."
                value={remarks}
                onChange={(e) => setRemarks(e.target.value)}
                rows={3}
                className="rounded-xl font-normal bg-background/50"
              />
            </div>
          </div>

          <DialogFooter className="gap-2 sm:gap-0 mt-3">
            <Button
              type="button"
              variant="outline"
              className="rounded-xl"
              onClick={() => {
                setHodApprovalDialog(null);
                setRemarks("");
                setHodAssignee("unassigned");
              }}
            >
              Cancel
            </Button>
            <Button
              type="button"
              variant={hodApprovalDialog?.action === "Rejected" ? "destructive" : "default"}
              className="rounded-xl"
              onClick={handleHodDecide}
            >
              Confirm {hodApprovalDialog?.action === "Approved" ? "Approval" : "Rejection"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
