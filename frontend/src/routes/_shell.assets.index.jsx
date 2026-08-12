import { useMemo, useState, useEffect } from "react";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { Boxes, CircleCheck, Wrench, Archive, Search } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatCard } from "@/components/shared/StatCard";
import { AssetStatusBadge } from "@/components/shared/badges";
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
import { assets } from "@/data/mock";

export const Route = createFileRoute("/_shell/assets/")({
  validateSearch: (search) => ({
    status: search.status || "all",
  }),
  head: () => ({
    meta: [
      { title: "Assets — ServiceDesk" },
      {
        name: "description",
        content: "Track IT and facility assets, assignments, warranty and repair status.",
      },
    ],
  }),
  component: AssetsPage,
});

function AssetsPage() {
  const navigate = useNavigate();
  const searchParams = Route.useSearch();
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState(searchParams.status || "all");

  useEffect(() => {
    setStatus(searchParams.status || "all");
  }, [searchParams.status]);

  const filtered = useMemo(
    () =>
      assets.filter((a) => {
        const q = search.toLowerCase();
        const matches =
          !q ||
          a.name.toLowerCase().includes(q) ||
          a.tag.toLowerCase().includes(q) ||
          (a.assignedTo ?? "").toLowerCase().includes(q);
        return matches && (status === "all" || a.status === status);
      }),
    [search, status],
  );

  const counts = {
    inUse: assets.filter((a) => a.status === "In Use").length,
    available: assets.filter((a) => a.status === "Available").length,
    repair: assets.filter((a) => a.status === "Under Repair").length,
    retired: assets.filter((a) => a.status === "Retired").length,
  };

  return (
    <div>
      <PageHeader
        title="Asset Management"
        description={`${assets.length} assets in inventory`}
        crumbs={[{ label: "Assets" }]}
      />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard
          title="In Use"
          value={counts.inUse}
          icon={Boxes}
          iconClass="bg-info/10 text-info"
          index={0}
          onClickDetails={() => navigate({ to: "/assets", search: { status: "In Use" } })}
        />

        <StatCard
          title="Available"
          value={counts.available}
          icon={CircleCheck}
          iconClass="bg-success/10 text-success"
          index={1}
          onClickDetails={() => navigate({ to: "/assets", search: { status: "Available" } })}
        />

        <StatCard
          title="Under Repair"
          value={counts.repair}
          icon={Wrench}
          iconClass="bg-warning/15 text-warning-foreground dark:text-warning"
          index={2}
          onClickDetails={() => navigate({ to: "/assets", search: { status: "Under Repair" } })}
        />

        <StatCard
          title="Retired"
          value={counts.retired}
          icon={Archive}
          iconClass="bg-muted text-muted-foreground"
          index={3}
          onClickDetails={() => navigate({ to: "/assets", search: { status: "Retired" } })}
        />
      </div>

      <div className="mt-6 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card">
        <div className="flex flex-col gap-3 border-b p-4 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by name, tag or assignee…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-9 rounded-xl pl-9"
            />
          </div>
          <Select value={status} onValueChange={setStatus}>
            <SelectTrigger className="h-9 rounded-xl sm:w-44">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              {["In Use", "Available", "Under Repair", "Retired"].map((s) => (
                <SelectItem key={s} value={s}>
                  {s}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Asset</TableHead>
                <TableHead className="hidden md:table-cell">Category</TableHead>
                <TableHead className="hidden lg:table-cell">Assigned to</TableHead>
                <TableHead className="hidden sm:table-cell">Department</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={5}
                    className="py-12 text-center text-sm text-muted-foreground"
                  >
                    No assets found.
                  </TableCell>
                </TableRow>
              )}
              {filtered.map((a) => (
                <TableRow key={a.id} className="group">
                  <TableCell>
                    <Link
                      to="/assets/$assetId"
                      params={{ assetId: a.id }}
                      className="block min-w-0"
                    >
                      <p className="max-w-52 truncate text-sm font-semibold group-hover:text-primary sm:max-w-72">
                        {a.name}
                      </p>
                      <p className="text-xs text-muted-foreground">{a.tag}</p>
                    </Link>
                  </TableCell>
                  <TableCell className="hidden text-sm md:table-cell">{a.category}</TableCell>
                  <TableCell className="hidden text-sm lg:table-cell">
                    {a.assignedTo ?? <span className="text-muted-foreground">—</span>}
                  </TableCell>
                  <TableCell className="hidden text-sm text-muted-foreground sm:table-cell">
                    {a.department}
                  </TableCell>
                  <TableCell>
                    <AssetStatusBadge status={a.status} />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
