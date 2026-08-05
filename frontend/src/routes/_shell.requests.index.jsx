import { useMemo, useState, useEffect } from "react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowUpDown, ChevronLeft, ChevronRight, Plus, Search, Loader2 } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatusBadge, PriorityBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getServiceRequests, getDepartments, getServiceRequestStatuses } from "@/services/api";
import { Can, useAuth } from "@/lib/auth";

export const Route = createFileRoute("/_shell/requests/")({
  validateSearch: (search) => ({
    status: search.status || "all",
    priority: search.priority || "all",
    department: search.department || "all",
  }),
  component: RequestsPage,
});


const PAGE_SIZE = 8;
const priorityOrder = { Critical: 0, High: 1, Medium: 2, Low: 3 };

function RequestsPage() {
  const { role, user } = useAuth();
  const [requestsList, setRequestsList] = useState([]);
  const [deptList, setDeptList] = useState([]);
  const [statusList, setStatusList] = useState([]);
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
    return role
      ? { name: user?.fullName || user?.name || "", email: user?.email || "", department: user?.department || "", role }
      : { name: "", email: "", department: "", role: "Requestor" };
  }, [user, role]);

  const searchParams = Route.useSearch();

  const [search, setSearch] = useState("");
  const [status, setStatus] = useState(searchParams.status || "all");
  const [priority, setPriority] = useState(searchParams.priority || "all");
  const [department, setDepartment] = useState(searchParams.department || "all");
  const [sortKey, setSortKey] = useState("created");
  const [sortAsc, setSortAsc] = useState(false);
  const [page, setPage] = useState(1);
  const [techFilter, setTechFilter] = useState("all");

  const loadData = async () => {
    setLoading(true);
    try {
      const [apiRequests, apiDepts, apiStatuses] = await Promise.all([
        getServiceRequests().catch(() => []),
        getDepartments().catch(() => []),
        getServiceRequestStatuses().catch(() => []),
      ]);

      setDeptList((apiDepts || []).map((d) => d.departmentName));
      setStatusList((apiStatuses || []).map((s) => s.statusName));

      const mapped = (apiRequests || []).map((sr) => ({
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
      }));

      setRequestsList(mapped);
    } catch (err) {
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    setStatus(searchParams.status || "all");
    setPriority(searchParams.priority || "all");
    setDepartment(searchParams.department || "all");
  }, [searchParams.status, searchParams.priority, searchParams.department]);

  const filtered = useMemo(() => {
    let list = requestsList.filter((r) => {
      // Role-based filtering
      if (role === "Requestor" && activeProfile.email && r.requesterEmail.toLowerCase() !== activeProfile.email.toLowerCase()) {
        return false;
      }
      if (role === "Technician") {
        const isMine = r.assignee && r.assignee.toLowerCase() === activeProfile.name.toLowerCase();
        const isUnassignedInDept = !r.assignee && r.department === activeProfile.department;
        if (techFilter === "mine" && !isMine) return false;
        if (techFilter === "unassigned" && !isUnassignedInDept) return false;
        if (techFilter === "all" && !isMine && !isUnassignedInDept) return false;
      }
      if (role === "HOD" && activeProfile.department && r.department !== activeProfile.department) {
        return false;
      }

      const q = search.toLowerCase();
      const matches =
        !q ||
        r.title.toLowerCase().includes(q) ||
        r.no.toLowerCase().includes(q) ||
        r.requester.toLowerCase().includes(q);
      return (
        matches &&
        (status === "all" || r.status === status) &&
        (priority === "all" || r.priority === priority) &&
        (department === "all" || r.department === department)
      );
    });
    list = [...list].sort((a, b) => {
      let cmp = 0;
      if (sortKey === "created") cmp = a.created.localeCompare(b.created);
      if (sortKey === "priority") cmp = (priorityOrder[a.priority] ?? 2) - (priorityOrder[b.priority] ?? 2);
      if (sortKey === "title") cmp = a.title.localeCompare(b.title);
      return sortAsc ? cmp : -cmp;
    });
    return list;
  }, [requestsList, search, status, priority, department, sortKey, sortAsc, role, activeProfile, techFilter]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const current = Math.min(page, totalPages);
  const pageItems = filtered.slice((current - 1) * PAGE_SIZE, current * PAGE_SIZE);

  const toggleSort = (key) => {
    if (sortKey === key) setSortAsc(!sortAsc);
    else {
      setSortKey(key);
      setSortAsc(key === "title");
    }
  };

  return (
    <div>
      <PageHeader
        title="Service Requests"
        description={`${filtered.length} requests found`}
        crumbs={[{ label: "Service Requests" }]}
        actions={
          <Can perm="requests.create">
            <Button asChild className="rounded-xl">
              <Link to="/requests/new">
                <Plus className="mr-1.5 size-4" /> New Request
              </Link>
            </Button>
          </Can>
        }
      />

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card">
        <div className="flex flex-col gap-3 border-b p-4 lg:flex-row lg:items-center">
          {role === "Technician" && (
            <div className="flex gap-1 p-1 bg-muted/60 rounded-xl border max-w-fit self-start lg:self-auto shrink-0">
              <Button
                type="button"
                variant={techFilter === "all" ? "default" : "ghost"}
                size="sm"
                className="h-7 rounded-lg text-xs cursor-pointer shadow-none"
                onClick={() => {
                  setTechFilter("all");
                  setPage(1);
                }}
              >
                All {activeProfile.department || "Dept"} Tasks
              </Button>
              <Button
                type="button"
                variant={techFilter === "mine" ? "default" : "ghost"}
                size="sm"
                className="h-7 rounded-lg text-xs cursor-pointer shadow-none"
                onClick={() => {
                  setTechFilter("mine");
                  setPage(1);
                }}
              >
                Assigned to Me
              </Button>
              <Button
                type="button"
                variant={techFilter === "unassigned" ? "default" : "ghost"}
                size="sm"
                className="h-7 rounded-lg text-xs cursor-pointer shadow-none"
                onClick={() => {
                  setTechFilter("unassigned");
                  setPage(1);
                }}
              >
                Unassigned
              </Button>
            </div>
          )}
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by title, request number or requester…"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              className="h-9 rounded-xl pl-9"
            />
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Select
              value={status}
              onValueChange={(val) => {
                setStatus(val);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl sm:w-36">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                {(statusList.length > 0 ? statusList : ["Pending", "Assigned", "In Progress", "Completed", "Closed", "Cancelled"]).map((s) => (
                  <SelectItem key={s} value={s}>
                    {s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={priority}
              onValueChange={(val) => {
                setPriority(val);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl sm:w-36">
                <SelectValue placeholder="Priority" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All priorities</SelectItem>
                {["Critical", "High", "Medium", "Low"].map((p) => (
                  <SelectItem key={p} value={p}>
                    {p}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={department}
              onValueChange={(val) => {
                setDepartment(val);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl sm:w-40">
                <SelectValue placeholder="Department" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All departments</SelectItem>
                {(deptList.length > 0 ? deptList : ["IT", "Operations", "Sales", "Maintenance"]).map((d) => (
                  <SelectItem key={d} value={d}>
                    {d}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-28 cursor-pointer select-none" onClick={() => toggleSort("created")}>
                  <span className="inline-flex items-center gap-1">
                    Request ID <ArrowUpDown className="size-3" />
                  </span>
                </TableHead>
                <TableHead className="min-w-[200px] cursor-pointer select-none" onClick={() => toggleSort("title")}>
                  <span className="inline-flex items-center gap-1">
                    Title <ArrowUpDown className="size-3" />
                  </span>
                </TableHead>
                <TableHead className="hidden md:table-cell">Requester</TableHead>
                <TableHead className="hidden lg:table-cell">Department</TableHead>
                <TableHead className="hidden xl:table-cell">Assignee</TableHead>
                <TableHead className="cursor-pointer select-none" onClick={() => toggleSort("priority")}>
                  <span className="inline-flex items-center gap-1">
                    Priority <ArrowUpDown className="size-3" />
                  </span>
                </TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading && (
                <TableRow>
                  <TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">
                    <Loader2 className="size-6 animate-spin mx-auto mb-2 text-primary" />
                    Loading requests from server...
                  </TableCell>
                </TableRow>
              )}
              {!loading && pageItems.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">
                    No requests match your filters.
                  </TableCell>
                </TableRow>
              )}
              {!loading && pageItems.map((r) => (
                <TableRow key={r.id} className="group">
                  <TableCell className="font-mono text-xs font-semibold text-primary">
                    <Link to="/requests/$requestId" params={{ requestId: r.id }} className="block">
                      {r.no}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <Link to="/requests/$requestId" params={{ requestId: r.id }} className="block">
                      <p className="font-semibold text-sm group-hover:text-primary transition-colors truncate max-w-[260px] sm:max-w-xs">
                        {r.title}
                      </p>
                      <p className="text-xs text-muted-foreground md:hidden">{r.requester} · {r.department}</p>
                    </Link>
                  </TableCell>
                  <TableCell className="hidden md:table-cell text-sm">{r.requester}</TableCell>
                  <TableCell className="hidden lg:table-cell text-sm text-muted-foreground">{r.department}</TableCell>
                  <TableCell className="hidden xl:table-cell text-sm text-muted-foreground">{r.assignee ?? "—"}</TableCell>
                  <TableCell>
                    <PriorityBadge priority={r.priority} />
                  </TableCell>
                  <TableCell>
                    <StatusBadge status={r.status} />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {/* Pagination footer */}
        <div className="flex items-center justify-between border-t p-4">
          <p className="text-xs text-muted-foreground">
            Showing <span className="font-medium text-foreground">{filtered.length > 0 ? (current - 1) * PAGE_SIZE + 1 : 0}</span> to{" "}
            <span className="font-medium text-foreground">{Math.min(current * PAGE_SIZE, filtered.length)}</span> of{" "}
            <span className="font-medium text-foreground">{filtered.length}</span> requests
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={current <= 1}
              onClick={() => setPage(current - 1)}
              className="h-8 rounded-lg cursor-pointer"
            >
              <ChevronLeft className="size-4 mr-1" /> Prev
            </Button>
            <span className="text-xs text-muted-foreground px-2">
              Page {current} of {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={current >= totalPages}
              onClick={() => setPage(current + 1)}
              className="h-8 rounded-lg cursor-pointer"
            >
              Next <ChevronRight className="size-4 ml-1" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
