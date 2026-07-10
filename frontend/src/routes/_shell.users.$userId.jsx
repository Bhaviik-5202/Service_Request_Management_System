import { createFileRoute, Link, notFound } from "@tanstack/react-router";
import { motion } from "motion/react";
import { Mail, Phone, Building2, CalendarDays, Ticket, CheckCircle2 } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatusBadge, PriorityBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { requests, users, syncLocalStorage } from "@/data/mock";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_shell/users/$userId")({
  loader: ({ params }) => {
    syncLocalStorage();
    const user = users.find((u) => u.id === params.userId);
    if (!user) throw notFound();
    return { user };
  },
  head: ({ loaderData }) => ({
    meta: [
      { title: loaderData ? `${loaderData.user.name} — ServiceDesk` : "User — ServiceDesk" },
      { name: "description", content: "User profile, role, department and request history." },
    ],
  }),
  notFoundComponent: UserNotFound,
  component: UserProfile,
});

function UserNotFound() {
  return (
    <div className="py-20 text-center">
      <h1 className="text-xl font-bold">User not found</h1>
      <Button asChild className="mt-4 rounded-xl">
        <Link to="/users">Back to users</Link>
      </Button>
    </div>
  );
}

function UserProfile() {
  const { user } = Route.useLoaderData();
  const userRequests = requests
    .filter((r) => r.requester === user.name || r.assignee === user.name)
    .slice(0, 5);

  return (
    <div>
      <PageHeader
        title="User Profile"
        crumbs={[{ label: "Users", to: "/users" }, { label: user.name }]}
      />

      <div className="grid gap-4 lg:grid-cols-3">
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="h-fit rounded-2xl border bg-card/40 backdrop-blur-md p-6 text-center shadow-card"
        >
          <Avatar className="mx-auto size-20">
            <AvatarFallback className="bg-primary text-xl font-bold text-primary-foreground">
              {user.name
                .split(" ")
                .map((w) => w[0])
                .join("")}
            </AvatarFallback>
          </Avatar>
          <h2 className="mt-4 text-lg font-bold">{user.name}</h2>
          <p className="text-sm text-muted-foreground">
            {user.role} · {user.department}
          </p>
          <span
            className={cn(
              "mt-3 inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold ring-1 ring-inset",
              user.status === "Active"
                ? "bg-success/10 text-success ring-success/20"
                : "bg-muted text-muted-foreground ring-border",
            )}
          >
            <span className="size-1.5 rounded-full bg-current" /> {user.status}
          </span>
          <div className="mt-6 space-y-3 text-left">
            {[
              { icon: Mail, value: user.email },
              { icon: Phone, value: user.phone },
              { icon: Building2, value: user.department },
              {
                icon: CalendarDays,
                value: `Joined ${new Date(user.joined).toLocaleDateString("en-IN", { dateStyle: "medium" })}`,
              },
            ].map((m, i) => (
              <div key={i} className="flex items-center gap-3 rounded-xl bg-muted/50 p-3">
                <m.icon className="size-4 shrink-0 text-primary" />
                <p className="min-w-0 truncate text-sm font-medium">{m.value}</p>
              </div>
            ))}
          </div>
        </motion.div>

        <div className="space-y-4 lg:col-span-2">
          <div className="grid grid-cols-2 gap-4">
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.05 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium text-muted-foreground">Requests raised</p>
                <Ticket className="size-4 text-primary" />
              </div>
              <p className="mt-2 font-display text-3xl font-bold">{user.requestsRaised}</p>
            </motion.div>
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium text-muted-foreground">Requests resolved</p>
                <CheckCircle2 className="size-4 text-success" />
              </div>
              <p className="mt-2 font-display text-3xl font-bold">{user.requestsResolved}</p>
            </motion.div>
          </div>

          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold">Related Requests</h3>
              <p className="text-xs text-muted-foreground">
                Raised by or assigned to {user.name.split(" ")[0]}
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Request</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {userRequests.length === 0 && (
                    <TableRow>
                      <TableCell
                        colSpan={3}
                        className="py-10 text-center text-sm text-muted-foreground"
                      >
                        No related requests.
                      </TableCell>
                    </TableRow>
                  )}
                  {userRequests.map((r) => (
                    <TableRow key={r.id} className="group">
                      <TableCell>
                        <Link
                          to="/requests/$requestId"
                          params={{ requestId: r.id }}
                          className="block min-w-0"
                        >
                          <p className="max-w-52 truncate text-sm font-semibold group-hover:text-primary sm:max-w-72">
                            {r.title}
                          </p>
                          <p className="text-xs text-muted-foreground">{r.no}</p>
                        </Link>
                      </TableCell>
                      <TableCell>
                        <PriorityBadge priority={r.priority} />
                      </TableCell>
                      <TableCell>
                        <StatusBadge status={r.status} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </motion.div>
        </div>
      </div>
    </div>
  );
}
