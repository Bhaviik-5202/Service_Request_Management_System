import { useMemo, useState, useEffect } from "react";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { Plus, Edit2, Trash2, Search } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

// Data and CRUD imports
import {
  users,
  syncLocalStorage,
  addUser,
  updateUser,
  deleteUser,
  departments, // Dyn list loaded from departments master
} from "@/data/mock";

// Reusable components
import { ReusableTable } from "@/components/shared/ReusableTable";
import { ReusableModal } from "@/components/shared/ReusableModal";
import { ReusablePagination } from "@/components/shared/ReusablePagination";
import { DeleteConfirmation } from "@/components/shared/DeleteConfirmation";

export const Route = createFileRoute("/_shell/users/")({
  validateSearch: (search) => ({
    role: search.role || "all",
  }),
  head: () => ({
    meta: [
      { title: "Users — ServiceDesk" },
      { name: "description", content: "Manage users, roles and department assignments." },
    ],
  }),
  component: UsersPage,
});

const roleStyles = {
  Admin: "bg-primary/10 text-primary ring-primary/20",
  HOD: "bg-chart-2/10 text-chart-2 ring-chart-2/20",
  Technician: "bg-warning/15 text-warning-foreground ring-warning/30 dark:text-warning",
  Requestor: "bg-muted text-muted-foreground ring-border",
};

