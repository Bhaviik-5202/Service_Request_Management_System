import { ArrowUp, ArrowDown, ArrowUpDown } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

export function ReusableTable({
  columns,
  data,
  loading = false,
  emptyMessage = "No records found.",
  sortColumn,
  sortDirection,
  onSort,
  onRowClick,
}) {
  const handleSort = (column) => {
    if (column.sortable && onSort) {
      onSort(column.key);
    }
  };

  return (
    <div className="overflow-x-auto rounded-2xl border bg-card/40 backdrop-blur-md shadow-card">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            {columns.map((column) => {
              const isSorted = sortColumn === column.key;
              return (
                <TableHead
                  key={column.key}
                  className={cn(
                    column.className,
                    column.sortable && "cursor-pointer select-none hover:text-foreground",
                  )}
                  onClick={() => handleSort(column)}
                >
                  <div className="flex items-center gap-1.5 py-1">
                    <span>{column.header}</span>
                    {column.sortable && (
                      <span className="text-muted-foreground/60 transition-colors group-hover:text-foreground">
                        {isSorted ? (
                          sortDirection === "asc" ? (
                            <ArrowUp className="size-3.5 text-primary" />
                          ) : (
                            <ArrowDown className="size-3.5 text-primary" />
                          )
                        ) : (
                          <ArrowUpDown className="size-3.5 opacity-60 hover:opacity-100" />
                        )}
                      </span>
                    )}
                  </div>
                </TableHead>
              );
            })}
          </TableRow>
        </TableHeader>
        <TableBody>
          {loading ? (
            Array.from({ length: 5 }).map((_, rIdx) => (
              <TableRow key={rIdx} className="hover:bg-transparent">
                {columns.map((column, cIdx) => (
                  <TableCell key={cIdx} className={column.className}>
                    <Skeleton className="h-5 w-full rounded-md" />
                  </TableCell>
                ))}
              </TableRow>
            ))
          ) : data.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={columns.length}
                className="py-12 text-center text-sm text-muted-foreground"
              >
                <div className="flex flex-col items-center justify-center gap-2">
                  <span className="text-base font-medium">{emptyMessage}</span>
                  <span className="text-xs text-muted-foreground/75">
                    Try adjusting your filters or search terms.
                  </span>
                </div>
              </TableCell>
            </TableRow>
          ) : (
            data.map((row, rIdx) => (
              <TableRow
                key={rIdx}
                className={cn(
                  "group transition-colors",
                  onRowClick && "cursor-pointer hover:bg-muted/50",
                )}
                onClick={() => onRowClick && onRowClick(row)}
              >
                {columns.map((column) => (
                  <TableCell key={column.key} className={cn("py-3", column.className)}>
                    {column.render ? column.render(row, rIdx) : row[column.key]}
                  </TableCell>
                ))}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
