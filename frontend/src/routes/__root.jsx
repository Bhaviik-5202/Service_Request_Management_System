import { QueryClientProvider } from "@tanstack/react-query";
import {
  Outlet,
  Link,
  createRootRouteWithContext,
  useRouter,
  HeadContent,
  Scripts,
} from "@tanstack/react-router";

import appCss from "../styles.css?url";
import { ThemeProvider } from "../lib/theme";
import { Toaster } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider } from "../lib/auth";

import { motion } from "motion/react";

function NotFoundComponent() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4 selection:bg-primary/20">
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="max-w-md text-center p-8 rounded-3xl border bg-card/60 backdrop-blur-xl shadow-card"
      >
        <span className="inline-flex items-center gap-1.5 px-3 py-1 text-xs font-semibold rounded-full bg-destructive/10 text-destructive border border-destructive/20">
          Error 404
        </span>
        <h1 className="text-8xl font-black tracking-tighter mt-4 bg-gradient-to-r from-primary via-info to-success bg-clip-text text-transparent font-display animate-none">
          404
        </h1>
        <h2 className="mt-2 text-xl font-bold tracking-tight text-foreground font-display">
          Page Not Found
        </h2>
        <p className="mt-3 text-sm text-muted-foreground leading-relaxed">
          The requested page could not be located on our support desk server. It might have been
          moved or deleted.
        </p>
        <div className="mt-6">
          <Link
            to="/"
            className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground transition-all hover:bg-primary/95 shadow-md shadow-primary/10 hover:shadow-lg hover:shadow-primary/20 hover:-translate-y-0.5 cursor-pointer"
          >
            Return to Dashboard
          </Link>
        </div>
      </motion.div>
    </div>
  );
}

function ErrorComponent({ error, reset }) {
  console.error(error);
  const router = useRouter();

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4 selection:bg-primary/20">
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="max-w-md text-center p-8 rounded-3xl border bg-card/60 backdrop-blur-xl shadow-card"
      >
        <span className="inline-flex items-center gap-1.5 px-3 py-1 text-xs font-semibold rounded-full bg-warning/15 text-warning-foreground dark:text-warning border border-warning/30">
          System Failure
        </span>
        <h1 className="text-6xl font-black tracking-tighter mt-4 bg-gradient-to-r from-destructive to-warning bg-clip-text text-transparent font-display">
          Oops!
        </h1>
        <h2 className="mt-2 text-xl font-bold tracking-tight text-foreground font-display">
          An error occurred
        </h2>
        <p className="mt-3 text-sm text-muted-foreground leading-relaxed">
          We encountered an unexpected crash while rendering this section. Our development team has
          been notified.
        </p>

        {error?.message && (
          <div className="mt-4 p-3 rounded-xl bg-accent/30 text-left border text-xs font-mono max-h-24 overflow-y-auto select-all text-muted-foreground">
            {error.message}
          </div>
        )}

        <div className="mt-6 flex flex-wrap justify-center gap-3">
          <button
            onClick={() => {
              router.invalidate();
              reset();
            }}
            className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground transition-all hover:bg-primary/95 shadow-md shadow-primary/10 hover:shadow-lg hover:shadow-primary/20 hover:-translate-y-0.5 cursor-pointer"
          >
            Reload Component
          </button>
          <a
            href="/"
            className="inline-flex items-center justify-center rounded-xl border border-input bg-card/40 backdrop-blur-md px-5 py-2.5 text-sm font-semibold text-foreground transition-all hover:bg-accent hover:-translate-y-0.5"
          >
            Go Home
          </a>
        </div>
      </motion.div>
    </div>
  );
}

export const Route = createRootRouteWithContext()({
  head: () => ({
    meta: [
      { charSet: "utf-8" },
      { name: "viewport", content: "width=device-width, initial-scale=1" },
      { title: "IT Service Request Management System" },
      {
        name: "description",
        content:
          "Raise, track, approve and resolve IT service requests with dashboards, assets, approvals and reports.",
      },
      { name: "author", content: "IT Service Request Management System" },
      { property: "og:title", content: "IT Service Request Management System" },
      {
        property: "og:description",
        content:
          "Raise, track, approve and resolve IT service requests with dashboards, assets, approvals and reports.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
      { name: "twitter:title", content: "IT Service Request Management System" },
      {
        name: "twitter:description",
        content:
          "Raise, track, approve and resolve IT service requests with dashboards, assets, approvals and reports.",
      },
      { property: "og:image", content: "/og-image.png" },
      { name: "twitter:image", content: "/og-image.png" },
    ],
    links: [
      {
        rel: "stylesheet",
        href: appCss,
      },
      { rel: "preconnect", href: "https://fonts.googleapis.com" },
      { rel: "preconnect", href: "https://fonts.gstatic.com", crossOrigin: "anonymous" },
      {
        rel: "stylesheet",
        href: "https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&family=Space+Grotesk:wght@400;500;600;700&display=swap",
      },
    ],
  }),

  shellComponent: RootShell,
  component: RootComponent,
  notFoundComponent: NotFoundComponent,
  errorComponent: ErrorComponent,
});

function RootShell({ children }) {
  return (
    <html lang="en">
      <head>
        <HeadContent />
      </head>
      <body>
        {children}
        <Scripts />
      </body>
    </html>
  );
}

function RootComponent() {
  const { queryClient } = Route.useRouteContext();

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <TooltipProvider delayDuration={200}>
            {/* Required: nested routes render here. Removing <Outlet /> breaks all child routes. */}
            <Outlet />
            <Toaster position="top-right" />
          </TooltipProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
