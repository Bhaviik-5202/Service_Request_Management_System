import { cn } from "@/lib/utils";

const statusStyles = {
  Pending: "bg-info/10 text-info ring-info/20",
  Assigned: "bg-primary/10 text-primary ring-primary/20",
  "In Progress": "bg-warning/15 text-warning-foreground ring-warning/30 dark:text-warning",
  Completed: "bg-success/10 text-success ring-success/20",
  Closed: "bg-muted text-muted-foreground ring-border",
  Cancelled: "bg-destructive/10 text-destructive ring-destructive/20",
};

export function StatusBadge({ status, className }) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 whitespace-nowrap rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset",
        statusStyles[status],
        className,
      )}
    >
      <span className="size-1.5 rounded-full bg-current" />
      {status}
    </span>
  );
}

const priorityStyles = {
  Critical: "bg-destructive/10 text-destructive ring-destructive/20",
  High: "bg-chart-5/10 text-chart-5 ring-chart-5/20",
  Medium: "bg-warning/15 text-warning-foreground ring-warning/30 dark:text-warning",
  Low: "bg-success/10 text-success ring-success/20",
};

export function PriorityBadge({ priority, className }) {
  return (
    <span
      className={cn(
        "inline-flex items-center whitespace-nowrap rounded-md px-2 py-0.5 text-xs font-semibold ring-1 ring-inset",
        priorityStyles[priority],
        className,
      )}
    >
      {priority}
    </span>
  );
}

const assetStyles = {
  "In Use": "bg-info/10 text-info ring-info/20",
  Available: "bg-success/10 text-success ring-success/20",
  "Under Repair": "bg-warning/15 text-warning-foreground ring-warning/30 dark:text-warning",
  Retired: "bg-muted text-muted-foreground ring-border",
};

export function AssetStatusBadge({ status, className }) {
  return (
    <span
      className={cn(
        "inline-flex items-center whitespace-nowrap rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset",
        assetStyles[status] ?? "bg-muted text-muted-foreground ring-border",
        className,
      )}
    >
      {status}
    </span>
  );
}
