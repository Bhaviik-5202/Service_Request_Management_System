import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { PageHeader } from "@/components/shared/PageHeader";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useState, useMemo, useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import {
  Plus,
  Edit2,
  Trash2,
  CheckCircle,
  XCircle,
  Activity,
  FolderOpen,
  UserCheck,
  Tag,
  FileText,
  GitCommit,
} from "lucide-react";
import { cn } from "@/lib/utils";

// Data and CRUD imports
import {
  statuses,
  departmentsMaster,
  departmentPersons,
  serviceTypesMaster,
  requestTypesMaster,
  requestTypePersonMappings,
  users,
  // CRUD actions
  addStatus,
  updateStatus,
  deleteStatus,
  addDepartment,
  updateDepartment,
  deleteDepartment,
  addDepartmentPerson,
  updateDepartmentPerson,
  deleteDepartmentPerson,
  addServiceType,
  updateServiceType,
  deleteServiceType,
  addRequestType,
  updateRequestType,
  deleteRequestType,
  addRequestTypePersonMapping,
  updateRequestTypePersonMapping,
  deleteRequestTypePersonMapping,
  // Types
} from "@/data/mock";

// Reusable components
import { ReusableTable } from "@/components/shared/ReusableTable";
import { ReusableModal } from "@/components/shared/ReusableModal";
import { ReusablePagination } from "@/components/shared/ReusablePagination";
import { SearchBar } from "@/components/shared/SearchBar";
import { DeleteConfirmation } from "@/components/shared/DeleteConfirmation";

export const Route = createFileRoute("/_shell/masters")({
  validateSearch: (search) => ({
    tab: search.tab || "statuses",
  }),
  head: () => ({
    meta: [
      { title: "Configuration Masters — ServiceDesk" },
      {
        name: "description",
        content: "Configure service desk statuses, departments, personnel, types and mappings.",
      },
    ],
  }),
  component: MastersPage,
});

