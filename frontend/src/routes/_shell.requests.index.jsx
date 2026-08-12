import { useMemo, useState, useEffect } from "react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowUpDown, ChevronLeft, ChevronRight, Plus, Search } from "lucide-react";
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
import { requests, departments, syncLocalStorage } from "@/data/mock";
import { Can, useAuth, ROLE_PROFILES } from "@/lib/auth";

export const Route = createFileRoute("/_shell/requests/")({
  validateSearch: (search) => ({
    status: search.status || "all",
    priority: search.priority || "all",
    department: search.department || "all",
  }),
  head: () => ({
    meta: [
      { title: "Service Requests — ServiceDesk" },
      { name: "description", content: "Browse, search, filter and manage all service requests." },
    ],
  }),
  component: RequestsPage,
});

const PAGE_SIZE = 8;
const statuses = ["Pending", "Assigned", "In Progress", "Completed", "Closed", "Cancelled"];
const priorities = ["Critical", "High", "Medium", "Low"];

const priorityOrder = { Critical: 0, High: 1, Medium: 2, Low: 3 };

function RequestsPage() {
  const { role, user } = useAuth();

  const activeProfile = useMemo(() => {
    if (user) {
      return {
        name: user.name,
        email: user.email,
        department: user.department,
        role: user.role,
      };
    }
    return role
      ? { ...ROLE_PROFILES[role], role }
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

  // Sync localStorage data on mount
  useEffect(() => {
    syncLocalStorage();
  }, []);

  // Update local states from URL query search parameters
  useEffect(() => {
    setStatus(searchParams.status || "all");
    setPriority(searchParams.priority || "all");
    setDepartment(searchParams.department || "all");
  }, [searchParams.status, searchParams.priority, searchParams.department]);

  const filtered = useMemo(() => {
    let list = requests.filter((r) => {
      // Role-based filtering
      if (role === "Requestor" && r.requesterEmail !== activeProfile.email) {
        return false;
      }
      if (role === "Technician") {
        const isMine = r.assignee === activeProfile.name;
        const isUnassignedInDept = !r.assignee && r.department === activeProfile.department;
        if (techFilter === "mine" && !isMine) return false;
        if (techFilter === "unassigned" && !isUnassignedInDept) return false;
        if (techFilter === "all" && !isMine && !isUnassignedInDept) return false;
      }
      if (role === "HOD" && r.department !== activeProfile.department) {
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
      if (sortKey === "priority") cmp = priorityOrder[a.priority] - priorityOrder[b.priority];
      if (sortKey === "title") cmp = a.title.localeCompare(b.title);
      return sortAsc ? cmp : -cmp;
    });
    return list;
  }, [search, status, priority, department, sortKey, sortAsc, role, activeProfile, techFilter]);

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
              placeholder="Search by title, number or requester…"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              className="h-9 rounded-xl pl-9"
            />
          </div>
          <div className="grid grid-cols-3 gap-2 lg:flex">
            <Select
              value={status}
              onValueChange={(v) => {
                setStatus(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl lg:w-40">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                {statuses.map((s) => (
                  <SelectItem key={s} value={s}>
                    {s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={priority}
              onValueChange={(v) => {
                setPriority(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl lg:w-36">
                <SelectValue placeholder="Priority" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All priorities</SelectItem>
                {priorities.map((p) => (
                  <SelectItem key={p} value={p}>
                    {p}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={department}
              onValueChange={(v) => {
                setDepartment(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 rounded-xl lg:w-40">
                <SelectValue placeholder="Department" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All departments</SelectItem>
                {departments.map((d) => (
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
                <TableHead>
                  <button
                    className="inline-flex items-center gap-1 font-semibold hover:text-foreground"
                    onClick={() => toggleSort("title")}
                  >
                    Request <ArrowUpDown className="size-3" />
                  </button>
                </TableHead>
                <TableHead className="hidden md:table-cell">Requester</TableHead>
                <TableHead className="hidden lg:table-cell">Department</TableHead>
                <TableHead>
                  <button
                    className="inline-flex items-center gap-1 font-semibold hover:text-foreground"
                    onClick={() => toggleSort("priority")}
                  >
                    Priority <ArrowUpDown className="size-3" />
                  </button>
                </TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="hidden sm:table-cell">
                  <button
                    className="inline-flex items-center gap-1 font-semibold hover:text-foreground"
                    onClick={() => toggleSort("created")}
                  >
                    Created <ArrowUpDown className="size-3" />
                  </button>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {pageItems.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={6}
                    className="py-12 text-center text-sm text-muted-foreground"
                  >
                    No requests match your filters.
                  </TableCell>
                </TableRow>
              )}
              {pageItems.map((r) => (
                <TableRow key={r.id} className="group">
                  <TableCell>
                    <Link
                      to="/requests/$requestId"
                      params={{ requestId: r.id }}
                      className="block min-w-0"
                    >
                      <p className="max-w-52 truncate text-sm font-semibold group-hover:text-primary sm:max-w-80">
                        {r.title}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {r.no} · {r.requestType}
                      </p>
                    </Link>
                  </TableCell>
                  <TableCell className="hidden text-sm md:table-cell">{r.requester}</TableCell>
                  <TableCell className="hidden text-sm text-muted-foreground lg:table-cell">
                    {r.department}
                  </TableCell>
                  <TableCell>
                    <PriorityBadge priority={r.priority} />
                  </TableCell>
                  <TableCell>
                    <StatusBadge status={r.status} />
                  </TableCell>
                  <TableCell className="hidden text-sm text-muted-foreground sm:table-cell">
                    {new Date(r.created).toLocaleDateString("en-IN", {
                      day: "numeric",
                      month: "short",
                    })}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        <div className="flex items-center justify-between border-t p-4">
          <p className="text-xs text-muted-foreground">
            Page {current} of {totalPages} · {filtered.length} results
          </p>
          <div className="flex items-center gap-1">
            <Button
              variant="outline"
              size="icon"
              className="size-8 rounded-lg"
              disabled={current === 1}
              onClick={() => setPage(current - 1)}
            >
              <ChevronLeft className="size-4" />
            </Button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
              <Button
                key={p}
                variant={p === current ? "default" : "outline"}
                size="icon"
                className="size-8 rounded-lg text-xs"
                onClick={() => setPage(p)}
              >
                {p}
              </Button>
            ))}
            <Button
              variant="outline"
              size="icon"
              className="size-8 rounded-lg"
              disabled={current === totalPages}
              onClick={() => setPage(current + 1)}
            >
              <ChevronRight className="size-4" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
