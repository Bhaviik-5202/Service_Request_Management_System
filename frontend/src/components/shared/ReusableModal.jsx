import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { cn } from "@/lib/utils";

const sizeClasses = {
  sm: "max-w-sm",
  md: "max-w-md",
  lg: "max-w-lg",
  xl: "max-w-xl",
  "2xl": "max-w-2xl",
  "3xl": "max-w-3xl",
  "4xl": "max-w-4xl",
  full: "max-w-[95vw] h-[90vh] md:h-[85vh]",
};

export function ReusableModal({
  isOpen,
  onClose,
  title,
  description,
  children,
  footer,
  size = "md",
}) {
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent
        className={cn(
          "rounded-2xl bg-card dark:bg-card/40 backdrop-blur-md shadow-card sm:rounded-2xl border border-border/60 dark:border-border/20 p-6 overflow-y-auto max-h-[90vh] text-foreground",
          sizeClasses[size],
        )}
      >
        <DialogHeader className="space-y-1.5 text-left">
          <DialogTitle className="font-display text-xl font-bold tracking-tight">
            {title}
          </DialogTitle>
          {description && (
            <DialogDescription className="text-sm text-muted-foreground">
              {description}
            </DialogDescription>
          )}
        </DialogHeader>
        <div className="py-2">{children}</div>
        {footer && <DialogFooter className="mt-4 flex sm:space-x-2">{footer}</DialogFooter>}
      </DialogContent>
    </Dialog>
  );
}
