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
  departments,
  requestTypes,
  serviceTypes,
  addRequest,
  updateRequest,
  requests,
  technicians,
} from "@/data/mock";
import { useAuth, ROLE_PROFILES } from "@/lib/auth";
import { Paperclip } from "lucide-react";
import { useState } from "react";

export function RequestForm({ existing }) {
  const navigate = useNavigate();
  const { role } = useAuth();
  const activeProfile = role
    ? ROLE_PROFILES[role]
    : { name: "Demo User", email: "demo@company.com" };

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    defaultValues: existing
      ? { title: existing.title, description: existing.description }
      : undefined,
  });
  const [serviceType, setServiceType] = useState(existing?.serviceType ?? "Technical");
  const [requestType, setRequestType] = useState(existing?.requestType ?? "Computer Issue");
  const [department, setDepartment] = useState(existing?.department ?? "IT");
  const [priority, setPriority] = useState(existing?.priority ?? "Medium");
  const [assignee, setAssignee] = useState(existing?.assignee ?? null);
  const [attachments, setAttachments] = useState(existing?.attachments || []);

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

  const onSubmit = (data) => {
    const isAssignAllowed = role === "Admin" || role === "HOD";
    const selectedAssignee = isAssignAllowed ? assignee : (existing?.assignee ?? null);

    if (existing) {
      let newStatus = existing.status;
      if (isAssignAllowed) {
        if (existing.status === "Pending" && selectedAssignee) {
          newStatus = "Assigned";
        } else if (existing.status === "Assigned" && !selectedAssignee) {
          newStatus = "Pending";
        }
      }

      const updatedReq = {
        ...existing,
        title: data.title,
        description: data.description,
        serviceType,
        requestType,
        department,
        priority: priority,
        attachments,
        assignee: selectedAssignee,
        status: newStatus,
      };
      updateRequest(updatedReq, activeProfile.name, "Request edited");
      toast.success(`Request ${existing.no} updated successfully!`);
    } else {
      const reqId = String(requests.length + 1042 + Math.floor(Math.random() * 100));
      const reqNo = `SR-2026-${reqId}`;
      const isApprovalNeeded = ["Software Request", "Access Request", "Hardware Request"].includes(
        requestType,
      );

      const initialStatus = selectedAssignee ? "Assigned" : "Pending";

      const newReq = {
        id: reqId,
        no: reqNo,
        title: data.title,
        description: data.description,
        serviceType,
        requestType,
        department,
        requester: activeProfile.name,
        requesterEmail: activeProfile.email,
        assignee: selectedAssignee,
        status: initialStatus,
        priority: priority,
        created: new Date().toISOString(),
        updated: new Date().toISOString(),
        replies: [],
        attachments,
      };

      addRequest(newReq, activeProfile.name);

      if (isApprovalNeeded) {
        const newApproval = {
          id: String(Date.now()),
          requestId: reqId,
          requestNo: reqNo,
          title: data.title,
          requester: activeProfile.name,
          department,
          priority: priority,
          submitted: new Date().toISOString(),
          status: "Pending",
        };
        const currentApprovals = localStorage.getItem("servicedesk.approvals");
        const list = currentApprovals ? JSON.parse(currentApprovals) : [];
        list.unshift(newApproval);
        localStorage.setItem("servicedesk.approvals", JSON.stringify(list));
      }

      toast.success(`Request submitted successfully! Tracking no: ${reqNo}`);
    }
    navigate({ to: "/requests" });
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
              {serviceTypes.map((t) => (
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
              {requestTypes.map((t) => (
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
              {departments.map((d) => (
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
                {technicians.map((t) => (
                  <SelectItem key={t} value={t}>
                    {t}
                  </SelectItem>
                ))}
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
            <p className="text-xs text-muted-foreground">PNG, JPG, PDF up to 10 MB (UI only)</p>
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
        <Button type="submit" className="rounded-xl px-6">
          {existing ? "Save changes" : "Submit request"}
        </Button>
      </div>
    </form>
  );
}
