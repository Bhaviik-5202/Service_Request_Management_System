import { useState, useEffect } from "react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { motion } from "motion/react";
import { Check, X, Clock, Search, Inbox } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { PriorityBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { approvals, updateApproval, syncLocalStorage } from "@/data/mock";
import { Can, useAuth, ROLE_PROFILES } from "@/lib/auth";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_shell/approvals")({
  head: () => ({
    meta: [
      { title: "Approvals — ServiceDesk" },
      {
        name: "description",
        content: "Review, approve or reject pending service requests and view approval history.",
      },
    ],
  }),
  component: ApprovalsPage,
});

function ApprovalsPage() {
  const { role } = useAuth();
  const [localApprovals, setLocalApprovals] = useState([]);
  const [dialog, setDialog] = useState(null);
  const [remarks, setRemarks] = useState("");
  const [search, setSearch] = useState("");
  const [priority, setPriority] = useState("all");

  useEffect(() => {
    syncLocalStorage();
    setLocalApprovals([...approvals]);
  }, []);

  const decide = () => {
    if (!dialog) return;
    const activeProfile = role ? ROLE_PROFILES[role] : { name: "Aarav Sharma" };

    const updatedApproval = {
      ...dialog.approval,
      status: dialog.action,
      decidedBy: activeProfile.name,
      decidedOn: new Date().toISOString(),
      remarks: remarks || undefined,
    };

    updateApproval(updatedApproval, activeProfile.name, remarks);
    toast.success(`${dialog.approval.requestNo} ${dialog.action.toLowerCase()}`);

    // Resync local list
    syncLocalStorage();
    setLocalApprovals([...approvals]);

    setDialog(null);
    setRemarks("");
  };

  const filteredApprovals = localApprovals.filter((a) => {
    const q = search.toLowerCase();
    const matchesSearch =
      !q ||
      a.title.toLowerCase().includes(q) ||
      a.requestNo.toLowerCase().includes(q) ||
      a.requester.toLowerCase().includes(q);
    const matchesPriority = priority === "all" || a.priority === priority;
    return matchesSearch && matchesPriority;
  });

  const pending = filteredApprovals.filter((a) => a.status === "Pending");
  const history = filteredApprovals.filter((a) => a.status !== "Pending");

  const totalPendingRaw = localApprovals.filter((a) => a.status === "Pending").length;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Approval Management"
        description={`${totalPendingRaw} requests awaiting your decision`}
        crumbs={[{ label: "Approvals" }]}
      />

      <Tabs defaultValue="pending" className="w-full">
        <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between bg-card/25 backdrop-blur-md p-3 rounded-2xl border border-border/40">
          <TabsList className="rounded-xl h-10 p-1 shrink-0 bg-muted/65 border max-w-fit">
            <TabsTrigger
              value="pending"
              className="rounded-lg h-8 cursor-pointer text-xs font-bold px-5"
            >
              Pending
              {totalPendingRaw > 0 && (
                <span className="ml-2 inline-flex items-center justify-center px-2 py-0.5 rounded-full bg-primary text-[10px] font-black text-primary-foreground">
                  {totalPendingRaw}
                </span>
              )}
            </TabsTrigger>
            <TabsTrigger
              value="history"
              className="rounded-lg h-8 cursor-pointer text-xs font-bold px-5"
            >
              History
            </TabsTrigger>
          </TabsList>

          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 flex-1 sm:justify-end">
            <div className="relative flex-1 max-w-md">
              <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search approvals by title, no, requester..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="h-9 rounded-xl pl-9 bg-background/50 border-border/50 focus-visible:ring-primary"
              />
            </div>
            <Select value={priority} onValueChange={setPriority}>
              <SelectTrigger className="h-9 rounded-xl w-full sm:w-36 shrink-0 bg-background/50 border-border/50">
                <SelectValue placeholder="Priority" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Priorities</SelectItem>
                <SelectItem value="Critical">Critical</SelectItem>
                <SelectItem value="High">High</SelectItem>
                <SelectItem value="Medium">Medium</SelectItem>
                <SelectItem value="Low">Low</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        <TabsContent value="pending" className="space-y-4 outline-none">
          {pending.length === 0 && (
            <div className="rounded-3xl border border-dashed border-border bg-card/30 backdrop-blur-md p-12 text-center shadow-sm max-w-md mx-auto mt-8 flex flex-col items-center">
              <div className="size-16 grid place-items-center rounded-2xl bg-success/15 text-success mb-5">
                <Inbox className="size-8 stroke-[1.5]" />
              </div>
              <h3 className="text-lg font-bold text-foreground">All Caught Up!</h3>
              <p className="text-xs text-muted-foreground mt-2 max-w-xs leading-relaxed font-medium">
                No pending approval requests are waiting on your decision. Outstanding items will
                appear here automatically.
              </p>
            </div>
          )}
          {pending.map((a, i) => {
            // Priority-based border classes for cards
            const borderColors = {
              Critical: "border-l-4 border-l-destructive border-t border-r border-b",
              High: "border-l-4 border-l-warning border-t border-r border-b",
              Medium: "border-l-4 border-l-primary border-t border-r border-b",
              Low: "border-l-4 border-l-slate-400 border-t border-r border-b",
            };
            const cardBorderClass = borderColors[a.priority] || "border";

            return (
              <motion.div
                key={a.id}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.05 }}
                className={cn(
                  "group rounded-2xl bg-card/45 backdrop-blur-md p-5 shadow-card hover:shadow-lg transition-all border-border/50 hover:border-primary/20",
                  cardBorderClass,
                )}
              >
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div className="space-y-2 min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <Link
                        to="/requests/$requestId"
                        params={{ requestId: a.requestId }}
                        className="text-base font-extrabold text-slate-800 dark:text-slate-100 hover:text-primary dark:hover:text-primary transition-colors truncate max-w-md sm:max-w-xl"
                      >
                        {a.title}
                      </Link>
                      <PriorityBadge priority={a.priority} />
                    </div>
                    <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-xs text-muted-foreground font-semibold">
                      <span className="font-bold text-primary bg-primary/5 dark:bg-primary/10 px-2 py-0.5 rounded-md border border-primary/10">
                        {a.requestNo}
                      </span>
                      <span className="text-muted-foreground/30">•</span>
                      <span>
                        Submitted by{" "}
                        <span className="text-foreground dark:text-slate-200">{a.requester}</span>
                      </span>
                      <span className="text-muted-foreground/30">•</span>
                      <span>{a.department}</span>
                      <span className="text-muted-foreground/30">•</span>
                      <span className="inline-flex items-center gap-1 font-medium bg-muted/40 px-2 py-0.5 rounded-md border border-border/20">
                        <Clock className="size-3 text-muted-foreground" />
                        {new Date(a.submitted).toLocaleDateString("en-IN", {
                          day: "numeric",
                          month: "short",
                          year: "numeric",
                        })}
                      </span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 self-stretch md:self-auto shrink-0 border-t pt-4 md:border-t-0 md:pt-0 border-border/30 w-full md:w-auto justify-end">
                    <Can
                      perm="approvals.decide"
                      fallback={
                        <span className="rounded-full bg-muted/65 px-3 py-1 text-[10px] font-bold uppercase tracking-wider text-muted-foreground border">
                          View only
                        </span>
                      }
                    >
                      <Button
                        size="sm"
                        className="rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white cursor-pointer font-extrabold px-4 h-9 shadow-sm shadow-emerald-600/10 transition-colors gap-1.5"
                        onClick={() => setDialog({ approval: a, action: "Approved" })}
                      >
                        <Check className="size-4 stroke-[2.5]" /> Approve
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="rounded-xl border-rose-200/50 text-rose-500 hover:bg-rose-500/10 dark:hover:bg-rose-950/20 cursor-pointer font-extrabold px-4 h-9 transition-colors gap-1.5"
                        onClick={() => setDialog({ approval: a, action: "Rejected" })}
                      >
                        <X className="size-4 stroke-[2.5]" /> Reject
                      </Button>
                    </Can>
                  </div>
                </div>
              </motion.div>
            );
          })}
        </TabsContent>

        <TabsContent value="history" className="outline-none">
          <div className="overflow-hidden rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md shadow-card">
            <div className="overflow-x-auto">
              <Table>
                <TableHeader className="bg-muted/30">
                  <TableRow className="hover:bg-transparent border-b">
                    <TableHead className="px-6 py-4 font-bold text-slate-800 dark:text-slate-200">
                      Request
                    </TableHead>
                    <TableHead className="hidden md:table-cell px-6 py-4 font-bold text-slate-800 dark:text-slate-200">
                      Requester
                    </TableHead>
                    <TableHead className="px-6 py-4 font-bold text-slate-800 dark:text-slate-200">
                      Decision
                    </TableHead>
                    <TableHead className="hidden lg:table-cell px-6 py-4 font-bold text-slate-800 dark:text-slate-200">
                      Decided by
                    </TableHead>
                    <TableHead className="hidden md:table-cell px-6 py-4 font-bold text-slate-800 dark:text-slate-200">
                      Remarks
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {history.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={5}
                        className="py-16 text-center text-sm text-muted-foreground"
                      >
                        <div className="flex flex-col items-center justify-center gap-2">
                          <Inbox className="size-10 stroke-[1.25] text-muted-foreground/60" />
                          <span className="font-bold text-foreground">No Decision History</span>
                          <span className="text-xs text-muted-foreground/80 max-w-xs leading-relaxed">
                            No matching decision history was found for this filter.
                          </span>
                        </div>
                      </TableCell>
                    </TableRow>
                  ) : (
                    history.map((a) => (
                      <TableRow
                        key={a.id}
                        className="group hover:bg-muted/20 border-b last:border-0 transition-colors"
                      >
                        <TableCell className="px-6 py-4 font-normal">
                          <Link
                            to="/requests/$requestId"
                            params={{ requestId: a.requestId }}
                            className="font-bold text-slate-800 dark:text-slate-200 hover:text-primary transition-colors text-sm"
                          >
                            {a.title}
                          </Link>
                          <p className="text-xs text-muted-foreground mt-1 font-semibold">
                            {a.requestNo}
                          </p>
                        </TableCell>
                        <TableCell className="hidden text-sm md:table-cell px-6 py-4 font-bold text-slate-700 dark:text-slate-300">
                          {a.requester}
                        </TableCell>
                        <TableCell className="px-6 py-4">
                          <span
                            className={cn(
                              "inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[10px] font-black ring-1 ring-inset uppercase tracking-wide",
                              a.status === "Approved"
                                ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 ring-emerald-500/20 border border-emerald-500/10"
                                : "bg-rose-500/10 text-rose-600 dark:text-rose-400 ring-rose-500/20 border border-rose-500/10",
                            )}
                          >
                            {a.status === "Approved" ? (
                              <Check className="size-3 stroke-[2.5]" />
                            ) : (
                              <X className="size-3 stroke-[2.5]" />
                            )}
                            {a.status}
                          </span>
                        </TableCell>
                        <TableCell className="hidden text-sm text-muted-foreground lg:table-cell px-6 py-4 font-semibold">
                          {a.decidedBy}
                        </TableCell>
                        <TableCell className="hidden max-w-64 truncate text-xs text-slate-600 dark:text-slate-400 md:table-cell px-6 py-4 italic font-medium">
                          {a.remarks ? (
                            `"${a.remarks}"`
                          ) : (
                            <span className="text-muted-foreground/30 font-semibold">—</span>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </div>
        </TabsContent>
      </Tabs>

      <Dialog open={!!dialog} onOpenChange={(o) => !o && setDialog(null)}>
        <DialogContent className="rounded-2xl max-w-md bg-card/90 backdrop-blur-xl border-border/50">
          <DialogHeader>
            <DialogTitle className="font-display text-lg font-bold text-foreground">
              {dialog?.action === "Approved" ? "Approve Request" : "Reject Request"}
            </DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground font-semibold mt-1">
              {dialog?.approval.requestNo} — {dialog?.approval.title}
            </DialogDescription>
          </DialogHeader>
          <div className="py-2.5">
            <Label className="text-xs font-bold text-foreground/80 mb-1.5 block">
              Decision Remarks
            </Label>
            <Textarea
              placeholder="Add your remarks here (optional)…"
              value={remarks}
              onChange={(e) => setRemarks(e.target.value)}
              rows={3}
              className="rounded-xl border-border/50 focus:border-primary text-sm font-normal bg-background/30"
            />
          </div>

          <DialogFooter className="gap-2 sm:gap-0 mt-2">
            <Button
              variant="outline"
              className="rounded-xl font-bold"
              onClick={() => setDialog(null)}
            >
              Cancel
            </Button>
            <Button
              className={cn(
                "rounded-xl font-extrabold cursor-pointer px-5",
                dialog?.action === "Approved"
                  ? "bg-emerald-600 text-white hover:bg-emerald-500 shadow-sm shadow-emerald-600/10"
                  : "bg-rose-600 text-white hover:bg-rose-500 shadow-sm shadow-rose-600/10",
              )}
              onClick={decide}
            >
              Confirm {dialog?.action === "Approved" ? "Approval" : "Rejection"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
