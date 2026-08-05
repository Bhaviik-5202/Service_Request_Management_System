import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

export function DeleteConfirmation({
  isOpen,
  onClose,
  onConfirm,
  title = "Are you absolutely sure?",
  description,
  confirmText = "Delete",
  cancelText = "Cancel",
  itemName,
}) {
  return (
    <AlertDialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <AlertDialogContent className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card sm:rounded-2xl">
        <AlertDialogHeader className="text-left">
          <AlertDialogTitle className="font-display text-lg font-bold tracking-tight">
            {title}
          </AlertDialogTitle>
          <AlertDialogDescription className="mt-2 text-sm text-muted-foreground">
            {description || (
              <>
                This action cannot be undone. This will permanently delete{" "}
                {itemName ? (
                  <span className="font-semibold text-foreground">"{itemName}"</span>
                ) : (
                  "this record"
                )}{" "}
                from the local storage configuration.
              </>
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter className="mt-4 flex sm:space-x-2">
          <AlertDialogCancel onClick={onClose} className="rounded-xl cursor-pointer">
            {cancelText}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={() => {
              onConfirm();
              onClose();
            }}
            className="rounded-xl bg-destructive text-destructive-foreground hover:bg-destructive/90 cursor-pointer"
          >
            {confirmText}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
