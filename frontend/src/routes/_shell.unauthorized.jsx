import { createFileRoute, Link } from "@tanstack/react-router";
import { ShieldAlert, ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";

export const Route = createFileRoute("/_shell/unauthorized")({
  head: () => ({
    meta: [
      { title: "Access Restricted — ServiceDesk" },
      { name: "description", content: "You do not have permission to view this page." },
    ],
  }),
  component: UnauthorizedPage,
});

function UnauthorizedPage() {
  return (
    <div className="grid place-items-center py-20 text-center px-4">
      <div className="max-w-md w-full rounded-2xl border bg-card/40 backdrop-blur-md p-8 shadow-card flex flex-col items-center">
        <div className="grid size-14 place-items-center rounded-2xl bg-destructive/10 text-destructive mb-6">
          <ShieldAlert className="size-7" />
        </div>
        <h1 className="font-display text-2xl font-bold tracking-tight text-foreground">
          Access Restricted
        </h1>
        <p className="mt-3 text-sm text-muted-foreground leading-relaxed">
          Your current account role does not have permission to view this page or resource. Contact
          your IT administrator or HOD if you believe this is in error.
        </p>
        <div className="mt-8 flex flex-col gap-2 w-full">
          <Button asChild className="rounded-xl font-semibold cursor-pointer w-full">
            <Link to="/">Go to Dashboard</Link>
          </Button>
          <Button
            asChild
            variant="outline"
            className="rounded-xl font-semibold cursor-pointer w-full"
          >
            <Link to="/requests" className="flex items-center justify-center gap-1.5">
              <ArrowLeft className="size-4" /> Go back to requests
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
