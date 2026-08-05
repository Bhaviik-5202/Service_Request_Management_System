import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
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
  Loader2,
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
import {
  getServiceRequestById,
  updateServiceRequest,
  deleteServiceRequest,
  createServiceRequestReply,
  getServiceRequestStatuses,
  getUsers,
} from "@/services/api";
import { useAuth } from "@/lib/auth";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/requests/$requestId/")({
  component: RequestDetail,
});

function RequestDetail() {
  const { requestId } = Route.useParams();
  const { role, user } = useAuth();
  const navigate = useNavigate();

  const [request, setRequest] = useState(null);
  const [techniciansList, setTechniciansList] = useState([]);
  const [statusesList, setStatusesList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [notFoundState, setNotFoundState] = useState(false);

  const [replyText, setReplyText] = useState("");
  const [replyAttachments, setReplyAttachments] = useState([]);
  const [isSendingReply, setIsSendingReply] = useState(false);

  const [modal, setModal] = useState(null);
  const [remarks, setRemarks] = useState("");
  const [modalAssignee, setModalAssignee] = useState(null);

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
      : { name: "System", email: "system@company.com", department: "IT", role: "Requestor" };
  }, [user, role]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [sr, apiUsers, apiStatuses] = await Promise.all([
        getServiceRequestById(requestId).catch(() => null),
        getUsers().catch(() => []),
        getServiceRequestStatuses().catch(() => []),
      ]);

      if (!sr) {
        setNotFoundState(true);
        return;
      }

      setStatusesList(apiStatuses || []);
      const techs = (apiUsers || []).filter((u) => u.roleName === "Technician" || u.role === "Technician" || u.roleId === 3);
      setTechniciansList(techs.length > 0 ? techs : apiUsers || []);

      const formattedReplies = (sr.replies || []).map((rep) => ({
        id: String(rep.replyId || rep.id),
        author: rep.authorUser ? (rep.authorUser.fullName || rep.authorUser.name) : (rep.author || "User"),
        role: rep.authorUser?.role?.roleName || rep.role || "User",
        message: rep.message,
        date: rep.createdAt ? new Date(rep.createdAt).toISOString() : new Date().toISOString(),
        status: rep.status,
      }));

      const formattedTimeline = (sr.timelines || []).map((tl) => ({
        id: String(tl.timelineId || tl.id),
        status: tl.status?.statusName || tl.status || "Pending",
        changedBy: tl.changedByUser ? (tl.changedByUser.fullName || tl.changedByUser.name) : (tl.changedBy || "System"),
        changedAt: tl.changedAt ? new Date(tl.changedAt).toISOString() : new Date().toISOString(),
        note: tl.note || "",
      }));

      const formattedAttachments = (sr.attachments || []).map((att) => ({
        id: String(att.attachmentId || att.id),
        name: att.fileName || att.name || "Attachment",
        size: att.fileSize ? `${Math.round(att.fileSize / 1024)} KB` : (att.size || "10 KB"),
        url: att.fileUrl || "#",
      }));

      setRequest({
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
        replies: formattedReplies,
        timeline: formattedTimeline,
        attachments: formattedAttachments,
        raw: sr,
      });
    } catch (err) {
      setNotFoundState(true);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [requestId]);

  const hasAccess = useMemo(() => {
    if (!request) return true;
    if (role === "Admin") return true;
    if (role === "Requestor") return activeProfile.email && request.requesterEmail.toLowerCase() === activeProfile.email.toLowerCase();
    if (role === "Technician") return request.assignee && request.assignee.toLowerCase() === activeProfile.name.toLowerCase();
    if (role === "HOD") return request.department === activeProfile.department;
    return true;
  }, [role, activeProfile, request]);

  if (loading) {
    return (
      <div className="py-20 text-center">
        <Loader2 className="size-8 mx-auto animate-spin text-primary" />
        <p className="mt-3 text-sm text-muted-foreground">Loading request details…</p>
      </div>
    );
  }

  if (notFoundState || !request) {
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

  const isRequestor = role === "Requestor";
  const isTechnician = role === "Technician";
  const isHOD = role === "HOD";
  const isAdmin = role === "Admin";

  const canAssign = isAdmin || isHOD || (isTechnician && request.department === activeProfile.department);
  const canUpdateStatus = isAdmin || isHOD || isTechnician;
  const canDelete = isAdmin;
  const canEdit = isAdmin || (isRequestor && request.requesterEmail.toLowerCase() === activeProfile.email.toLowerCase());

  const handleAssignChange = async (value) => {
    const assigneeName = value === "unassigned" ? null : value;
    const assigneeUserObj = techniciansList.find((t) => (t.fullName || t.name) === assigneeName);
    const assigneeUserId = assigneeUserObj ? assigneeUserObj.userId : null;

    let pendingStatusId = statusesList.find((s) => s.statusName === "Pending")?.statusId || 1;
    let assignedStatusId = statusesList.find((s) => s.statusName === "Assigned")?.statusId || 2;
    let newStatusId = (request.status === "Pending" && assigneeUserId) ? assignedStatusId : (request.raw?.statusId || 1);

    try {
      await updateServiceRequest(request.id, {
        ...request.raw,
        assigneeUserId: assigneeUserId,
        statusId: newStatusId,
        updatedByUserId: user?.userId || 1,
      });

      await createServiceRequestReply({
        requestId: Number(request.id),
        authorUserId: user?.userId || 1,
        message: assigneeName ? `Assigned ticket to technician: ${assigneeName}.` : "Removed technician assignment from ticket.",
      }).catch(() => null);

      toast.success(assigneeName ? `Assigned to ${assigneeName}` : "Removed technician assignment");
      await loadData();
    } catch (err) {
      toast.error(err.message || "Failed to update technician assignment.");
    }
  };

  const handleStatusChange = async (value) => {
    const targetStatus = statusesList.find((s) => s.statusName === value);
    const targetStatusId = targetStatus ? targetStatus.statusId : (request.raw?.statusId || 1);

    try {
      await updateServiceRequest(request.id, {
        ...request.raw,
        statusId: targetStatusId,
        updatedByUserId: user?.userId || 1,
      });

      await createServiceRequestReply({
        requestId: Number(request.id),
        authorUserId: user?.userId || 1,
        message: `Updated status to: ${value}.`,
      }).catch(() => null);

      toast.success(`Status updated to ${value}`);
      await loadData();
    } catch (err) {
      toast.error(err.message || "Failed to update status.");
    }
  };

  const handleStartWork = () => handleStatusChange("In Progress");
  const handleMarkCompleted = () => handleStatusChange("Completed");
  const handleReopen = () => handleStatusChange(request.assignee ? "Assigned" : "Pending");

  const handleAddReply = async () => {
    if (!replyText.trim()) {
      toast.error("Reply text cannot be empty.");
      return;
    }

    setIsSendingReply(true);
    try {
      await createServiceRequestReply({
        requestId: Number(request.id),
        authorUserId: user?.userId || 1,
        message: replyText,
      });

      toast.success("Reply added successfully!");
      setReplyText("");
      setReplyAttachments([]);
      await loadData();
    } catch (err) {
      toast.error(err.message || "Failed to post reply.");
    } finally {
      setIsSendingReply(false);
    }
  };

  const handleConfirmModal = async () => {
    if (!modal) return;

    try {
      if (modal.type === "approve") {
        const assigneeName = modalAssignee === "unassigned" ? null : modalAssignee;
        const assigneeUserObj = techniciansList.find((t) => (t.fullName || t.name) === assigneeName);
        const assigneeUserId = assigneeUserObj ? assigneeUserObj.userId : null;
        let assignedStatusId = statusesList.find((s) => s.statusName === "Assigned")?.statusId || 2;
        let pendingStatusId = statusesList.find((s) => s.statusName === "Pending")?.statusId || 1;

        await updateServiceRequest(request.id, {
          ...request.raw,
          assigneeUserId: assigneeUserId,
          statusId: assigneeName ? assignedStatusId : pendingStatusId,
          updatedByUserId: user?.userId || 1,
        });

        await createServiceRequestReply({
          requestId: Number(request.id),
          authorUserId: user?.userId || 1,
          message: `HOD Decision: Approved.\nRemarks: ${remarks || "No remarks provided."}${assigneeName ? `\nAssigned Technician: ${assigneeName}` : ""}`,
        }).catch(() => null);

        toast.success(assigneeName ? `Approved and assigned to ${assigneeName}` : "Request approved");
        await loadData();
      } else if (modal.type === "reject") {
        let cancelStatusId = statusesList.find((s) => s.statusName === "Cancelled")?.statusId || 6;
        await updateServiceRequest(request.id, {
          ...request.raw,
          statusId: cancelStatusId,
          updatedByUserId: user?.userId || 1,
        });

        await createServiceRequestReply({
          requestId: Number(request.id),
          authorUserId: user?.userId || 1,
          message: `HOD Decision: Rejected.\nRemarks: ${remarks || "No remarks provided."}`,
        }).catch(() => null);

        toast.success("Request rejected and cancelled");
        await loadData();
      } else if (modal.type === "close") {
        let closeStatusId = statusesList.find((s) => s.statusName === "Closed")?.statusId || 5;
        await updateServiceRequest(request.id, {
          ...request.raw,
          statusId: closeStatusId,
          updatedByUserId: user?.userId || 1,
        });
        toast.success("Request status set to Closed");
        await loadData();
      } else if (modal.type === "cancel") {
        let cancelStatusId = statusesList.find((s) => s.statusName === "Cancelled")?.statusId || 6;
        await updateServiceRequest(request.id, {
          ...request.raw,
          statusId: cancelStatusId,
          updatedByUserId: user?.userId || 1,
        });
        toast.success("Request status set to Cancelled");
        await loadData();
      } else if (modal.type === "delete") {
        await deleteServiceRequest(request.id);
        toast.success(`Request ${request.no} deleted successfully`);
        navigate({ to: "/requests" });
        return;
      }
    } catch (err) {
      toast.error(err.message || "Operation failed.");
    } finally {
      setModal(null);
      setRemarks("");
      setModalAssignee(null);
    }
  };

  return (
    <div>
      <PageHeader
        title={request.no}
        description={request.title}
        crumbs={[{ label: "Service Requests", to: "/requests" }, { label: request.no }]}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            {canEdit && (
              <Button asChild variant="outline" className="rounded-xl gap-1.5 cursor-pointer">
                <Link to="/requests/$requestId/edit" params={{ requestId: request.id }}>
                  <Pencil className="size-3.5" /> Edit
                </Link>
              </Button>
            )}

            {isHOD && request.status === "Pending" && (
              <>
                <Button onClick={() => setModal({ type: "approve" })} className="rounded-xl gap-1.5 bg-success text-success-foreground hover:bg-success/90 cursor-pointer">
                  <Check className="size-4" /> Approve
                </Button>
                <Button onClick={() => setModal({ type: "reject" })} variant="destructive" className="rounded-xl gap-1.5 cursor-pointer">
                  <X className="size-4" /> Reject
                </Button>
              </>
            )}

            {canUpdateStatus && (
              <>
                {(isTechnician || isAdmin) && (request.status === "Assigned" || request.status === "Pending") && (
                  <Button onClick={handleStartWork} className="rounded-xl cursor-pointer">
                    Start Work
                  </Button>
                )}

                {(isTechnician || isAdmin || isHOD) && request.status === "In Progress" && (
                  <Button onClick={handleMarkCompleted} className="rounded-xl bg-success text-success-foreground hover:bg-success/90 cursor-pointer">
                    Mark Completed
                  </Button>
                )}

                {request.status === "Completed" && (
                  <Button onClick={() => setModal({ type: "close" })} className="rounded-xl cursor-pointer">
                    Close Ticket
                  </Button>
                )}

                {(request.status === "Closed" || request.status === "Cancelled" || request.status === "Completed") && (
                  <Button onClick={handleReopen} variant="outline" className="rounded-xl cursor-pointer">
                    Reopen Ticket
                  </Button>
                )}

                {request.status !== "Closed" && request.status !== "Cancelled" && (
                  <Button onClick={() => setModal({ type: "cancel" })} variant="outline" className="rounded-xl cursor-pointer">
                    Cancel Ticket
                  </Button>
                )}
              </>
            )}

            {canDelete && (
              <Button onClick={() => setModal({ type: "delete" })} variant="destructive" size="icon" className="rounded-xl cursor-pointer" title="Delete request">
                <Trash2 className="size-4" />
              </Button>
            )}
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main conversation column */}
        <div className="space-y-6 lg:col-span-2">
          {/* Ticket Header & Description */}
          <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card">
            <div className="flex flex-wrap items-center justify-between gap-3 border-b pb-4">
              <div className="flex items-center gap-3">
                <StatusBadge status={request.status} />
                <PriorityBadge priority={request.priority} />
              </div>
              <p className="text-xs text-muted-foreground">
                Created {new Date(request.created).toLocaleString("en-IN", { dateStyle: "medium", timeStyle: "short" })}
              </p>
            </div>

            <h2 className="mt-4 font-display text-xl font-bold text-foreground">{request.title}</h2>
            <div className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-slate-700 dark:text-slate-300">
              {request.description}
            </div>

            {/* Attachments Section */}
            {request.attachments && request.attachments.length > 0 && (
              <div className="mt-6 border-t pt-4">
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">Attached Files ({request.attachments.length})</p>
                <div className="grid gap-2 sm:grid-cols-2">
                  {request.attachments.map((file) => (
                    <div key={file.id} className="flex items-center justify-between rounded-xl border bg-muted/40 p-2.5 text-sm">
                      <div className="flex items-center gap-2.5 min-w-0">
                        <Paperclip className="size-4 shrink-0 text-primary" />
                        <span className="truncate font-medium">{file.name}</span>
                        <span className="text-xs text-muted-foreground">({file.size})</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </motion.div>

          {/* Conversation / Replies */}
          <div className="space-y-4">
            <h3 className="font-display text-base font-bold flex items-center gap-2">
              <MessageSquare className="size-4 text-primary" /> Activity &amp; Replies ({request.replies.length})
            </h3>

            {request.replies.map((r) => (
              <motion.div key={r.id} initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Avatar className="size-8">
                      <AvatarFallback className="bg-primary/10 text-xs font-bold text-primary">
                        {r.author.split(" ").map((w) => w[0]).join("")}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="text-sm font-semibold">{r.author} <span className="text-xs font-normal text-muted-foreground">({r.role})</span></p>
                      <p className="text-[11px] text-muted-foreground">{new Date(r.date).toLocaleString("en-IN", { dateStyle: "medium", timeStyle: "short" })}</p>
                    </div>
                  </div>
                  {r.status && <StatusBadge status={r.status} />}
                </div>
                <p className="mt-3 text-sm whitespace-pre-wrap text-slate-700 dark:text-slate-300">{r.message}</p>
              </motion.div>
            ))}

            {/* Add Reply Input */}
            {request.status !== "Closed" && request.status !== "Cancelled" && (
              <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-4 shadow-card space-y-3">
                <Label htmlFor="reply" className="text-sm font-semibold">Post a Reply</Label>
                <Textarea
                  id="reply"
                  placeholder="Type your response or update..."
                  value={replyText}
                  onChange={(e) => setReplyText(e.target.value)}
                  rows={3}
                  className="rounded-xl bg-background/80"
                />
                <div className="flex items-center justify-between pt-1">
                  <div />
                  <Button onClick={handleAddReply} disabled={isSendingReply || !replyText.trim()} className="rounded-xl gap-2 cursor-pointer">
                    {isSendingReply ? <Loader2 className="size-4 animate-spin" /> : <Send className="size-4" />}
                    Send Reply
                  </Button>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Sidebar Info Column */}
        <div className="space-y-6">
          <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card space-y-4">
            <h3 className="font-display text-base font-bold">Ticket Details</h3>

            <div className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground flex items-center gap-2"><User className="size-4 text-primary" /> Requester</span>
                <span className="font-semibold">{request.requester}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground flex items-center gap-2"><Building2 className="size-4 text-primary" /> Department</span>
                <span className="font-semibold">{request.department}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground flex items-center gap-2"><Tag className="size-4 text-primary" /> Service Type</span>
                <span className="font-semibold">{request.serviceType}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground flex items-center gap-2"><Tag className="size-4 text-primary" /> Request Type</span>
                <span className="font-semibold">{request.requestType}</span>
              </div>
            </div>

            <Separator />

            {/* Assignee Manager Dropdown */}
            <div className="space-y-2">
              <Label className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Assigned Technician</Label>
              {canAssign ? (
                <Select value={request.assignee ?? "unassigned"} onValueChange={handleAssignChange}>
                  <SelectTrigger className="h-10 rounded-xl bg-background/80">
                    <SelectValue placeholder="Unassigned" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="unassigned">Unassigned</SelectItem>
                    {techniciansList.map((t) => {
                      const tName = t.fullName || t.name;
                      return (
                        <SelectItem key={t.userId || tName} value={tName}>
                          {tName}
                        </SelectItem>
                      );
                    })}
                  </SelectContent>
                </Select>
              ) : (
                <div className="rounded-xl border bg-muted/40 p-2.5 font-medium text-sm">
                  {request.assignee || "Unassigned"}
                </div>
              )}
            </div>

            {/* Status Manager Dropdown */}
            {canUpdateStatus && (
              <div className="space-y-2 pt-2">
                <Label className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Update Status</Label>
                <Select value={request.status} onValueChange={handleStatusChange}>
                  <SelectTrigger className="h-10 rounded-xl bg-background/80">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {(statusesList.length > 0 ? statusesList.map((s) => s.statusName) : ["Pending", "Assigned", "In Progress", "Completed", "Closed", "Cancelled"]).map((s) => (
                      <SelectItem key={s} value={s}>
                        {s}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}
          </motion.div>
        </div>
      </div>

      {/* Decision / Action Dialog */}
      <Dialog open={!!modal} onOpenChange={(open) => !open && setModal(null)}>
        <DialogContent className="rounded-2xl max-w-md">
          <DialogHeader>
            <DialogTitle>
              {modal?.type === "approve" && "Approve Service Request"}
              {modal?.type === "reject" && "Reject Service Request"}
              {modal?.type === "close" && "Close Ticket"}
              {modal?.type === "cancel" && "Cancel Ticket"}
              {modal?.type === "delete" && "Delete Ticket"}
            </DialogTitle>
            <DialogDescription>
              {modal?.type === "approve" && "Provide remarks and optionally assign a technician."}
              {modal?.type === "reject" && "Provide the reason for rejecting this request."}
              {modal?.type === "close" && "Are you sure you want to mark this request as closed?"}
              {modal?.type === "cancel" && "Are you sure you want to cancel this request?"}
              {modal?.type === "delete" && "This action cannot be undone. Are you sure you want to delete this ticket permanently?"}
            </DialogDescription>
          </DialogHeader>

          {modal?.type === "approve" && (
            <div className="space-y-4 py-2">
              <div className="space-y-1.5">
                <Label>Assign Technician (Optional)</Label>
                <Select value={modalAssignee ?? "unassigned"} onValueChange={setModalAssignee}>
                  <SelectTrigger className="h-10 rounded-xl">
                    <SelectValue placeholder="Unassigned" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="unassigned">Unassigned</SelectItem>
                    {techniciansList.map((t) => {
                      const tName = t.fullName || t.name;
                      return (
                        <SelectItem key={t.userId || tName} value={tName}>
                          {tName}
                        </SelectItem>
                      );
                    })}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="modal-remarks">Remarks</Label>
                <Textarea id="modal-remarks" placeholder="Approval notes..." value={remarks} onChange={(e) => setRemarks(e.target.value)} rows={3} className="rounded-xl" />
              </div>
            </div>
          )}

          {modal?.type === "reject" && (
            <div className="space-y-1.5 py-2">
              <Label htmlFor="modal-remarks">Rejection Reason *</Label>
              <Textarea id="modal-remarks" placeholder="State reason for rejection..." value={remarks} onChange={(e) => setRemarks(e.target.value)} rows={3} className="rounded-xl" />
            </div>
          )}

          <DialogFooter className="gap-2 sm:gap-0">
            <Button variant="outline" onClick={() => setModal(null)} className="rounded-xl">Cancel</Button>
            <Button onClick={handleConfirmModal} variant={modal?.type === "reject" || modal?.type === "delete" ? "destructive" : "default"} className="rounded-xl">Confirm</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