function MastersPage() {
  const navigate = useNavigate();
  const searchParams = Route.useSearch();
  const [activeTab, setActiveTab] = useState(searchParams.tab || "statuses");

  useEffect(() => {
    if (searchParams.tab) {
      setActiveTab(searchParams.tab);
    }
  }, [searchParams.tab]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Configuration Masters"
        description="Manage the status pipeline, departments, staff, types, and mappings"
        crumbs={[{ label: "Masters" }]}
      />

      <Tabs
        value={activeTab}
        onValueChange={(val) => {
          setActiveTab(val);
          navigate({ to: "/masters", search: { tab: val } });
        }}
        className="w-full"
      >
        <TabsList className="w-full justify-start overflow-x-auto rounded-xl h-auto p-1 bg-muted/60 mb-6 flex-wrap md:flex-nowrap">
          <TabsTrigger value="statuses" className="rounded-lg gap-1.5 py-2 px-3">
            <Activity className="size-4" />
            Statuses
          </TabsTrigger>
          <TabsTrigger value="departments" className="rounded-lg gap-1.5 py-2 px-3">
            <FolderOpen className="size-4" />
            Departments
          </TabsTrigger>
          <TabsTrigger value="personnel" className="rounded-lg gap-1.5 py-2 px-3">
            <UserCheck className="size-4" />
            Personnel Mapping
          </TabsTrigger>
          <TabsTrigger value="serviceTypes" className="rounded-lg gap-1.5 py-2 px-3">
            <Tag className="size-4" />
            Service Types
          </TabsTrigger>
          <TabsTrigger value="requestTypes" className="rounded-lg gap-1.5 py-2 px-3">
            <FileText className="size-4" />
            Request Types
          </TabsTrigger>
          <TabsTrigger value="mappings" className="rounded-lg gap-1.5 py-2 px-3">
            <GitCommit className="size-4" />
            Technician Assignment
          </TabsTrigger>
        </TabsList>

        <div className="mt-2 min-h-[400px]">
          <TabsContent value="statuses" className="outline-none">
            <StatusesManager />
          </TabsContent>
          <TabsContent value="departments" className="outline-none">
            <DepartmentsManager />
          </TabsContent>
          <TabsContent value="personnel" className="outline-none">
            <PersonnelManager />
          </TabsContent>
          <TabsContent value="serviceTypes" className="outline-none">
            <ServiceTypesManager />
          </TabsContent>
          <TabsContent value="requestTypes" className="outline-none">
            <RequestTypesManager />
          </TabsContent>
          <TabsContent value="mappings" className="outline-none">
            <MappingsManager />
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
}

// ==========================================
// 1. STATUSES MANAGER
// ==========================================
function StatusesManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("name");
  const [sortDir, setSortDir] = useState("asc");

  // Modal & form states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm();

  // Fetch initial data
  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      setData([...statuses]);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    reset({
      name: "",
      color: "bg-info/10 text-info ring-info/20",
      description: "",
      isActive: true,
    });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    reset({
      name: item.name,
      color: item.color,
      description: item.description,
      isActive: item.isActive,
    });
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteStatus(deletingId);
    toast.success("Status deleted successfully!");
    loadData();
  };

  const onSubmit = (formData) => {
    // Unique check
    const duplicate = data.find(
      (s) => s.name.toLowerCase() === formData.name.toLowerCase() && s.id !== editingItem?.id,
    );
    if (duplicate) {
      toast.error(`Status with name "${formData.name}" already exists.`);
      return;
    }

    if (editingItem) {
      updateStatus({
        ...editingItem,
        ...formData,
      });
      toast.success("Status updated successfully!");
    } else {
      addStatus({
        id: String(Date.now()),
        ...formData,
      });
      toast.success("Status created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  // Sorting, filtering and pagination logic
  const processedData = useMemo(() => {
    let result = [...data];

    // Filter search
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (s) =>
          s.name.toLowerCase().includes(q) ||
          s.description.toLowerCase().includes(q) ||
          s.color.toLowerCase().includes(q),
      );
    }

    // Filter status
    if (statusFilter !== "all") {
      const active = statusFilter === "active";
      result = result.filter((s) => s.isActive === active);
    }

    // Sort
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];

        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }

        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }

    return result;
  }, [data, search, statusFilter, sortCol, sortDir]);

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
      header: "Status Name",
      key: "name",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset",
            row.color,
          )}
        >
          {row.name}
        </span>
      ),
    },
    {
      header: "Description",
      key: "description",
      sortable: true,
      className: "hidden md:table-cell text-sm text-muted-foreground",
    },
    {
      header: "Color Code / Class",
      key: "color",
      sortable: true,
      className: "hidden lg:table-cell text-xs font-mono",
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active" : "Inactive"}
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
            className="size-8 rounded-lg text-muted-foreground hover:text-foreground cursor-pointer"
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg text-muted-foreground hover:text-destructive cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Status Pipeline Masters</h2>
          <p className="text-xs text-muted-foreground">
            Configure workflow status names and colors.
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Add Status
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search status name, description..."
          />

          <Select
            value={statusFilter}
            onValueChange={(val) => {
              setStatusFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-40">
              <SelectValue placeholder="All states" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All states</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No statuses found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={handleSort}
          />
        </div>

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
        title={editingItem ? "Edit Request Status" : "Create Request Status"}
        description="Statuses define the pipeline of a request. Colors are Tailwind tag classes."
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label htmlFor="name" className="text-xs font-bold text-slate-700 dark:text-slate-300">
              Status Name *
            </Label>
            <Input
              id="name"
              placeholder="e.g. Awaiting Vendor"
              className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              {...register("name", { required: "Name is required" })}
            />

            {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="color" className="text-xs font-bold text-slate-700 dark:text-slate-300">
              Badge Styling Class *
            </Label>
            <Input
              id="color"
              placeholder="e.g. bg-info/10 text-info ring-info/20"
              className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              {...register("color", { required: "Styling class is required" })}
            />

            <p className="text-[10px] text-muted-foreground leading-normal">
              Provide Tailwind classes. Example:{" "}
              <code className="bg-muted px-1 rounded">
                bg-success/10 text-success ring-success/20
              </code>
            </p>
            {errors.color && <p className="text-xs text-destructive">{errors.color.message}</p>}
          </div>

          <div className="space-y-1.5">
            <Label
              htmlFor="description"
              className="text-xs font-bold text-slate-700 dark:text-slate-300"
            >
              Description
            </Label>
            <Textarea
              id="description"
              placeholder="Explain when this status is applied..."
              className="rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100"
              rows={3}
              {...register("description")}
            />
          </div>

          <div className="flex items-center justify-between rounded-xl border border-border/80 dark:border-border/20 p-3.5 bg-muted/40 dark:bg-accent/10">
            <div>
              <p className="text-sm font-semibold">Active Status</p>
              <p className="text-xs text-muted-foreground">
                Controls if status can be used for new requests.
              </p>
            </div>
            <Switch
              defaultChecked={editingItem ? editingItem.isActive : true}
              onCheckedChange={(checked) => setValue("isActive", checked)}
            />
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
              {editingItem ? "Save Changes" : "Create Status"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      {/* Delete Confirmation */}
      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Status Master?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.name : ""}
      />
    </div>
  );
}

// ==========================================
// 2. DEPARTMENTS MANAGER
// ==========================================
function DepartmentsManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("name");
  const [sortDir, setSortDir] = useState("asc");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm();

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      setData([...departmentsMaster]);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    reset({ name: "", code: "", description: "", isActive: true });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    reset({
      name: item.name,
      code: item.code,
      description: item.description,
      isActive: item.isActive,
    });
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteDepartment(deletingId);
    toast.success("Department deleted successfully!");
    loadData();
  };

  const onSubmit = (formData) => {
    // Unique check
    const dupName = data.find(
      (d) => d.name.toLowerCase() === formData.name.toLowerCase() && d.id !== editingItem?.id,
    );
    const dupCode = data.find(
      (d) => d.code.toUpperCase() === formData.code.toUpperCase() && d.id !== editingItem?.id,
    );
    if (dupName) {
      toast.error(`Department "${formData.name}" already exists.`);
      return;
    }
    if (dupCode) {
      toast.error(`Department Code "${formData.code}" is already in use.`);
      return;
    }

    const uppercaseData = {
      ...formData,
      code: formData.code.toUpperCase(),
    };

    if (editingItem) {
      updateDepartment({ ...editingItem, ...uppercaseData });
      toast.success("Department updated successfully!");
    } else {
      addDepartment({ id: String(Date.now()), ...uppercaseData });
      toast.success("Department created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  const processedData = useMemo(() => {
    let result = [...data];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (x) =>
          x.name.toLowerCase().includes(q) ||
          x.code.toLowerCase().includes(q) ||
          x.description.toLowerCase().includes(q),
      );
    }
    if (statusFilter !== "all") {
      result = result.filter((x) => x.isActive === (statusFilter === "active"));
    }
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];
        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }
        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [data, search, statusFilter, sortCol, sortDir]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  const columns = [
    { header: "Department Name", key: "name", sortable: true, className: "font-semibold text-sm" },
    {
      header: "Code",
      key: "code",
      sortable: true,
      className: "text-xs font-mono text-primary font-semibold",
    },
    {
      header: "Description",
      key: "description",
      sortable: true,
      className: "hidden md:table-cell text-sm text-muted-foreground",
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active" : "Inactive"}
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
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Service Department Masters</h2>
          <p className="text-xs text-muted-foreground">
            Manage service departments handling requests.
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Add Department
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search department name, code, description..."
          />

          <Select
            value={statusFilter}
            onValueChange={(val) => {
              setStatusFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All states</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No departments found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={(c) => {
              if (sortCol === c) setSortDir(sortDir === "asc" ? "desc" : "asc");
              else {
                setSortCol(c);
                setSortDir("asc");
              }
            }}
          />
        </div>

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

      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit Department" : "Create Department"}
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="sm:col-span-2 space-y-1.5">
              <Label htmlFor="dept-name">Department Name *</Label>
              <Input
                id="dept-name"
                placeholder="e.g. IT Helpdesk"
                className="h-10 rounded-xl"
                {...register("name", { required: "Name is required" })}
              />

              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="dept-code">Code *</Label>
              <Input
                id="dept-code"
                placeholder="e.g. ITHD"
                className="h-10 rounded-xl uppercase font-mono"
                {...register("code", {
                  required: "Code is required",
                  maxLength: { value: 10, message: "Code cannot exceed 10 chars" },
                })}
              />

              {errors.code && <p className="text-xs text-destructive">{errors.code.message}</p>}
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="dept-description">Description</Label>
            <Textarea
              id="dept-description"
              placeholder="Description of the department responsibilities..."
              className="rounded-xl"
              rows={3}
              {...register("description")}
            />
          </div>

          <div className="flex items-center justify-between rounded-xl border p-3.5 bg-accent/30">
            <div>
              <p className="text-sm font-semibold">Active Department</p>
              <p className="text-xs text-muted-foreground">
                Inactive departments won't show in new request selectors.
              </p>
            </div>
            <Switch
              defaultChecked={editingItem ? editingItem.isActive : true}
              onCheckedChange={(checked) => setValue("isActive", checked)}
            />
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
              {editingItem ? "Save Changes" : "Create Department"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Department Master?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.name : ""}
      />
    </div>
  );
}

// ==========================================
// 3. SERVICE DEPARTMENT PERSON MANAGER
// ==========================================
function PersonnelManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [deptFilter, setDeptFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("userName");
  const [sortDir, setSortDir] = useState("asc");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  // Selectable drop downs
  const [selectedUser, setSelectedUser] = useState("");
  const [selectedDept, setSelectedDept] = useState("");
  const [isHOD, setIsHOD] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      // Map names for display & filtering
      const mapped = departmentPersons.map((dp) => {
        const u = users.find((x) => x.id === dp.userId);
        const d = departmentsMaster.find((x) => x.id === dp.departmentId);
        return {
          ...dp,
          userName: u ? u.name : "Unknown User",
          userEmail: u ? u.email : "",
          deptName: d ? d.name : "Unknown Department",
        };
      });
      setData(mapped);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    setSelectedUser("");
    setSelectedDept("");
    setIsHOD(false);
    setIsActive(true);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    setSelectedUser(item.userId);
    setSelectedDept(item.departmentId);
    setIsHOD(item.isHOD);
    setIsActive(item.isActive);
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteDepartmentPerson(deletingId);
    toast.success("Personnel mapping removed successfully!");
    loadData();
  };

  const handleSave = (e) => {
    e.preventDefault();
    if (!selectedUser || !selectedDept) {
      toast.error("Please select a user and a department.");
      return;
    }

    // Duplicate check: can't map same user to same department twice
    const dup = data.find(
      (x) =>
        x.userId === selectedUser && x.departmentId === selectedDept && x.id !== editingItem?.id,
    );
    if (dup) {
      toast.error("This user is already mapped to this department.");
      return;
    }

    const payload = {
      id: editingItem ? editingItem.id : String(Date.now()),
      userId: selectedUser,
      departmentId: selectedDept,
      isHOD,
      isActive,
    };

    if (editingItem) {
      updateDepartmentPerson(payload);
      toast.success("Personnel mapping updated successfully!");
    } else {
      addDepartmentPerson(payload);
      toast.success("Personnel mapped successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  const processedData = useMemo(() => {
    let result = [...data];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (x) =>
          x.userName.toLowerCase().includes(q) ||
          x.userEmail.toLowerCase().includes(q) ||
          x.deptName.toLowerCase().includes(q),
      );
    }
    if (deptFilter !== "all") {
      result = result.filter((x) => x.departmentId === deptFilter);
    }
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];
        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }
        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [data, search, deptFilter, sortCol, sortDir]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  const columns = [
    {
      header: "Staff Member",
      key: "userName",
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-semibold text-sm">{row.userName}</p>
          <p className="text-xs text-muted-foreground">{row.userEmail}</p>
        </div>
      ),
    },
    {
      header: "Department",
      key: "deptName",
      sortable: true,
      render: (row) => <span className="text-sm font-medium text-foreground">{row.deptName}</span>,
    },
    {
      header: "Role Designation",
      key: "isHOD",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex rounded px-2 py-0.5 text-xs font-semibold ring-1 ring-inset",
            row.isHOD
              ? "bg-chart-2/10 text-chart-2 ring-chart-2/20"
              : "bg-muted text-muted-foreground ring-border",
          )}
        >
          {row.isHOD ? "HOD (Head of Dept)" : "Technician"}
        </span>
      ),
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active Mapping" : "Inactive"}
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
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Service Department Personnel</h2>
          <p className="text-xs text-muted-foreground">
            Map system users to departments as Technicians or HODs.
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Map Personnel
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search name, email, department..."
          />

          <Select
            value={deptFilter}
            onValueChange={(val) => {
              setDeptFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-48">
              <SelectValue placeholder="All departments" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All departments</SelectItem>
              {departmentsMaster.map((d) => (
                <SelectItem key={d.id} value={d.id}>
                  {d.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No personnel mapping found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={(c) => {
              if (sortCol === c) setSortDir(sortDir === "asc" ? "desc" : "asc");
              else {
                setSortCol(c);
                setSortDir("asc");
              }
            }}
          />
        </div>

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

      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit Personnel Mapping" : "Map Personnel to Department"}
      >
        <form onSubmit={handleSave} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label>Select User *</Label>
            <Select value={selectedUser} onValueChange={setSelectedUser}>
              <SelectTrigger className="h-10 rounded-xl">
                <SelectValue placeholder="Choose a user..." />
              </SelectTrigger>
              <SelectContent>
                {users.map((u) => (
                  <SelectItem key={u.id} value={u.id}>
                    {u.name} ({u.role} - {u.email})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-1.5">
            <Label>Select Department *</Label>
            <Select value={selectedDept} onValueChange={setSelectedDept}>
              <SelectTrigger className="h-10 rounded-xl">
                <SelectValue placeholder="Choose a department..." />
              </SelectTrigger>
              <SelectContent>
                {departmentsMaster
                  .filter((d) => d.isActive)
                  .map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name} ({d.code})
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between rounded-xl border p-3 bg-accent/30">
            <div>
              <p className="text-sm font-semibold">Head of Department (HOD)</p>
              <p className="text-xs text-muted-foreground">
                Designates this person to approve requests for this department.
              </p>
            </div>
            <Switch checked={isHOD} onCheckedChange={setIsHOD} />
          </div>

          <div className="flex items-center justify-between rounded-xl border p-3 bg-accent/30">
            <div>
              <p className="text-sm font-semibold">Active Assignment</p>
              <p className="text-xs text-muted-foreground">
                Control if this mapping is active for service request routing.
              </p>
            </div>
            <Switch checked={isActive} onCheckedChange={setIsActive} />
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
              {editingItem ? "Save Changes" : "Create Mapping"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Remove Personnel Mapping?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.userName : ""}
      />
    </div>
  );
}

// ==========================================
// 4. SERVICE TYPE MANAGER
// ==========================================
function ServiceTypesManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("name");
  const [sortDir, setSortDir] = useState("asc");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm();

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      setData([...serviceTypesMaster]);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    reset({ name: "", code: "", description: "", isActive: true });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    reset({
      name: item.name,
      code: item.code,
      description: item.description,
      isActive: item.isActive,
    });
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteServiceType(deletingId);
    toast.success("Service type deleted successfully!");
    loadData();
  };

  const onSubmit = (formData) => {
    const dupName = data.find(
      (s) => s.name.toLowerCase() === formData.name.toLowerCase() && s.id !== editingItem?.id,
    );
    const dupCode = data.find(
      (s) => s.code.toUpperCase() === formData.code.toUpperCase() && s.id !== editingItem?.id,
    );
    if (dupName) {
      toast.error(`Service Type "${formData.name}" already exists.`);
      return;
    }
    if (dupCode) {
      toast.error(`Service Code "${formData.code}" is already in use.`);
      return;
    }

    const payload = {
      ...formData,
      code: formData.code.toUpperCase(),
    };

    if (editingItem) {
      updateServiceType({ ...editingItem, ...payload });
      toast.success("Service type updated successfully!");
    } else {
      addServiceType({ id: String(Date.now()), ...payload });
      toast.success("Service type created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  const processedData = useMemo(() => {
    let result = [...data];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (x) =>
          x.name.toLowerCase().includes(q) ||
          x.code.toLowerCase().includes(q) ||
          x.description.toLowerCase().includes(q),
      );
    }
    if (statusFilter !== "all") {
      result = result.filter((x) => x.isActive === (statusFilter === "active"));
    }
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];
        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }
        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [data, search, statusFilter, sortCol, sortDir]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  const columns = [
    { header: "Service Type", key: "name", sortable: true, className: "font-semibold text-sm" },
    {
      header: "Code",
      key: "code",
      sortable: true,
      className: "text-xs font-mono text-primary font-semibold",
    },
    {
      header: "Description",
      key: "description",
      sortable: true,
      className: "hidden md:table-cell text-sm text-muted-foreground",
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active" : "Inactive"}
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
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Service Type Masters</h2>
          <p className="text-xs text-muted-foreground">
            Define high-level categories of services (e.g. Technical, Facility).
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Add Service Type
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search service type, code, description..."
          />

          <Select
            value={statusFilter}
            onValueChange={(val) => {
              setStatusFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All states</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No service types found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={(c) => {
              if (sortCol === c) setSortDir(sortDir === "asc" ? "desc" : "asc");
              else {
                setSortCol(c);
                setSortDir("asc");
              }
            }}
          />
        </div>

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

      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit Service Type" : "Create Service Type"}
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="sm:col-span-2 space-y-1.5">
              <Label htmlFor="st-name">Service Type Name *</Label>
              <Input
                id="st-name"
                placeholder="e.g. Technical Support"
                className="h-10 rounded-xl"
                {...register("name", { required: "Name is required" })}
              />

              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="st-code">Code *</Label>
              <Input
                id="st-code"
                placeholder="e.g. TECH"
                className="h-10 rounded-xl uppercase font-mono"
                {...register("code", { required: "Code is required" })}
              />

              {errors.code && <p className="text-xs text-destructive">{errors.code.message}</p>}
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="st-description">Description</Label>
            <Textarea
              id="st-description"
              placeholder="Short description of this service type classification..."
              className="rounded-xl"
              rows={3}
              {...register("description")}
            />
          </div>

          <div className="flex items-center justify-between rounded-xl border p-3.5 bg-accent/30">
            <div>
              <p className="text-sm font-semibold">Active Type</p>
              <p className="text-xs text-muted-foreground">
                Controls if this service type and its child request types are available.
              </p>
            </div>
            <Switch
              defaultChecked={editingItem ? editingItem.isActive : true}
              onCheckedChange={(checked) => setValue("isActive", checked)}
            />
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
              {editingItem ? "Save Changes" : "Create Service Type"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Service Type Master?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.name : ""}
      />
    </div>
  );
}

// ==========================================
// 5. SERVICE REQUEST TYPE MASTER
// ==========================================
function RequestTypesManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [serviceTypeFilter, setServiceTypeFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("name");
  const [sortDir, setSortDir] = useState("asc");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  // Select field state
  const [selectedServiceType, setSelectedServiceType] = useState("");
  const [isActive, setIsActive] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm();

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      // Map parent service type name
      const mapped = requestTypesMaster.map((rt) => {
        const st = serviceTypesMaster.find((x) => x.id === rt.serviceTypeId);
        return {
          ...rt,
          serviceTypeName: st ? st.name : "Unknown Service Type",
        };
      });
      setData(mapped);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    setSelectedServiceType("");
    setIsActive(true);
    reset({ name: "", description: "" });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    setSelectedServiceType(item.serviceTypeId);
    setIsActive(item.isActive);
    reset({ name: item.name, description: item.description });
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteRequestType(deletingId);
    toast.success("Request type deleted successfully!");
    loadData();
  };

  const onSubmit = (formData) => {
    if (!selectedServiceType) {
      toast.error("Please select a parent service type.");
      return;
    }

    const dup = data.find(
      (r) =>
        r.name.toLowerCase() === formData.name.toLowerCase() &&
        r.serviceTypeId === selectedServiceType &&
        r.id !== editingItem?.id,
    );
    if (dup) {
      toast.error(`Request type "${formData.name}" already exists in this service classification.`);
      return;
    }

    const payload = {
      id: editingItem ? editingItem.id : String(Date.now()),
      name: formData.name,
      description: formData.description,
      serviceTypeId: selectedServiceType,
      isActive,
    };

    if (editingItem) {
      updateRequestType(payload);
      toast.success("Request type updated successfully!");
    } else {
      addRequestType(payload);
      toast.success("Request type created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  const processedData = useMemo(() => {
    let result = [...data];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (x) =>
          x.name.toLowerCase().includes(q) ||
          x.description.toLowerCase().includes(q) ||
          x.serviceTypeName.toLowerCase().includes(q),
      );
    }
    if (serviceTypeFilter !== "all") {
      result = result.filter((x) => x.serviceTypeId === serviceTypeFilter);
    }
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];
        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }
        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [data, search, serviceTypeFilter, sortCol, sortDir]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  const columns = [
    { header: "Request Type", key: "name", sortable: true, className: "font-semibold text-sm" },
    {
      header: "Service Classification",
      key: "serviceTypeName",
      sortable: true,
      render: (row) => (
        <span className="text-sm font-medium text-primary">{row.serviceTypeName}</span>
      ),
    },
    {
      header: "Description",
      key: "description",
      sortable: true,
      className: "hidden md:table-cell text-sm text-muted-foreground",
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active" : "Inactive"}
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
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Service Request Type Masters</h2>
          <p className="text-xs text-muted-foreground">
            Manage types of service requests linked to core service classes.
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Add Request Type
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search request type, classification..."
          />

          <Select
            value={serviceTypeFilter}
            onValueChange={(val) => {
              setServiceTypeFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-48">
              <SelectValue placeholder="All classifications" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All classifications</SelectItem>
              {serviceTypesMaster.map((st) => (
                <SelectItem key={st.id} value={st.id}>
                  {st.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No request types found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={(c) => {
              if (sortCol === c) setSortDir(sortDir === "asc" ? "desc" : "asc");
              else {
                setSortCol(c);
                setSortDir("asc");
              }
            }}
          />
        </div>

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

      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit Request Type" : "Create Request Type"}
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label htmlFor="rt-name">Request Type Name *</Label>
            <Input
              id="rt-name"
              placeholder="e.g. Email Password Reset"
              className="h-10 rounded-xl"
              {...register("name", { required: "Name is required" })}
            />

            {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
          </div>

          <div className="space-y-1.5">
            <Label>Parent Service Classification *</Label>
            <Select value={selectedServiceType} onValueChange={setSelectedServiceType}>
              <SelectTrigger className="h-10 rounded-xl">
                <SelectValue placeholder="Select classification..." />
              </SelectTrigger>
              <SelectContent>
                {serviceTypesMaster
                  .filter((s) => s.isActive)
                  .map((st) => (
                    <SelectItem key={st.id} value={st.id}>
                      {st.name} ({st.code})
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="rt-description">Description</Label>
            <Textarea
              id="rt-description"
              placeholder="Provide context or instructions for this request type..."
              className="rounded-xl"
              rows={3}
              {...register("description")}
            />
          </div>

          <div className="flex items-center justify-between rounded-xl border p-3.5 bg-accent/30">
            <div>
              <p className="text-sm font-semibold">Active Status</p>
              <p className="text-xs text-muted-foreground">
                Inactive request types won't appear in request forms.
              </p>
            </div>
            <Switch checked={isActive} onCheckedChange={setIsActive} />
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
              {editingItem ? "Save Changes" : "Create Request Type"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Request Type?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.name : ""}
      />
    </div>
  );
}

// ==========================================
// 6. REQUEST TYPE WISE PERSON MAPPING
// ==========================================
function MappingsManager() {
  const [data, setData] = useState([]);
  const [search, setSearch] = useState("");
  const [requestTypeFilter, setRequestTypeFilter] = useState("all");
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [sortCol, setSortCol] = useState("requestTypeName");
  const [sortDir, setSortDir] = useState("asc");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState("");

  // States for selectors
  const [selectedRequestType, setSelectedRequestType] = useState("");
  const [selectedPerson, setSelectedPerson] = useState("");
  const [isActive, setIsActive] = useState(true);

  const loadData = () => {
    setLoading(true);
    setTimeout(() => {
      // Map names for request types and mapped personnel
      const mapped = requestTypePersonMappings.map((m) => {
        const rt = requestTypesMaster.find((x) => x.id === m.requestTypeId);
        const st = rt ? serviceTypesMaster.find((x) => x.id === rt.serviceTypeId) : null;

        const dp = departmentPersons.find((x) => x.id === m.personId);
        const u = dp ? users.find((x) => x.id === dp.userId) : null;
        const d = dp ? departmentsMaster.find((x) => x.id === dp.departmentId) : null;

        return {
          ...m,
          requestTypeName: rt ? rt.name : "Unknown Request Type",
          serviceTypeName: st ? st.name : "",
          personName: u ? u.name : "Unknown Staff",
          personEmail: u ? u.email : "",
          personDept: d ? d.name : "",
        };
      });
      setData(mapped);
      setLoading(false);
    }, 250);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenAdd = () => {
    setEditingItem(null);
    setSelectedRequestType("");
    setSelectedPerson("");
    setIsActive(true);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item) => {
    setEditingItem(item);
    setSelectedRequestType(item.requestTypeId);
    setSelectedPerson(item.personId);
    setIsActive(item.isActive);
    setIsModalOpen(true);
  };

  const handleOpenDelete = (id) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = () => {
    deleteRequestTypePersonMapping(deletingId);
    toast.success("Assignment mapping deleted successfully!");
    loadData();
  };

  const handleSave = (e) => {
    e.preventDefault();
    if (!selectedRequestType || !selectedPerson) {
      toast.error("Please select a request type and a technician.");
      return;
    }

    const dup = data.find(
      (m) =>
        m.requestTypeId === selectedRequestType &&
        m.personId === selectedPerson &&
        m.id !== editingItem?.id,
    );
    if (dup) {
      toast.error("This assignment mapping already exists.");
      return;
    }

    const payload = {
      id: editingItem ? editingItem.id : String(Date.now()),
      requestTypeId: selectedRequestType,
      personId: selectedPerson,
      isActive,
    };

    if (editingItem) {
      updateRequestTypePersonMapping(payload);
      toast.success("Assignment mapping updated successfully!");
    } else {
      addRequestTypePersonMapping(payload);
      toast.success("Assignment mapping created successfully!");
    }
    setIsModalOpen(false);
    loadData();
  };

  const processedData = useMemo(() => {
    let result = [...data];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (x) =>
          x.requestTypeName.toLowerCase().includes(q) ||
          x.personName.toLowerCase().includes(q) ||
          x.personDept.toLowerCase().includes(q),
      );
    }
    if (requestTypeFilter !== "all") {
      result = result.filter((x) => x.requestTypeId === requestTypeFilter);
    }
    if (sortCol) {
      result.sort((a, b) => {
        let valA = a[sortCol];
        let valB = b[sortCol];
        if (typeof valA === "boolean") {
          valA = valA ? 1 : 0;
          valB = valB ? 1 : 0;
        } else {
          valA = String(valA).toLowerCase();
          valB = String(valB).toLowerCase();
        }
        if (valA < valB) return sortDir === "asc" ? -1 : 1;
        if (valA > valB) return sortDir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [data, search, requestTypeFilter, sortCol, sortDir]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize]);

  const totalPages = Math.ceil(processedData.length / pageSize) || 1;

  // Retrieve active technicians for the selector dropdown
  const techniciansList = useMemo(() => {
    return departmentPersons
      .filter((dp) => dp.isActive)
      .map((dp) => {
        const u = users.find((x) => x.id === dp.userId);
        const d = departmentsMaster.find((x) => x.id === dp.departmentId);
        return {
          id: dp.id,
          name: u ? u.name : "Unknown",
          email: u ? u.email : "",
          dept: d ? d.name : "",
        };
      });
  }, []);

  const columns = [
    {
      header: "Service Request Type",
      key: "requestTypeName",
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-semibold text-sm">{row.requestTypeName}</p>
          <p className="text-xs text-muted-foreground">{row.serviceTypeName}</p>
        </div>
      ),
    },
    {
      header: "Assigned Staff / Tech",
      key: "personName",
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-semibold text-sm text-primary">{row.personName}</p>
          <p className="text-[11px] text-muted-foreground">
            {row.personDept} · {row.personEmail}
          </p>
        </div>
      ),
    },
    {
      header: "Status",
      key: "isActive",
      sortable: true,
      render: (row) => (
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium",
            row.isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
          )}
        >
          {row.isActive ? <CheckCircle className="size-3" /> : <XCircle className="size-3" />}
          {row.isActive ? "Active Assign" : "Inactive"}
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
            onClick={() => handleOpenEdit(row)}
          >
            <Edit2 className="size-3.5" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="size-8 rounded-lg cursor-pointer"
            onClick={() => handleOpenDelete(row.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold">Service Request Type Wise Assignee Mappings</h2>
          <p className="text-xs text-muted-foreground">
            Map request types directly to preferred resolution technicians.
          </p>
        </div>
        <Button onClick={handleOpenAdd} className="rounded-xl gap-1.5 cursor-pointer">
          <Plus className="size-4" /> Map Technician
        </Button>
      </div>

      <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <SearchBar
            value={search}
            onChange={(val) => {
              setSearch(val);
              setCurrentPage(1);
            }}
            placeholder="Search request type, staff name, dept..."
          />

          <Select
            value={requestTypeFilter}
            onValueChange={(val) => {
              setRequestTypeFilter(val);
              setCurrentPage(1);
            }}
          >
            <SelectTrigger className="h-9 rounded-xl sm:w-48">
              <SelectValue placeholder="All request types" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All request types</SelectItem>
              {requestTypesMaster
                .filter((rt) => rt.isActive)
                .map((rt) => (
                  <SelectItem key={rt.id} value={rt.id}>
                    {rt.name}
                  </SelectItem>
                ))}
            </SelectContent>
          </Select>
        </div>

        <div className="mt-4">
          <ReusableTable
            columns={columns}
            data={paginatedData}
            loading={loading}
            emptyMessage="No assignments found"
            sortColumn={sortCol}
            sortDirection={sortDir}
            onSort={(c) => {
              if (sortCol === c) setSortDir(sortDir === "asc" ? "desc" : "asc");
              else {
                setSortCol(c);
                setSortDir("asc");
              }
            }}
          />
        </div>

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

      <ReusableModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingItem ? "Edit Assignment Mapping" : "Map Request Type Assignee"}
      >
        <form onSubmit={handleSave} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
              Select Request Type *
            </Label>
            <Select value={selectedRequestType} onValueChange={setSelectedRequestType}>
              <SelectTrigger className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100">
                <SelectValue placeholder="Select request type..." />
              </SelectTrigger>
              <SelectContent>
                {requestTypesMaster
                  .filter((r) => r.isActive)
                  .map((rt) => (
                    <SelectItem key={rt.id} value={rt.id}>
                      {rt.name}
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-1.5">
            <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
              Select Assigned Technician *
            </Label>
            <Select value={selectedPerson} onValueChange={setSelectedPerson}>
              <SelectTrigger className="h-10 rounded-xl bg-background/80 dark:bg-transparent border-border/80 dark:border-border/30 text-slate-800 dark:text-slate-100">
                <SelectValue placeholder="Select technician..." />
              </SelectTrigger>
              <SelectContent>
                {techniciansList.map((tech) => (
                  <SelectItem key={tech.id} value={tech.id}>
                    {tech.name} ({tech.dept} · {tech.email})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between rounded-xl border border-border/80 dark:border-border/20 p-3.5 bg-muted/40 dark:bg-accent/10">
            <div>
              <p className="text-sm font-semibold">Active Mapping</p>
              <p className="text-xs text-muted-foreground">
                Controls if this mapping is active for routing new tickets.
              </p>
            </div>
            <Switch checked={isActive} onCheckedChange={setIsActive} />
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
              {editingItem ? "Save Changes" : "Create Assignment"}
            </Button>
          </div>
        </form>
      </ReusableModal>

      <DeleteConfirmation
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Assignment Mapping?"
        itemName={deletingId ? data.find((x) => x.id === deletingId)?.requestTypeName : ""}
      />
    </div>
  );
}
