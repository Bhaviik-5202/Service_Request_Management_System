import { createFileRoute, Link, notFound, useNavigate, useRouter } from "@tanstack/react-router";
import { motion } from "motion/react";
import {
  CalendarDays,
  MessageSquare,
  Pencil,
  Send,
  User,
  Building2,
  Tag,
  Trash2,
  Check,
  X,
  Paperclip,
} from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatusBadge, PriorityBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { requests, updateRequest, deleteRequest, technicians, syncLocalStorage } from "@/data/mock";
import { useAuth, ROLE_PROFILES } from "@/lib/auth";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/requests/$requestId/")({
  loader: ({ params }) => {
    // Sync first to get fresh data if page is reloaded
    syncLocalStorage();
    const request = requests.find((r) => r.id === params.requestId);
    if (!request) throw notFound();
    return { request };
  },
  head: ({ loaderData }) => ({
    meta: [
      { title: loaderData ? `${loaderData.request.no} — ServiceDesk` : "Request — ServiceDesk" },
      { name: "description", content: loaderData?.request.title ?? "Service request detail" },
      ...(loaderData ? [] : [{ name: "robots", content: "noindex" }]),
    ],
  }),
  notFoundComponent: RequestNotFound,
  errorComponent: RequestError,
  component: RequestDetail,
});

function RequestNotFound() {
  return (
    <div className="py-20 text-center">
      <h1 className="text-xl font-bold">Request not found</h1>
      <p className="mt-2 text-sm text-muted-foreground">This request may have been removed.</p>
      <Button asChild className="mt-4 rounded-xl">
        <Link to="/requests">Back to requests</Link>
      </Button>
    </div>
  );
}

function RequestError() {
  return (
    <div className="py-20 text-center">
      <h1 className="text-xl font-bold">Something went wrong</h1>
      <Button asChild className="mt-4 rounded-xl">
        <Link to="/requests">Back to requests</Link>
      </Button>
    </div>
  );
}

