import { createFileRoute } from "@tanstack/react-router";
import { motion } from "motion/react";
import { Download, FileSpreadsheet, FileText, TrendingDown } from "lucide-react";
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
import { departmentReports, monthlyRequests, resolutionTrend } from "@/data/mock";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/reports")({
  head: () => ({
    meta: [
      { title: "Reports — ServiceDesk" },
      {
        name: "description",
        content: "Request analytics, resolution time trends and department-wise reports.",
      },
    ],
  }),
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
  const exportToast = (fmt) => toast.info(`Export to ${fmt} — UI demo only`);

  return (
    <div>
      <PageHeader
        title="Reports & Analytics"
        description="Performance across requests, resolution time and departments"
        crumbs={[{ label: "Reports" }]}
        actions={
          <>
            <Button variant="outline" className="rounded-xl" onClick={() => exportToast("Excel")}>
              <FileSpreadsheet className="mr-1.5 size-4" /> Excel
            </Button>
            <Button variant="outline" className="rounded-xl" onClick={() => exportToast("PDF")}>
              <FileText className="mr-1.5 size-4" /> PDF
            </Button>
            <Button className="rounded-xl" onClick={() => exportToast("CSV")}>
              <Download className="mr-1.5 size-4" /> Export CSV
            </Button>
          </>
        }
      />

      <div className="grid gap-4 lg:grid-cols-2">
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="rounded-2xl border bg-card/40 backdrop-blur-md p-5 shadow-card"
        >
          <h3 className="font-display text-base font-bold">Request Analytics</h3>
          <p className="text-xs text-muted-foreground">Raised vs resolved, last 12 months</p>
          <div className="mt-4 h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={monthlyRequests} barGap={2}>
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
              <LineChart data={resolutionTrend}>
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
    </div>
  );
}