function UsersPage() {
  const navigate = useNavigate();
  const searchParams = Route.useSearch();
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState(searchParams.role || "all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);

  useEffect(() => {
    setRoleFilter(searchParams.role || "all");
  }, [searchParams.role]);
  const [sortCol, setSortCol] = useState("name");
  const [sortDir, setSortDir] = useState("asc");

  // Modal and form states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  // Select inputs
  const [selectedRole, setSelectedRole] = useState("Requestor");
  const [selectedDept, setSelectedDept] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("Active");

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm();

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      syncLocalStorage();
      setData([...users]);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    setSelectedRole("Requestor");
    setSelectedDept(departments[0] || "IT");
    setSelectedStatus("Active");
    reset({ name: "", email: "", phone: "" });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    setSelectedRole(item.role);
    setSelectedDept(item.department);
    setSelectedStatus(item.status);
    reset({ name: item.name, email: item.email, phone: item.phone });
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteUser(deletingId);
    toast.success("User deleted successfully!");
    loadData();
  };

  const onSubmit = (formData) => {
    // Unique check email
    const duplicate = data.find(
      (u) => u.email.toLowerCase() === formData.email.toLowerCase() && u.id !== editingItem?.id,
    );
    if (duplicate) {
      toast.error(`A user with email "${formData.email}" already exists.`);
      return;
    }

    if (editingItem) {
      updateUser({
        ...editingItem,
        ...formData,
        role: selectedRole,
        department: selectedDept,
        status: selectedStatus,
      });
      toast.success("User updated successfully!");
    } else {
      addUser({
        id: String(Date.now()),
        ...formData,
        role: selectedRole,
        department: selectedDept,
        status: selectedStatus,
        joined: new Date().toISOString().split("T")[0],
        requestsRaised: 0,
        requestsResolved: 0,
      });
      toast.success("User created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  // Sorting, filtering and pagination logic
  const processedData = useMemo(() => {
    let result = [...data];

    // Search filter
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (u) =>
          u.name.toLowerCase().includes(q) ||
          u.email.toLowerCase().includes(q) ||
          u.department.toLowerCase().includes(q),
      );
    }

    // Role filter
    if (roleFilter !== "all") {
      result = result.filter((u) => u.role === roleFilter);
    }

    // Sorting
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];

        if (typeof valA === "number") {
          return sortDir === "asc" ? valA - valB : valB - valA;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
          if (valA < valB) return sortDir === "asc" ? -1 : 1;
          if (valA > valB) return sortDir === "asc" ? 1 : -1;
          return 0;
        }
      });
    }

    return result;
  }, [data, search, roleFilter, sortCol, sortDir]);

  // Paginated items
  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  const handleSort = (colKey) => {
    if (sortCol === colKey) {
      setSortDir(sortDir === "asc" ? "desc" : "asc");
    } else {
      setSortCol(colKey);
      setSortDir("asc");
    }
  };

  const columns = [
    {
      header: "User",
      key: "name",
      sortable: true,
      render: (row) => (
        <Link
          to="/users/$userId"
          params={{ userId: row.id }}
          className="flex min-w-0 items-center gap-3"
        >
          <Avatar className="size-9 shrink-0">
            <AvatarFallback className="bg-primary/10 text-xs font-bold text-primary">
              {row.name
                .split(" ")
                .map((w) => w[0])
                .join("")}
            </AvatarFallback>
          </Avatar>
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold group-hover:text-primary">{row.name}</p>
            <p className="truncate text-xs text-muted-foreground">{row.email}</p>
          </div>
        </Link>
      ),
    },
    {
      header: "Role",
      key: "role",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex rounded-md px-2 py-0.5 text-xs font-semibold ring-1 ring-inset",
            roleStyles[row.role],
          )}
        >
          {row.role}
        </span>
      ),
    },
    {
      header: "Department",
      key: "department",
      sortable: true,
      className: "hidden md:table-cell text-sm",
    },
    {
      header: "Phone",
      key: "phone",
      sortable: true,
      className: "hidden lg:table-cell text-sm text-muted-foreground",
    },
    {
      header: "Status",
      key: "status",
      sortable: true,
      className: "hidden sm:table-cell",
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset",
            row.status === "Active"
              ? "bg-success/10 text-success ring-success/20"
              : "bg-muted text-muted-foreground ring-border",
          )}
        >
          <span className="size-1.5 rounded-full bg-current" />
          {row.status}
        </span>
      ),
    },
    {
      header: "Actions",
      key: "actions",
      className: "text-right",
      render: (row) => (
        <div className="flex justify-end gap-1.5">
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={(e) => {
              e.stopPropagation();
              handleOpenEdit(row);
            }}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg text-muted-foreground hover:text-destructive cursor-pointer"
            onClick={(e) => {
              e.stopPropagation();
              handleOpenDelete(row.id);
            }}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Users"
        description={`${data.length} people across departments`}
        crumbs={[{ label: "Users" }]}
        actions={
          <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
            <Plus className="size-4" /> Add User
          </Button>
        }
      />

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center mb-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by name, email or department…"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setCurrentPage(1);
              }}
              className="h-9 rounded-xl pl-9"
            />
          </div>
          <Select
            value={roleFilter}
            onValueChange={(val) => {
              setRoleFilter(val);
              setCurrentPage(1);
              navigate({ to: "/users", search: { role: val } });
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-40">
              <SelectValue placeholder="All roles" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All roles</SelectItem>
              {["Admin", "HOD", "Technician", "Requestor"].map((r) => (
                <SelectItem key={r} value={r}>
                  {r}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <ReusableTable
          columns={columns}
          data={paginatedData}
          loading={loading}
          emptyMessage="No users found"
          sortColumn={sortCol}
          sortDirection={sortDir}
          onSort={handleSort}
        />

        <ReusablePagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalItems={processedData.length}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={(val) => {
            setPageSize(val);
            setCurrentPage(1);
          }}
        />
      </div>

      {/* Add / Edit Modal */}
      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit User Account" : "Register New User"}
        description="Creates or updates user accounts, designating application roles."
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label
              htmlFor="user-name"
              className="text-xs font-bold text-slate-700 dark:text-slate-300"
            >
              Full Name *
            </Label>
            <Input
              id="user-name"
              placeholder="e.g. Ramesh Patel"
              className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              {...register("name", { required: "Name is required" })}
            />

            {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
          </div>

          <div className="space-y-1.5">
            <Label
              htmlFor="user-email"
              className="text-xs font-bold text-slate-700 dark:text-slate-300"
            >
              Email Address *
            </Label>
            <Input
              id="user-email"
              type="email"
              placeholder="e.g. ramesh@company.com"
              className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              {...register("email", {
                required: "Email is required",
                pattern: {
                  value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                  message: "Invalid email address format",
                },
              })}
            />

            {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
          </div>

          <div className="space-y-1.5">
            <Label
              htmlFor="user-phone"
              className="text-xs font-bold text-slate-700 dark:text-slate-300"
            >
              Phone Number *
            </Label>
            <Input
              id="user-phone"
              placeholder="e.g. +91 98765 43210"
              className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              {...register("phone", { required: "Phone number is required" })}
            />

            {errors.phone && <p className="text-xs text-destructive">{errors.phone.message}</p>}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                Application Role *
              </Label>
              <Select value={selectedRole} onValueChange={(val) => setSelectedRole(val)}>
                <SelectTrigger className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {["Admin", "HOD", "Technician", "Requestor"].map((r) => (
                    <SelectItem key={r} value={r}>
                      {r}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                Department Assignment *
              </Label>
              <Select value={selectedDept} onValueChange={setSelectedDept}>
                <SelectTrigger className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100">
                  <SelectValue placeholder="Choose department" />
                </SelectTrigger>
                <SelectContent>
                  {departments.map((d) => (
                    <SelectItem key={d} value={d}>
                      {d}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="space-y-1.5">
            <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
              User Status *
            </Label>
            <Select value={selectedStatus} onValueChange={(val) => setSelectedStatus(val)}>
              <SelectTrigger className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Active">Active</SelectItem>
                <SelectItem value="Inactive">Inactive</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => setIsModalOpen(false)}
              className="rounded-xl cursor-pointer"
            >
              Cancel
            </Button>
            <Button type="submit" className="rounded-xl cursor-pointer">
              {editingItem ? "Save Changes" : "Register User"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      {/* Delete Confirmation */}
      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete User Account?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.name : ""}
      />
    </div>
  );
}