function RequestDetail() {
  const { request } = Route.useLoaderData();
  const { role, user } = useAuth();
  const router = useRouter();
  const navigate = useNavigate();

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
      : { name: "System", email: "system@company.com", department: "IT", role: "Requestor" };
  }, [user, role]);

  const hasAccess = useMemo(() => {
    if (role === "Admin") return true;
    if (role === "Requestor") return request.requesterEmail === activeProfile.email;
    if (role === "Technician") return request.assignee === activeProfile.name;
    if (role === "HOD") return request.department === activeProfile.department;
    return false;
  }, [role, activeProfile, request]);

  useEffect(() => {
    if (!hasAccess) {
      navigate({ to: "/unauthorized", replace: true });
    }
  }, [hasAccess, navigate]);

  const [modal, setModal] = useState(null);
  const [remarks, setRemarks] = useState("");
  const [modalAssignee, setModalAssignee] = useState(null);
  const [replyAttachments, setReplyAttachments] = useState([]);

  if (!hasAccess) {
    return <div className="text-center py-20 text-muted-foreground">Redirecting...</div>;
  }

  const isRequestor = role === "Requestor";
  const isTechnician = role === "Technician";
  const isHOD = role === "HOD";
  const isAdmin = role === "Admin";

  const canAssign =
    isAdmin || isHOD || (isTechnician && request.department === activeProfile.department);
  const canUpdateStatus = isAdmin || isHOD || isTechnician;
  const canDelete = isAdmin;
  const canEdit = isAdmin || (isRequestor && request.requesterEmail === activeProfile.email);

  const handleAssignChange = (value) => {
    const assigneeName = value === "unassigned" ? null : value;
    const newStatus = request.status === "Pending" && assigneeName ? "Assigned" : request.status;
    const updatedReq = {
      ...request,
      assignee: assigneeName,
      status: newStatus,
      replies: [
        ...request.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: role || "System",
          message: assigneeName
            ? `Assigned ticket to technician: ${assigneeName}.`
            : "Removed technician assignment from ticket.",
          date: new Date().toISOString(),
        },
      ],
    };
    updateRequest(
      updatedReq,
      activeProfile.name,
      assigneeName ? `Technician assigned: ${assigneeName}` : "Technician unassigned",
    );
    toast.success(assigneeName ? `Assigned to ${assigneeName}` : "Removed technician assignment");
    router.invalidate();
  };

  const handleStatusChange = (value) => {
    const updatedReq = {
      ...request,
      status: value,
      replies: [
        ...request.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: role || "System",
          message: `Updated status to: ${value}.`,
          date: new Date().toISOString(),
          status: value,
        },
      ],
    };
    updateRequest(updatedReq, activeProfile.name, `Status changed to ${value}`);
    toast.success(`Status updated to ${value}`);
    router.invalidate();
  };

  const handleApprove = () => {
    setModal({ type: "approve" });
  };

  const handleReject = () => {
    setModal({ type: "reject" });
  };

  const handleClose = () => {
    setModal({ type: "close" });
  };

  const handleCancel = () => {
    setModal({ type: "cancel" });
  };

  const handleDelete = () => {
    setModal({ type: "delete" });
  };

  const handleStartWork = () => {
    const updatedReq = {
      ...request,
      status: "In Progress",
      replies: [
        ...request.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: role || "System",
          message: "Started working on this request.",
          date: new Date().toISOString(),
          status: "In Progress",
        },
      ],
    };
    updateRequest(updatedReq, activeProfile.name, "Started work");
    toast.success("Work started - Status set to In Progress");
    router.invalidate();
  };

  const handleMarkCompleted = () => {
    const updatedReq = {
      ...request,
      status: "Completed",
      replies: [
        ...request.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: role || "System",
          message: "Marked this request as Completed.",
          date: new Date().toISOString(),
          status: "Completed",
        },
      ],
    };
    updateRequest(updatedReq, activeProfile.name, "Marked completed");
    toast.success("Status set to Completed");
    router.invalidate();
  };

  const handleReopen = () => {
    const nextStatus = request.assignee ? "Assigned" : "Pending";
    const updatedReq = {
      ...request,
      status: nextStatus,
      replies: [
        ...request.replies,
        {
          id: Date.now(),
          author: activeProfile.name,
          role: role || "System",
          message: "Reopened the request.",
          date: new Date().toISOString(),
          status: nextStatus,
        },
      ],
    };
    updateRequest(updatedReq, activeProfile.name, "Reopened request");
    toast.success("Request reopened");
    router.invalidate();
  };

  const handleConfirmModal = () => {
    if (!modal) return;

    if (modal.type === "approve") {
      const assigneeName = modalAssignee === "unassigned" ? null : modalAssignee;
      const newStatus = assigneeName ? "Assigned" : "Pending";
      const updatedReq = {
        ...request,
        status: newStatus,
        assignee: assigneeName,
        replies: [
          ...request.replies,
          {
            id: Date.now(),
            author: activeProfile.name,
            role: role || "HOD",
            message: `HOD Decision: Approved.\nRemarks: ${remarks || "No remarks provided."}${assigneeName ? `\nAssigned Technician: ${assigneeName}` : ""}`,
            date: new Date().toISOString(),
          },
        ],
      };
      updateRequest(
        updatedReq,
        activeProfile.name,
        assigneeName ? `Approved and assigned to ${assigneeName}` : "Approved request",
      );
      toast.success(assigneeName ? `Approved and assigned to ${assigneeName}` : "Request approved");
      router.invalidate();
    } else if (modal.type === "reject") {
      const updatedReq = {
        ...request,
        status: "Cancelled",
        replies: [
          ...request.replies,
          {
            id: Date.now(),
            author: activeProfile.name,
            role: role || "HOD",
            message: `HOD Decision: Rejected.\nRemarks: ${remarks || "No remarks provided."}`,
            date: new Date().toISOString(),
          },
        ],
      };
      updateRequest(
        updatedReq,
        activeProfile.name,
        `Rejected request. Remarks: ${remarks || "None"}`,
      );
      toast.success("Request rejected and cancelled");
      router.invalidate();
    } else if (modal.type === "close") {
      const updatedReq = {
        ...request,
        status: "Closed",
      };
      updateRequest(updatedReq, activeProfile.name, "Request closed successfully");
      toast.success("Request status set to Closed");
      router.invalidate();
    } else if (modal.type === "cancel") {
      const updatedReq = {
        ...request,
        status: "Cancelled",
      };
      updateRequest(updatedReq, activeProfile.name, "Request cancelled");
      toast.success("Request status set to Cancelled");
      router.invalidate();
    } else if (modal.type === "delete") {
      deleteRequest(request.id);
      toast.success(`Request ${request.no} deleted successfully`);
      navigate({ to: "/requests" });
    }

    setModal(null);
    setRemarks("");
    setModalAssignee(null);
  };

  const handlePostReply = (e) => {
    e.preventDefault();
    const form = e.currentTarget;
    const replyText = form.elements.namedItem("replyMessage").value.trim();

    if (!replyText) return;

    const newReply = {
      id: Date.now(),
      author: activeProfile.name,
      role: role || "System",
      message: replyText,
      date: new Date().toISOString(),
      attachments: replyAttachments.length > 0 ? replyAttachments : undefined,
    };

    const updatedReq = {
      ...request,
      replies: [...request.replies, newReply],
    };

    updateRequest(updatedReq, activeProfile.name, "Reply posted");
    toast.success("Reply posted successfully");
    form.reset();
    setReplyAttachments([]);
    router.invalidate();
  };

  const meta = [
    { icon: User, label: "Requester", value: request.requester },
    { icon: Building2, label: "Department", value: request.department },
    { icon: Tag, label: "Type", value: `${request.serviceType} · ${request.requestType}` },
    {
      icon: CalendarDays,
      label: "Created",
      value: new Date(request.created).toLocaleString("en-IN", {
        dateStyle: "medium",
        timeStyle: "short",
      }),
    },
    {
      icon: CalendarDays,
      label: "Last updated",
      value: new Date(request.updated).toLocaleString("en-IN", {
        dateStyle: "medium",
        timeStyle: "short",
      }),
    },
  ];

  return (
    <div>
      <PageHeader
        title={request.title}
        description={request.no}
        crumbs={[{ label: "Service Requests", to: "/requests" }, { label: request.no }]}
        actions={
          <div className="flex items-center gap-2">
            {canEdit && (
              <Button asChild variant="outline" className="rounded-xl">
                <Link to="/requests/$requestId/edit" params={{ requestId: request.id }}>
                  <Pencil className="mr-1.5 size-4" /> Edit
                </Link>
              </Button>
            )}
            {canDelete && (
              <Button variant="destructive" className="rounded-xl" onClick={handleDelete}>
                <Trash2 className="mr-1.5 size-4" /> Delete
              </Button>
            )}
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <div className="mb-4 flex flex-wrap items-center gap-2">
              <StatusBadge status={request.status} />
              <PriorityBadge priority={request.priority} />
            </div>
            <h3 className="text-sm font-bold uppercase tracking-wide text-muted-foreground">
              Description
            </h3>
            <p className="mt-2 text-sm leading-relaxed whitespace-pre-wrap">
              {request.description}
            </p>
          </motion.div>

          {request.attachments && request.attachments.length > 0 && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.05 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="flex items-center gap-2 font-display text-base font-bold mb-3">
                <Paperclip className="size-4 text-primary" /> Attachments (
                {request.attachments.length})
              </h3>
              <div className="grid gap-3 sm:grid-cols-2">
                {request.attachments.map((file) => (
                  <div
                    key={file.id}
                    className="flex items-center justify-between rounded-xl border bg-accent/20 p-3 text-sm"
                  >
                    <div className="flex items-center gap-2.5 min-w-0">
                      <Paperclip className="size-4 shrink-0 text-muted-foreground" />
                      <div className="min-w-0">
                        <p className="truncate font-semibold leading-none">{file.name}</p>
                        <span className="text-[10px] text-muted-foreground mt-1 block">
                          {file.size}
                        </span>
                      </div>
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="rounded-lg h-8 cursor-pointer text-xs"
                      onClick={() => alert(`Mock Download: Opening ${file.name}`)}
                    >
                      Open
                    </Button>
                  </div>
                ))}
              </div>
            </motion.div>
          )}

          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <h3 className="flex items-center gap-2 font-display text-base font-bold">
              <MessageSquare className="size-4 text-primary" /> Communication & History (
              {request.replies.length})
            </h3>
            <div className="mt-4 space-y-4">
              {request.replies.length === 0 && (
                <p className="rounded-xl bg-muted p-4 text-center text-sm text-muted-foreground">
                  No replies yet. Updates from the assigned technician will appear here.
                </p>
              )}
              {request.replies.map((reply) => (
                <div key={reply.id} className="flex gap-3">
                  <Avatar className="size-9 shrink-0">
                    <AvatarFallback className="bg-primary/10 text-xs font-bold text-primary">
                      {reply.author
                        .split(" ")
                        .map((w) => w[0])
                        .join("")}
                    </AvatarFallback>
                  </Avatar>
                  <div className="min-w-0 flex-1 rounded-xl bg-muted/60 p-3">
                    <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
                      <p className="text-sm font-semibold">{reply.author}</p>
                      <span className="rounded-md bg-accent px-1.5 py-0.5 text-[10px] font-semibold text-accent-foreground">
                        {reply.role}
                      </span>
                      {reply.status && (
                        <StatusBadge status={reply.status} className="text-[10px]" />
                      )}
                      <span className="ml-auto text-[11px] text-muted-foreground">
                        {new Date(reply.date).toLocaleString("en-IN", {
                          dateStyle: "medium",
                          timeStyle: "short",
                        })}
                      </span>
                    </div>
                    <p className="mt-1.5 text-sm leading-relaxed whitespace-pre-wrap">
                      {reply.message}
                    </p>
                    {reply.attachments && reply.attachments.length > 0 && (
                      <div className="mt-2.5 flex flex-wrap gap-1.5">
                        {reply.attachments.map((file) => (
                          <div
                            key={file.id}
                            className="flex items-center gap-1.5 rounded-lg border bg-background/50 px-2 py-1 text-xs"
                          >
                            <Paperclip className="size-3 text-muted-foreground shrink-0" />
                            <span className="truncate max-w-[150px] font-medium leading-none">
                              {file.name}
                            </span>
                            <span className="text-[10px] text-muted-foreground leading-none">
                              ({file.size})
                            </span>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>

            <Separator className="my-4" />

            <form onSubmit={handlePostReply} className="space-y-3">
              <Textarea
                name="replyMessage"
                placeholder="Write a message, reply, or detail on resolution…"
                rows={3}
                className="rounded-xl"
                required
              />

              {replyAttachments.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {replyAttachments.map((file) => (
                    <div
                      key={file.id}
                      className="flex items-center gap-1.5 rounded-lg border bg-accent/40 px-2.5 py-1 text-xs"
                    >
                      <Paperclip className="size-3 text-muted-foreground shrink-0" />
                      <span className="truncate max-w-[150px] font-medium leading-none">
                        {file.name}
                      </span>
                      <button
                        type="button"
                        className="text-muted-foreground hover:text-destructive text-[11px] font-bold ml-1 cursor-pointer leading-none"
                        onClick={() =>
                          setReplyAttachments(replyAttachments.filter((f) => f.id !== file.id))
                        }
                      >
                        ×
                      </button>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex justify-between items-center">
                <div>
                  <label className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground cursor-pointer px-2.5 py-1.5 rounded-lg border bg-accent/20 hover:bg-accent/50 transition-colors">
                    <Paperclip className="size-3.5" />
                    <span>Attach Files</span>
                    <input
                      type="file"
                      multiple
                      className="hidden"
                      onChange={(e) => {
                        if (e.target.files && e.target.files.length > 0) {
                          const fileList = Array.from(e.target.files);
                          const newAttachments = fileList.map((file) => ({
                            id: String(Date.now() + Math.random()),
                            name: file.name,
                            size: `${Math.round(file.size / 1024)} KB`,
                            url: "#",
                          }));
                          setReplyAttachments([...replyAttachments, ...newAttachments]);
                          toast.success("File attached to reply");
                        }
                      }}
                    />
                  </label>
                </div>
                <Button type="submit" size="sm" className="rounded-xl">
                  <Send className="mr-1.5 size-3.5" /> Send Message
                </Button>
              </div>
            </form>
          </motion.div>
        </div>

        <div className="space-y-4">
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <h3 className="font-display text-base font-bold">Status & Assignment</h3>
            <Separator className="my-3" />
            <div className="space-y-4">
              {/* Assigned Technician selector */}
              <div className="space-y-1.5">
                <p className="text-xs text-muted-foreground font-medium">Assigned Technician</p>
                {canAssign ? (
                  <Select
                    value={request.assignee ?? "unassigned"}
                    onValueChange={handleAssignChange}
                  >
                    <SelectTrigger className="w-full h-10 rounded-xl">
                      <SelectValue placeholder="Select Technician" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="unassigned">Unassigned</SelectItem>
                      {technicians.map((tech) => (
                        <SelectItem key={tech} value={tech}>
                          {tech}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                ) : (
                  <p className="text-sm font-semibold">{request.assignee ?? "Unassigned"}</p>
                )}
              </div>

              {/* Status Selector */}
              <div className="space-y-1.5">
                <p className="text-xs text-muted-foreground font-medium">Request Status</p>
                {canUpdateStatus ? (
                  <Select value={request.status} onValueChange={(val) => handleStatusChange(val)}>
                    <SelectTrigger className="w-full h-10 rounded-xl">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {[
                        "Pending",
                        "Assigned",
                        "In Progress",
                        "Completed",
                        "Closed",
                        "Cancelled",
                      ].map((st) => (
                        <SelectItem key={st} value={st}>
                          {st}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                ) : (
                  <div className="flex items-center">
                    <StatusBadge status={request.status} />
                  </div>
                )}
              </div>

              {/* Workflow Actions */}
              <div className="space-y-2 pt-3 border-t">
                <p className="text-xs text-muted-foreground font-medium">Available Actions</p>

                {/* HOD Approvals banner */}
                {request.status === "Pending" && (isAdmin || isHOD) && (
                  <div className="flex gap-2">
                    <Button
                      type="button"
                      className="flex-1 rounded-xl bg-success text-success-foreground hover:bg-success/90 cursor-pointer text-xs font-semibold h-9"
                      size="sm"
                      onClick={handleApprove}
                    >
                      <Check className="mr-1 size-3.5" /> Approve
                    </Button>
                    <Button
                      type="button"
                      variant="destructive"
                      className="flex-1 rounded-xl cursor-pointer text-xs font-semibold h-9"
                      size="sm"
                      onClick={handleReject}
                    >
                      <X className="mr-1 size-3.5" /> Reject
                    </Button>
                  </div>
                )}

                {/* Start Work */}
                {request.status === "Assigned" && (isAdmin || isHOD || isTechnician) && (
                  <Button
                    type="button"
                    className="w-full h-9 rounded-xl cursor-pointer text-xs font-semibold"
                    onClick={handleStartWork}
                  >
                    Start Work
                  </Button>
                )}

                {/* Mark as Completed */}
                {request.status === "In Progress" && (isAdmin || isHOD || isTechnician) && (
                  <Button
                    type="button"
                    className="w-full h-9 rounded-xl bg-success text-success-foreground hover:bg-success/90 cursor-pointer text-xs font-semibold"
                    onClick={handleMarkCompleted}
                  >
                    Mark as Completed
                  </Button>
                )}

                {/* Reopen Request */}
                {request.status === "Completed" && (isAdmin || isHOD || isRequestor) && (
                  <Button
                    type="button"
                    variant="outline"
                    className="w-full h-9 rounded-xl cursor-pointer text-xs font-semibold"
                    onClick={handleReopen}
                  >
                    Reopen Request
                  </Button>
                )}

                {/* Close Request */}
                {["Assigned", "In Progress", "Completed"].includes(request.status) &&
                  (isAdmin || isHOD || isRequestor) && (
                    <Button
                      type="button"
                      variant="outline"
                      className="w-full h-9 rounded-xl border-success text-success hover:bg-success/5 cursor-pointer text-xs font-semibold"
                      onClick={handleClose}
                    >
                      Close Request
                    </Button>
                  )}

                {/* Cancel Request */}
                {["Pending", "Assigned"].includes(request.status) &&
                  (isAdmin || isHOD || isRequestor) && (
                    <Button
                      type="button"
                      variant="outline"
                      className="w-full h-9 rounded-xl border-destructive text-destructive hover:bg-destructive/5 cursor-pointer text-xs font-semibold"
                      onClick={handleCancel}
                    >
                      Cancel Request
                    </Button>
                  )}

                {/* Finished State Banners */}
                {request.status === "Closed" && (
                  <p className="text-xs text-muted-foreground text-center bg-muted py-2 rounded-lg font-medium">
                    This request is closed and completed.
                  </p>
                )}
                {request.status === "Cancelled" && (
                  <p className="text-xs text-muted-foreground text-center bg-muted py-2 rounded-lg font-medium">
                    This request is cancelled.
                  </p>
                )}
              </div>
            </div>
          </motion.div>

          {/* Timeline History Card */}
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.18 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <h3 className="font-display text-base font-bold">Request Timeline</h3>
            <Separator className="my-3" />
            <div className="relative border-l pl-4 ml-1.5 space-y-4 py-1">
              {(!request.timeline || request.timeline.length === 0) && (
                <p className="text-xs text-muted-foreground">No timeline history recorded.</p>
              )}
              {request.timeline?.map((event) => (
                <div key={event.id} className="relative text-xs">
                  {/* Bullet dot */}
                  <div className="absolute -left-[21.5px] top-1 grid size-3 place-items-center rounded-full bg-background border border-border">
                    <span className="size-1.5 rounded-full bg-primary" />
                  </div>
                  <div>
                    <div className="flex items-center justify-between">
                      <span className="font-semibold text-foreground leading-tight">
                        {event.note}
                      </span>
                    </div>
                    <div className="flex items-center gap-1.5 mt-1 text-[10px] text-muted-foreground">
                      <span>By {event.changedBy}</span>
                      <span>·</span>
                      <span>
                        {new Date(event.changedAt).toLocaleString("en-IN", {
                          dateStyle: "short",
                          timeStyle: "short",
                        })}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="h-fit rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
          >
            <h3 className="font-display text-base font-bold">Metadata Details</h3>
            <Separator className="my-3" />
            <div className="space-y-4">
              {meta.map((m) => (
                <div key={m.label} className="flex items-start gap-3">
                  <div className="grid size-8 shrink-0 place-items-center rounded-lg bg-accent text-accent-foreground">
                    <m.icon className="size-4" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-xs text-muted-foreground font-medium">{m.label}</p>
                    <p className="truncate text-sm font-semibold">{m.value}</p>
                  </div>
                </div>
              ))}
            </div>
          </motion.div>
        </div>
      </div>
      {/* Dialog Modals */}
      <Dialog open={!!modal} onOpenChange={(open) => !open && setModal(null)}>
        <DialogContent className="rounded-2xl max-w-md">
          <DialogHeader>
            <DialogTitle className="font-display">
              {modal?.type === "approve" && "Approve Request"}
              {modal?.type === "reject" && "Reject Request"}
              {modal?.type === "close" && "Close Request"}
              {modal?.type === "cancel" && "Cancel Request"}
              {modal?.type === "delete" && "Delete Request"}
            </DialogTitle>
            <DialogDescription>
              {modal?.type === "approve" &&
                "Confirm approval. You can optionally assign a technician and write remarks below."}
              {modal?.type === "reject" &&
                "Rejecting this request will mark it as Cancelled. Please provide remarks below."}
              {modal?.type === "close" &&
                "Are you sure you want to close this request? This confirms the ticket as resolved."}
              {modal?.type === "cancel" &&
                "Are you sure you want to cancel this request? This action stops further work."}
              {modal?.type === "delete" &&
                "Are you sure you want to delete this request? This action is permanent and cannot be undone."}
            </DialogDescription>
          </DialogHeader>

          {/* Render extra inputs for approve/reject */}
          {(modal?.type === "approve" || modal?.type === "reject") && (
            <div className="space-y-4 py-2 font-medium">
              {modal?.type === "approve" && (
                <div className="space-y-1.5">
                  <Label>Assign Technician (Optional)</Label>
                  <Select
                    value={modalAssignee ?? "unassigned"}
                    onValueChange={(val) => setModalAssignee(val)}
                  >
                    <SelectTrigger className="w-full h-10 rounded-xl">
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
                  className="rounded-xl font-normal"
                />
              </div>
            </div>
          )}

          <DialogFooter className="gap-2 sm:gap-0 mt-3">
            <Button
              type="button"
              variant="outline"
              className="rounded-xl"
              onClick={() => {
                setModal(null);
                setRemarks("");
                setModalAssignee(null);
              }}
            >
              Cancel
            </Button>
            <Button
              type="button"
              variant={
                modal?.type === "delete" || modal?.type === "reject" ? "destructive" : "default"
              }
              className="rounded-xl"
              onClick={handleConfirmModal}
            >
              {modal?.type === "approve" && "Approve"}
              {modal?.type === "reject" && "Reject"}
              {modal?.type === "close" && "Close Ticket"}
              {modal?.type === "cancel" && "Cancel Ticket"}
              {modal?.type === "delete" && "Delete Permanently"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
