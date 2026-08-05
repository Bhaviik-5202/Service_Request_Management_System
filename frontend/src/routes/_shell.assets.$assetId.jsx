import { createFileRoute, Link, notFound } from "@tanstack/react-router";
import { useState, useEffect } from "react";
import { motion } from "motion/react";
import { Boxes, CalendarDays, ShieldCheck, IndianRupee, User, Building2, Hash, Loader2 } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { AssetStatusBadge } from "@/components/shared/badges";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { getAssetById, getAssetCategories, getUsers, getDepartments } from "@/services/api";

export const Route = createFileRoute("/_shell/assets/$assetId")({
  head: ({ params }) => ({
    meta: [
      { title: "Asset Details — ServiceDesk" },
      { name: "description", content: "Asset detail, assignment and warranty information." },
    ],
  }),
  component: AssetDetail,
});

function warrantyProgress(asset) {
  if (!asset.purchaseDate || !asset.warrantyUntil) return 100;
  const start = new Date(asset.purchaseDate).getTime();
  const end = new Date(asset.warrantyUntil).getTime();
  const now = Date.now();
  if (end <= start) return 100;
  return Math.min(100, Math.max(0, Math.round(((now - start) / (end - start)) * 100)));
}

function AssetDetail() {
  const { assetId } = Route.useParams();
  const [asset, setAsset] = useState(null);
  const [loading, setLoading] = useState(true);
  const [notFoundState, setNotFoundState] = useState(false);

  useEffect(() => {
    async function loadAsset() {
      setLoading(true);
      try {
        const [apiAsset, apiCategories, apiUsers, apiDepts] = await Promise.all([
          getAssetById(assetId).catch(() => null),
          getAssetCategories().catch(() => []),
          getUsers().catch(() => []),
          getDepartments().catch(() => []),
        ]);

        if (!apiAsset) {
          setNotFoundState(true);
          return;
        }

        const cat = (apiCategories || []).find((c) => c.assetCategoryId === apiAsset.categoryId || String(c.assetCategoryId) === String(apiAsset.categoryId));
        const u = (apiUsers || []).find((usr) => usr.userId === apiAsset.assignedToUserId || String(usr.userId) === String(apiAsset.assignedToUserId));
        const d = (apiDepts || []).find((dept) => dept.departmentId === apiAsset.departmentId || String(dept.departmentId) === String(apiAsset.departmentId));

        setAsset({
          id: String(apiAsset.assetId),
          tag: apiAsset.assetTag || `AST-${apiAsset.assetId}`,
          name: apiAsset.assetName,
          category: cat ? cat.categoryName : (apiAsset.category?.categoryName || "General"),
          assignedTo: u ? (u.fullName || u.name) : (apiAsset.assignedToUser?.fullName || null),
          department: d ? d.departmentName : (apiAsset.department?.departmentName || "N/A"),
          status: apiAsset.status || "Available",
          serial: apiAsset.serialNumber || "N/A",
          value: apiAsset.bookValue ? `₹${Number(apiAsset.bookValue).toLocaleString("en-IN")}` : "₹0",
          purchaseDate: apiAsset.purchaseDate ? new Date(apiAsset.purchaseDate).toISOString().split("T")[0] : new Date().toISOString().split("T")[0],
          warrantyUntil: apiAsset.warrantyUntil ? new Date(apiAsset.warrantyUntil).toISOString().split("T")[0] : new Date().toISOString().split("T")[0],
        });
      } catch (err) {
        setNotFoundState(true);
      } finally {
        setLoading(false);
      }
    }

    loadAsset();
  }, [assetId]);

  if (loading) {
    return (
      <div className="py-20 text-center">
        <Loader2 className="size-8 mx-auto animate-spin text-primary" />
        <p className="mt-3 text-sm text-muted-foreground">Loading asset details…</p>
      </div>
    );
  }

  if (notFoundState || !asset) {
    return (
      <div className="py-20 text-center">
        <h1 className="text-xl font-bold">Asset not found</h1>
        <Button asChild className="mt-4 rounded-xl">
          <Link to="/assets">Back to assets</Link>
        </Button>
      </div>
    );
  }

  const progress = warrantyProgress(asset);
  const expired = new Date(asset.warrantyUntil).getTime() < Date.now();

  const meta = [
    { icon: Hash, label: "Serial number", value: asset.serial },
    { icon: Boxes, label: "Category", value: asset.category },
    { icon: User, label: "Assigned to", value: asset.assignedTo ?? "Unassigned" },
    { icon: Building2, label: "Department", value: asset.department },
    {
      icon: CalendarDays,
      label: "Purchase date",
      value: new Date(asset.purchaseDate).toLocaleDateString("en-IN", { dateStyle: "medium" }),
    },
    { icon: IndianRupee, label: "Book value", value: asset.value },
  ];

  return (
    <div>
      <PageHeader
        title={asset.name}
        description={asset.tag}
        crumbs={[{ label: "Assets", to: "/assets" }, { label: asset.tag }]}
        actions={<AssetStatusBadge status={asset.status} className="text-sm" />}
      />

      <div className="grid gap-4 lg:grid-cols-3">
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card lg:col-span-2"
        >
          <h3 className="font-display text-base font-bold">Asset Information</h3>
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            {meta.map((m) => (
              <div key={m.label} className="flex items-start gap-3 rounded-xl bg-muted/50 p-3">
                <div className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary/10 text-primary">
                  <m.icon className="size-4" />
                </div>
                <div className="min-w-0">
                  <p className="text-xs text-muted-foreground">{m.label}</p>
                  <p className="truncate text-sm font-semibold">{m.value}</p>
                </div>
              </div>
            ))}
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="h-fit rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
        >
          <h3 className="flex items-center gap-2 font-display text-base font-bold">
            <ShieldCheck className="size-4 text-primary" /> Warranty
          </h3>
          <p className="mt-3 text-sm text-muted-foreground">
            {expired ? "Warranty expired on" : "Covered until"}{" "}
            <span className="font-semibold text-foreground">
              {new Date(asset.warrantyUntil).toLocaleDateString("en-IN", { dateStyle: "medium" })}
            </span>
          </p>
          <Progress value={progress} className="mt-3 h-2" />
          <p className="mt-2 text-xs text-muted-foreground">
            {progress}% of warranty period elapsed
          </p>
          {expired && (
            <p className="mt-3 rounded-xl bg-destructive/10 p-3 text-xs font-medium text-destructive">
              This asset is out of warranty. Repairs will be billed separately.
            </p>
          )}
        </motion.div>
      </div>
    </div>
  );
}
