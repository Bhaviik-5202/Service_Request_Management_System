import { createFileRoute } from "@tanstack/react-router";
import { PageHeader } from "@/components/shared/PageHeader";
import { RequestForm } from "@/components/requests/RequestForm";

export const Route = createFileRoute("/_shell/requests/new")({
  component: NewRequestPage,
});

function NewRequestPage() {
  return (
    <div className="mx-auto max-w-3xl">
      <PageHeader
        title="Raise a New Request"
        description="Fill in the details below — the right team will be assigned automatically."
        crumbs={[{ label: "Service Requests", to: "/requests" }, { label: "New Request" }]}
      />

      <RequestForm />
    </div>
  );
}
