import { Link } from "@tanstack/react-router";
import { ChevronRight, Home } from "lucide-react";

export function PageHeader({ title, description, crumbs, actions }) {
  return (
    <div className="mb-6">
      <nav
        className="mb-3 flex items-center gap-1.5 text-xs text-muted-foreground"
        aria-label="Breadcrumb"
      >
        <Link
          to="/"
          className="inline-flex items-center gap-1 transition-colors hover:text-foreground"
        >
          <Home className="size-3.5" />
          Home
        </Link>
        {crumbs?.map((c, i) => (
          <span key={i} className="flex items-center gap-1.5">
            <ChevronRight className="size-3" />
            {c.to ? (
              <Link to={c.to} className="transition-colors hover:text-foreground">
                {c.label}
              </Link>
            ) : (
              <span className="font-medium text-foreground">{c.label}</span>
            )}
          </span>
        ))}
      </nav>
      <div className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4 sm:flex sm:flex-wrap sm:items-center sm:justify-between">
        <div className="min-w-0">
          <h1 className="truncate text-xl font-bold sm:text-2xl">{title}</h1>
          {description && <p className="mt-1 text-sm text-muted-foreground">{description}</p>}
        </div>
        {actions && <div className="flex shrink-0 flex-wrap items-center gap-2">{actions}</div>}
      </div>
    </div>
  );
}
