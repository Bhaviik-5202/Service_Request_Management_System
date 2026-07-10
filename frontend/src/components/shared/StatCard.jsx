import { motion } from "motion/react";
import { useEffect, useState } from "react";
import { cn } from "@/lib/utils";
import { ArrowUpRight, TrendingUp, TrendingDown } from "lucide-react";

export function StatCard({
  title,
  value,
  icon: Icon,
  trend,
  trendUp,
  iconClass,
  progress = 65,
  colorClass = "bg-primary",
  description = "Overview metrics",
  onClickDetails,
  index = 0,
}) {
  const [displayValue, setDisplayValue] = useState(0);

  useEffect(() => {
    let start = 0;
    const end = typeof value === "number" ? value : parseInt(value, 10);
    if (isNaN(end)) {
      setDisplayValue(value);
      return;
    }
    if (end === 0) {
      setDisplayValue(0);
      return;
    }

    const duration = 800; // ms
    const steps = 30;
    const stepValue = Math.ceil(end / steps) || 1;
    const intervalTime = Math.floor(duration / steps);

    const timer = setInterval(() => {
      start += stepValue;
      if (start >= end) {
        setDisplayValue(end);
        clearInterval(timer);
      } else {
        setDisplayValue(start);
      }
    }, intervalTime);

    return () => clearInterval(timer);
  }, [value]);

  return (
    <motion.div
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, delay: index * 0.03 }}
      onClick={onClickDetails}
      className={cn(
        "relative overflow-hidden rounded-2xl border bg-card/45 backdrop-blur-md p-5 pb-6 shadow-card hover:shadow-xl transition-all duration-300 hover:-translate-y-1 group flex flex-col justify-between",
        onClickDetails && "cursor-pointer",
      )}
    >
      <div>
        <div className="flex items-center justify-between gap-3">
          <p className="min-w-0 truncate text-xs font-semibold text-muted-foreground uppercase tracking-wider">
            {title}
          </p>
          <div
            className={cn(
              "grid size-9 shrink-0 place-items-center rounded-xl transition-all duration-300 group-hover:scale-110",
              iconClass ?? "bg-primary/10 text-primary",
            )}
          >
            <Icon className="size-4.5" />
          </div>
        </div>

        <div className="mt-3 flex items-baseline gap-2">
          <p className="font-display text-3xl font-extrabold tracking-tight">{displayValue}</p>
          {trend && (
            <span
              className={cn(
                "inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full text-[10px] font-bold",
                trendUp ? "bg-success/10 text-success" : "bg-destructive/10 text-destructive",
              )}
            >
              {trendUp ? <TrendingUp className="size-3" /> : <TrendingDown className="size-3" />}
              {trend}
            </span>
          )}
        </div>

        <p className="mt-1 text-xs text-muted-foreground font-medium">{description}</p>

        {/* Progress Bar highlight */}
        <div className="mt-4 h-1 w-full bg-accent rounded-full overflow-hidden">
          <div
            className={cn("h-full rounded-full transition-all duration-1000", colorClass)}
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>

      <div className="mt-4 pt-3 border-t border-border/60 flex items-center justify-between text-[11px] font-bold text-primary hover:text-primary/80 transition-colors w-full">
        <span>View Details</span>
        <ArrowUpRight className="size-3.5 opacity-60 group-hover:opacity-100 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-all" />
      </div>

      {/* Solid bottom accent highlight like the uploaded mockup */}
      <div className={cn("absolute bottom-0 left-0 right-0 h-1.5", colorClass)} />
    </motion.div>
  );
}
