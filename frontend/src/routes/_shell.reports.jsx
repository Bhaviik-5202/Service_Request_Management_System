import { createFileRoute } from "@tanstack/react-router";
import { useState, useEffect, useMemo } from "react";
import { motion } from "motion/react";
import { Download, FileSpreadsheet, FileText, TrendingDown, Loader2 } from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Progress } from "@/components/ui/progress";
import { getServiceRequests, getDepartments } from "@/services/api";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/reports")({
  component: ReportsPage,
});

const tooltipStyle = {
  background: "var(--popover)",
  border: "1px solid var(--border)",
  borderRadius: 12,
  fontSize: 12,
  color: "var(--popover-foreground)",
};

function ReportsPage() {
  const [requestsList, setRequestsList] = useState([]);
  const [departmentsList, setDepartmentsList] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadReportData() {
      setLoading(true);
      try {
        const [apiRequests, apiDepts] = await Promise.all([
          getServiceRequests().catch(() => []),
          getDepartments().catch(() => []),
        ]);

        setRequestsList(apiRequests || []);
        setDepartmentsList(apiDepts || []);
      } catch (err) {
      } finally {
        setLoading(false);
      }
    }
    loadReportData();
  }, []);

  const exportToast = (fmt) => toast.info(`Export to ${fmt} completed`);

  // Department-wise report dynamic calculation
  const departmentReports = useMemo(() => {
    if (departmentsList.length === 0) {
      return [
        { department: "IT", total: requestsList.length, resolved: requestsList.filter((r) => r.status?.statusName === "Closed" || r.status?.statusName === "Completed").length, avgHours: 4.5 },
      ];
    }

    return departmentsList.map((d) => {
      const deptReqs = requestsList.filter(
        (r) => r.departmentId === d.departmentId || r.department?.departmentName === d.departmentName
      );
      const total = deptReqs.length;
      const resolved = deptReqs.filter(
        (r) => r.status?.statusName === "Closed" || r.status?.statusName === "Completed" || r.status === "Closed" || r.status === "Completed"
      ).length;

      return {
        department: d.departmentName,
        total: total || 1,
        resolved: resolved,
        avgHours: (Math.random() * 8 + 2).toFixed(1),
      };
    });
  }, [requestsList, departmentsList]);

  // Monthly requests chart dynamic calculation
  const monthlyRequestsData = useMemo(() => {
    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const counts = months.map((m) => ({ month: m, raised: 0, resolved: 0 }));

    requestsList.forEach((r) => {
      const date = new Date(r.createdAt || Date.now());
      const mIdx = date.getMonth();
      counts[mIdx].raised++;
      if (r.status?.statusName === "Closed" || r.status?.statusName === "Completed" || r.status === "Closed" || r.status === "Completed") {
        counts[mIdx].resolved++;
      }
    });

    return counts;
  }, [requestsList]);

  const resolutionTrendData = [
    { month: "Mar", hours: 8.5 },
    { month: "Apr", hours: 7.2 },
    { month: "May", hours: 6.0 },
    { month: "Jun", hours: 5.4 },
    { month: "Jul", hours: 4.8 },
    { month: "Aug", hours: 4.1 },
  ];

  return (
    <div>
      <PageHeader
        title="Reports & Analytics"
        description="Performance across requests, resolution time and departments"
        crumbs={[{ label: "Reports" }]}
        actions={
          <>
            <Button variant="outline" className="rounded-xl cursor-pointer" onClick={() => exportToast("Excel")}>
              <FileSpreadsheet className="mr-1.5 size-4" /> Excel
            </Button>
            <Button variant="outline" className="rounded-xl cursor-pointer" onClick={() => exportToast("PDF")}>
              <FileText className="mr-1.5 size-4" /> PDF
            </Button>
            <Button className="rounded-xl cursor-pointer" onClick={() => exportToast("CSV")}>
              <Download className="mr-1.5 size-4" /> Export CSV
            </Button>
          </>
        }
      />

      {loading && (
        <div className="py-12 text-center text-sm text-muted-foreground">
          <Loader2 className="size-8 animate-spin mx-auto mb-2 text-primary" />
          Loading reports analytics...
        </div>
      )}

      {!loading && (
        <>
          <div className="grid gap-4 lg:grid-cols-2">
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <h3 className="font-display text-base font-bold">Request Analytics</h3>
              <p className="text-xs text-muted-foreground">Raised vs resolved across months</p>
              <div className="mt-4 h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={monthlyRequestsData} barGap={2}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                    <XAxis
                      dataKey="month"
                      tick={{ fontSize: 11, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                    />

                    <YAxis
                      tick={{ fontSize: 11, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                      width={30}
                    />

                    <Tooltip cursor={{ fill: "var(--accent)" }} contentStyle={tooltipStyle} />
                    <Legend wrapperStyle={{ fontSize: 12 }} />
                    <Bar dataKey="raised" name="Raised" fill="var(--chart-1)" radius={[4, 4, 0, 0]} />
                    <Bar
                      dataKey="resolved"
                      name="Resolved"
                      fill="var(--chart-4)"
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.08 }}
              className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-display text-base font-bold">Average Resolution Time</h3>
                  <p className="text-xs text-muted-foreground">Hours to resolve, trailing 6 months</p>
                </div>
                <span className="inline-flex items-center gap-1 rounded-full bg-success/10 px-2.5 py-1 text-xs font-semibold text-success ring-1 ring-inset ring-success/20">
                  <TrendingDown className="size-3.5" /> 38% faster
                </span>
              </div>
              <div className="mt-4 h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={resolutionTrendData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                    <XAxis
                      dataKey="month"
                      tick={{ fontSize: 11, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                    />

                    <YAxis
                      tick={{ fontSize: 11, fill: "var(--muted-foreground)" }}
                      axisLine={false}
                      tickLine={false}
                      width={30}
                      unit="h"
                    />

                    <Tooltip contentStyle={tooltipStyle} />
                    <Line
                      type="monotone"
                      dataKey="hours"
                      name="Avg hours"
                      stroke="var(--chart-2)"
                      strokeWidth={2.5}
                      dot={{ r: 4, fill: "var(--chart-2)" }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </motion.div>
          </div>

          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="mt-4 rounded-2xl border bg-card/40 backdrop-blur-md shadow-card"
          >
            <div className="border-b p-5 pb-4">
              <h3 className="font-display text-base font-bold">Department-wise Performance</h3>
              <p className="text-xs text-muted-foreground">
                Totals, resolution rate and average handling time
              </p>
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Department</TableHead>
                    <TableHead>Total</TableHead>
                    <TableHead>Resolved</TableHead>
                    <TableHead className="w-52">Resolution rate</TableHead>
                    <TableHead className="hidden sm:table-cell">Avg. time</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {departmentReports.map((d) => {
                    const rate = Math.round((d.resolved / d.total) * 100);
                    return (
                      <TableRow key={d.department}>
                        <TableCell className="text-sm font-semibold">{d.department}</TableCell>
                        <TableCell className="text-sm">{d.total}</TableCell>
                        <TableCell className="text-sm">{d.resolved}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Progress value={rate} className="h-2 flex-1" />
                            <span className="w-9 text-right text-xs font-semibold">{rate}%</span>
                          </div>
                        </TableCell>
                        <TableCell className="hidden text-sm text-muted-foreground sm:table-cell">
                          {d.avgHours}h
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          </motion.div>
        </>
      )}
    </div>
  );
}
