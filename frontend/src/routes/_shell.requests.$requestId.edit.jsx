import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { PageHeader } from "@/components/shared/PageHeader";
import { RequestForm } from "@/components/requests/RequestForm";
import { Button } from "@/components/ui/button";
import { getServiceRequestById } from "@/services/api";
import { useAuth } from "@/lib/auth";
import { useEffect, useMemo, useState } from "react";
import { Loader2 } from "lucide-react";

export const Route = createFileRoute("/_shell/requests/$requestId/edit")({
  component: EditRequestPage,
});

function EditRequestPage() {
  const { requestId } = Route.useParams();
  const [request, setRequest] = useState(null);
  const [loading, setLoading] = useState(true);
  const { role, user } = useAuth();
  const navigate = useNavigate();

  const activeProfile = useMemo(() => {
    return {
      name: user?.fullName || user?.name || "",
      email: user?.email || "",
      department: user?.department || "",
      role: user?.role || role || "Requestor",
    };
  }, [user, role]);


  useEffect(() => {
    async function loadRequest() {
      setLoading(true);
      try {
        const sr = await getServiceRequestById(requestId);
        if (sr) {
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
            requesterUserId: sr.requesterUserId,
            createdByUserId: sr.createdByUserId,
          });
        }
      } catch (err) {
      } finally {
        setLoading(false);
      }
    }

    loadRequest();
  }, [requestId]);

  const canEdit = useMemo(() => {
    if (!request) return false;
    if (role === "Admin" || role === "HOD") return true;
    if (role === "Requestor" && activeProfile.email && request.requesterEmail.toLowerCase() === activeProfile.email.toLowerCase()) return true;
    return false;
  }, [role, activeProfile, request]);

  useEffect(() => {
    if (!loading && request && !canEdit) {
      navigate({ to: "/unauthorized", replace: true });
    }
  }, [loading, request, canEdit, navigate]);

  if (loading) {
    return (
      <div className="py-20 text-center">
        <Loader2 className="size-8 mx-auto animate-spin text-primary" />
        <p className="mt-3 text-sm text-muted-foreground">Loading request details…</p>
      </div>
    );
  }

  if (!request) {
    return (
      <div className="py-20 text-center">
        <h1 className="text-xl font-bold">Request not found</h1>
        <Button asChild className="mt-4 rounded-xl">
          <Link to="/requests">Back to requests</Link>
        </Button>
      </div>
    );
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
