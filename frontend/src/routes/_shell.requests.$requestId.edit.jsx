import { createFileRoute, Link, notFound, useNavigate } from "@tanstack/react-router";
import { PageHeader } from "@/components/shared/PageHeader";
import { RequestForm } from "@/components/requests/RequestForm";
import { Button } from "@/components/ui/button";
import { requests, syncLocalStorage } from "@/data/mock";
import { useAuth, ROLE_PROFILES } from "@/lib/auth";
import { useEffect, useMemo } from "react";

export const Route = createFileRoute("/_shell/requests/$requestId/edit")({
  loader: ({ params }) => {
    syncLocalStorage();
    const request = requests.find((r) => r.id === params.requestId);
    if (!request) throw notFound();
    return { request };
  },
  head: ({ loaderData }) => ({
    meta: [
      {
        title: loaderData
          ? `Edit ${loaderData.request.no} — ServiceDesk`
          : "Edit Request — ServiceDesk",
      },
      { name: "description", content: "Edit an existing service request." },
    ],
  }),
  notFoundComponent: EditNotFound,
  component: EditRequestPage,
});

function EditNotFound() {
  return (
    <div className="py-20 text-center">
      <h1 className="text-xl font-bold">Request not found</h1>
      <Button asChild className="mt-4 rounded-xl">
        <Link to="/requests">Back to requests</Link>
      </Button>
    </div>
  );
}

function EditRequestPage() {
  const { request } = Route.useLoaderData();
  const { role, user } = useAuth();
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
      : { name: "", email: "", department: "", role: "Requestor" };
  }, [user, role]);

  const canEdit = useMemo(() => {
    if (role === "Admin") return true;
    if (role === "Requestor" && request.requesterEmail === activeProfile.email) return true;
    return false;
  }, [role, activeProfile, request]);

  useEffect(() => {
    if (!canEdit) {
      navigate({ to: "/unauthorized", replace: true });
    }
  }, [canEdit, navigate]);

  if (!canEdit) {
    return <div className="text-center py-20 text-muted-foreground">Redirecting...</div>;
  }

  return (
    <div className="mx-auto max-w-3xl">
      <PageHeader
        title={`Edit ${request.no}`}
        description="Changes will notify the assigned technician."
        crumbs={[
          { label: "Service Requests", to: "/requests" },
          { label: request.no, to: `/requests/${request.id}` },
          { label: "Edit" },
        ]}
      />

      <RequestForm existing={request} />
    </div>
  );
}
