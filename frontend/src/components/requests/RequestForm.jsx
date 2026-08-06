import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  getServiceTypes,
  getRequestTypes,
  getDepartments,
  getUsers,
  getServiceRequestStatuses,
  createServiceRequest,
  updateServiceRequest,
  createApproval,
} from "@/services/api";
import { useAuth } from "@/lib/auth";
import { Paperclip, Loader2 } from "lucide-react";
import { useState, useEffect } from "react";

export function RequestForm({ existing }) {
  const navigate = useNavigate();
  const { role, user } = useAuth();

  const [serviceTypeList, setServiceTypeList] = useState([]);
  const [requestTypeList, setRequestTypeList] = useState([]);
  const [departmentList, setDepartmentList] = useState([]);
  const [technicianList, setTechnicianList] = useState([]);
  const [statusList, setStatusList] = useState([]);

  const [serviceType, setServiceType] = useState(existing?.serviceType ?? "");
  const [requestType, setRequestType] = useState(existing?.requestType ?? "");
  const [department, setDepartment] = useState(existing?.department ?? "");
  const [priority, setPriority] = useState(existing?.priority ?? "Medium");
  const [assignee, setAssignee] = useState(existing?.assignee ?? null);
  const [attachments, setAttachments] = useState(existing?.attachments || []);
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    defaultValues: existing
      ? { title: existing.title, description: existing.description }
      : undefined,
  });

  useEffect(() => {
    async function loadOptions() {
      try {
        const [apiServTypes, apiReqTypes, apiDepts, apiUsers, apiStatuses] = await Promise.all([
          getServiceTypes().catch(() => []),
          getRequestTypes().catch(() => []),
          getDepartments().catch(() => []),
          getUsers().catch(() => []),
          getServiceRequestStatuses().catch(() => []),
        ]);

        setServiceTypeList(apiServTypes || []);
        setRequestTypeList(apiReqTypes || []);
        setDepartmentList(apiDepts || []);
        setStatusList(apiStatuses || []);

        const techs = (apiUsers || []).filter((u) => u.roleName === "Technician" || u.role === "Technician" || u.roleId === 3);
        setTechnicianList(techs.length > 0 ? techs : apiUsers || []);

        if (!existing) {
          if (apiServTypes.length > 0 && !serviceType) setServiceType(apiServTypes[0].serviceTypeName);
          if (apiReqTypes.length > 0 && !requestType) setRequestType(apiReqTypes[0].requestTypeName);
          if (apiDepts.length > 0 && !department) setDepartment(apiDepts[0].departmentName);
        }
      } catch (err) {
      }
    }
    loadOptions();
  }, [existing]);

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files.length > 0) {
      const fileList = Array.from(e.target.files);
      const newAttachments = fileList.map((file) => ({
        id: String(Date.now() + Math.random()),
        name: file.name,
        size: `${Math.round(file.size / 1024)} KB`,
        url: "#",
      }));
      setAttachments([...(attachments || []), ...newAttachments]);
      toast.success("File attached successfully!");
    }
  };

  const handleRemoveAttachment = (id) => {
    setAttachments((attachments || []).filter((a) => a.id !== id));
    toast.success("Attachment removed");
  };

  const onSubmit = async (data) => {
    setSubmitting(true);
    try {
      const isAssignAllowed = role === "Admin" || role === "HOD";
      const selectedAssignee = isAssignAllowed ? assignee : (existing?.assignee ?? null);

      // Find IDs
      let servTypeId = serviceTypeList.find((s) => s.serviceTypeName === serviceType)?.serviceTypeId || 1;
      let reqTypeId = requestTypeList.find((r) => r.requestTypeName === requestType)?.requestTypeId || 1;
      let deptId = departmentList.find((d) => d.departmentName === department)?.departmentId || 1;
      let assigneeUser = technicianList.find((t) => (t.fullName || t.name) === selectedAssignee);
      let assigneeUserId = assigneeUser ? assigneeUser.userId : null;
      
      let pendingStatusId = statusList.find((s) => s.statusName === "Pending")?.statusId || 1;
      let assignedStatusId = statusList.find((s) => s.statusName === "Assigned")?.statusId || 2;
      
      let statusId = selectedAssignee ? assignedStatusId : pendingStatusId;
      let currentUserId = user?.userId || 1;

      if (existing) {
        let newStatusId = statusList.find((s) => s.statusName === existing.status)?.statusId || statusId;
        if (isAssignAllowed) {
          if (existing.status === "Pending" && selectedAssignee) {
            newStatusId = assignedStatusId;
          } else if (existing.status === "Assigned" && !selectedAssignee) {
            newStatusId = pendingStatusId;
          }
        }

        const updatePayload = {
          requestId: Number(existing.id),
          requestNumber: existing.no || `SR-2026-${existing.id}`,
          title: data.title,
          description: data.description,
          serviceTypeId: servTypeId,
          requestTypeId: reqTypeId,
          departmentId: deptId,
          requesterUserId: existing.requesterUserId || currentUserId,
          assigneeUserId: assigneeUserId,
          statusId: newStatusId,
          priority: priority,
          updatedByUserId: currentUserId,
          createdByUserId: existing.createdByUserId || currentUserId,
        };

        await updateServiceRequest(existing.id, updatePayload);
        toast.success(`Request ${existing.no} updated successfully!`);
      } else {
        const reqNo = `SR-2026-${Date.now().toString().slice(-4)}`;

        const createPayload = {
          requestNumber: reqNo,
          title: data.title,
          description: data.description,
          serviceTypeId: servTypeId,
          requestTypeId: reqTypeId,
          departmentId: deptId,
          requesterUserId: currentUserId,
          assigneeUserId: assigneeUserId,
          statusId: statusId,
          priority: priority,
          createdByUserId: currentUserId,
          updatedByUserId: currentUserId,
        };

        const created = await createServiceRequest(createPayload);

        const isApprovalNeeded = ["Software Request", "Access Request", "Hardware Request"].includes(requestType);
        if (isApprovalNeeded) {
          await createApproval({
            requestId: created.requestId,
            status: "Pending",
          }).catch(() => null);
        }

        toast.success(`Request submitted successfully! Tracking no: ${reqNo}`);
      }

      navigate({ to: "/requests" });
    } catch (err) {
      toast.error(err.message || "Failed to save request.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card"
    >
      <div className="grid gap-5 sm:grid-cols-2">
        <div className="space-y-1.5 sm:col-span-2">
          <Label htmlFor="title">Request title *</Label>
          <Input
            id="title"
            placeholder="Brief summary of the issue"
            className="h-10 rounded-xl"
            {...register("title", { required: "Title is required" })}
          />

          {errors.title && <p className="text-xs text-destructive">{errors.title.message}</p>}
        </div>

        <div className="space-y-1.5">
          <Label>Service type *</Label>
          <Select value={serviceType} onValueChange={setServiceType}>
            <SelectTrigger className="h-10 w-full rounded-xl">
              <SelectValue placeholder="Select service type" />
            </SelectTrigger>
            <SelectContent>
              {serviceTypeList.map((t) => t.serviceTypeName).map((t) => (
                <SelectItem key={t} value={t}>
                  {t}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label>Request type *</Label>
          <Select value={requestType} onValueChange={setRequestType}>
            <SelectTrigger className="h-10 w-full rounded-xl">
              <SelectValue placeholder="Select request type" />
            </SelectTrigger>
            <SelectContent>
              {requestTypeList.map((t) => t.requestTypeName).map((t) => (
                <SelectItem key={t} value={t}>
                  {t}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label>Department *</Label>
          <Select value={department} onValueChange={setDepartment}>
            <SelectTrigger className="h-10 w-full rounded-xl">
              <SelectValue placeholder="Select department" />
            </SelectTrigger>
            <SelectContent>
              {departmentList.map((d) => d.departmentName).map((d) => (
                <SelectItem key={d} value={d}>
                  {d}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label>Priority</Label>
          <Select value={priority} onValueChange={(v) => setPriority(v)}>
            <SelectTrigger className="h-10 w-full rounded-xl">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {["Critical", "High", "Medium", "Low"].map((p) => (
                <SelectItem key={p} value={p}>
                  {p}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {role === "Admin" || role === "HOD" ? (
          <div className="space-y-1.5">
            <Label>Assigned Technician</Label>
            <Select
              value={assignee ?? "unassigned"}
              onValueChange={(v) => setAssignee(v === "unassigned" ? null : v)}
            >
              <SelectTrigger className="h-10 w-full rounded-xl">
                <SelectValue placeholder="Unassigned" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="unassigned">Unassigned</SelectItem>
                {technicianList.map((t) => {
                  const techName = t.fullName || t.name;
                  return (
                    <SelectItem key={t.userId || techName} value={techName}>
                      {techName}
                    </SelectItem>
                  );
                })}
              </SelectContent>
            </Select>
          </div>
        ) : existing?.assignee ? (
          <div className="space-y-1.5">
            <Label>Assigned Technician</Label>
            <Input value={existing.assignee} disabled className="h-10 rounded-xl bg-muted" />
          </div>
        ) : null}

        <div className="space-y-1.5 sm:col-span-2">
          <Label htmlFor="description">Description *</Label>
          <Textarea
            id="description"
            rows={5}
            placeholder="Describe the issue in detail — what happened, when, and any error messages."
            className="rounded-xl"
            {...register("description", {
              required: "Description is required",
              minLength: { value: 20, message: "Please provide at least 20 characters" },
            })}
          />

          {errors.description && (
            <p className="text-xs text-destructive">{errors.description.message}</p>
          )}
        </div>

        <div className="sm:col-span-2">
          <Label>Attachments</Label>
          <label className="mt-1.5 flex cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed p-6 text-center transition-colors hover:border-primary/50 hover:bg-accent/50">
            <Paperclip className="size-5 text-muted-foreground" />
            <p className="mt-2 text-sm font-medium">Drop files here or click to browse</p>
            <p className="text-xs text-muted-foreground">PNG, JPG, PDF up to 10 MB</p>
            <input type="file" className="hidden" onChange={handleFileChange} multiple />
          </label>
          {attachments && attachments.length > 0 && (
            <div className="mt-3 space-y-2">
              <p className="text-xs font-semibold text-muted-foreground">
                Attached Files ({attachments.length})
              </p>
              <div className="grid gap-2 sm:grid-cols-2">
                {attachments.map((file) => (
                  <div
                    key={file.id}
                    className="flex items-center justify-between rounded-xl border bg-accent/30 p-2.5 text-sm"
                  >
                    <div className="flex items-center gap-2 min-w-0">
                      <Paperclip className="size-4 shrink-0 text-muted-foreground" />
                      <span className="truncate font-medium">{file.name}</span>
                      <span className="text-xs text-muted-foreground">({file.size})</span>
                    </div>
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="h-7 w-7 p-0 text-muted-foreground hover:text-destructive shrink-0 cursor-pointer"
                      onClick={() => handleRemoveAttachment(file.id)}
                    >
                      ×
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="mt-6 flex flex-wrap justify-end gap-2">
        <Button
          type="button"
          variant="outline"
          className="rounded-xl"
          onClick={() => navigate({ to: "/requests" })}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={submitting} className="rounded-xl px-6 cursor-pointer">
          {submitting && <Loader2 className="size-4 animate-spin mr-2" />}
          {existing ? "Save changes" : "Submit request"}
        </Button>
      </div>
    </form>
  );
}
