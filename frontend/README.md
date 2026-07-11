# IT Service Request Management System — Frontend

A full-featured **IT Service Desk** web application built with React 19, TanStack Start, and TailwindCSS v4. It allows employees to raise, track, and manage IT service requests through a role-based dashboard.

---

## ✨ Features

- **Role-Based Access Control** — Four roles with distinct permissions: `Admin`, `HOD`, `Technician`, `Requestor`
- **Dashboard** — Live stats, recent activity, and key metrics per role
- **Service Requests** — Create, view, edit, and delete IT support tickets with priority and status tracking
- **Approvals** — HOD-level approval workflow for pending requests
- **Asset Management** — Browse and manage IT assets with detail views
- **User Management** — Admin-only user directory with profile editing
- **Reports** — Department-wise reports, monthly trends, and resolution rate charts
- **Notifications** — In-app notification feed
- **Settings** — Theme toggle (light/dark), profile management
- **Help & FAQ** — Searchable FAQ accordion
- **Authentication** — Sign-in, sign-up, forgot/reset password flows with session persistence
- **Dark Mode** — Full light/dark theme support via `ThemeProvider`

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | [TanStack Start](https://tanstack.com/start) (SSR-capable React meta-framework) |
| Routing | [TanStack Router](https://tanstack.com/router) (file-based, type-safe) |
| UI Components | [shadcn/ui](https://ui.shadcn.com/) on [Radix UI](https://www.radix-ui.com/) |
| Styling | [TailwindCSS v4](https://tailwindcss.com/) + `tw-animate-css` |
| Animations | [Motion (Framer Motion v12)](https://motion.dev/) |
| Forms | [React Hook Form](https://react-hook-form.com/) + [Zod](https://zod.dev/) |
| Data Fetching | [TanStack Query v5](https://tanstack.com/query) |
| Icons | [Lucide React](https://lucide.dev/) |
| Toasts | [Sonner](https://sonner.emilkowal.ski/) |
| Build Tool | [Vite v8](https://vitejs.dev/) |
| Package Manager | [Bun](https://bun.sh/) |
| Linting | ESLint 9 (flat config) |
| Formatting | Prettier |

---

## 📁 Project Structure

```
frontend/
├── public/                   # Static assets (favicon, robots.txt)
├── src/
│   ├── components/
│   │   ├── auth/             # AuthLayout (login/signup pages wrapper)
│   │   ├── layout/           # AppSidebar, TopBar
│   │   ├── requests/         # RequestForm
│   │   ├── shared/           # Reusable components (StatCard, ReusableTable,
│   │   │                     #   PageHeader, SearchBar, badges, modals, etc.)
│   │   └── ui/               # shadcn/ui primitives (29 components)
│   ├── data/
│   │   └── mock.js           # In-memory mock data store (users, requests,
│   │                         #   assets, approvals, departments, etc.)
│   ├── hooks/
│   │   └── use-mobile.jsx    # Responsive breakpoint hook
│   ├── lib/
│   │   ├── auth.jsx          # AuthContext, AuthProvider, useAuth, Can
│   │   ├── theme.jsx         # ThemeProvider (light/dark)
│   │   ├── utils.js          # cn() utility (clsx + tailwind-merge)
│   │   ├── error-capture.js  # SSR error capture helper
│   │   └── error-page.js     # SSR error page renderer
│   ├── routes/               # File-based routes (TanStack Router)
│   │   ├── __root.jsx        # Root layout (providers, SEO head, 404/error)
│   │   ├── _shell.jsx        # Authenticated shell (sidebar + topbar layout)
│   │   ├── _shell.index.jsx  # Dashboard (/)
│   │   ├── _shell.requests.* # Requests list, detail, edit, new
│   │   ├── _shell.approvals.jsx
│   │   ├── _shell.assets.*   # Assets list & detail
│   │   ├── _shell.users.*    # Users list & profile
│   │   ├── _shell.reports.jsx
│   │   ├── _shell.notifications.jsx
│   │   ├── _shell.settings.jsx
│   │   ├── _shell.help.jsx
│   │   ├── _shell.masters.jsx
│   │   ├── login.jsx
│   │   ├── signup.jsx
│   │   ├── forgot-password.jsx
│   │   ├── reset-password.jsx
│   │   └── sitemap[.]xml.js
│   ├── routeTree.gen.ts      # Auto-generated route tree (do not edit)
│   ├── router.jsx            # Router instance setup
│   ├── server.js             # SSR server entry
│   ├── start.js              # App entry point
│   └── styles.css            # Global CSS & Tailwind theme tokens
├── components.json           # shadcn/ui configuration
├── vite.config.js            # Vite + TanStack Start build config
├── eslint.config.js          # ESLint flat config
├── .prettierrc               # Prettier settings
├── bunfig.toml               # Bun package manager config
└── package.json
```

---

## 🚀 Getting Started

### Prerequisites

- **[Bun](https://bun.sh/)** >= 1.0 — used as the package manager and runtime

  ```bash
  # Install Bun on Windows (if not already installed)
  powershell -c "irm bun.sh/install.ps1 | iex"
  ```

### Installation

```bash
# Navigate to the frontend directory
cd "Service Request Management System/frontend"

# Install dependencies
bun install
```

### Development

```bash
bun run dev
```

The app will be available at **http://localhost:3000** (or the port shown in the terminal).

### Build

```bash
# Production build
bun run build

# Development build (with source maps)
bun run build:dev

# Preview the production build locally
bun run preview
```

### Linting & Formatting

```bash
# Run ESLint
bun run lint

# Format all files with Prettier
bun run format
```

---

## 🔐 Roles & Demo Accounts

The app uses an in-memory mock data store — no backend is required. Use these credentials on the login page:

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@gmail.com` | `admin123` |
| **HOD** | `hod@gmail.com` | `hod123` |
| **Technician** | `tech@gmail.com` | `tech123` |
| **Requestor** | `requestor@gmail.com` | `requestor123` |

You can also use the **quick-login role switcher** on the login page to sign in without typing credentials.

### Role Permissions Summary

| Permission | Admin | HOD | Technician | Requestor |
|---|:---:|:---:|:---:|:---:|
| Dashboard | ✅ | ✅ | ✅ | ✅ |
| View Requests | ✅ | ✅ | ✅ | ✅ |
| Create Requests | ✅ | — | — | ✅ |
| Edit Requests | ✅ | ✅ | ✅ | — |
| Delete Requests | ✅ | — | — | — |
| Approvals | ✅ | — | — | — |
| Assets | ✅ | — | ✅ | — |
| Manage Users | ✅ | — | — | — |
| Reports | ✅ | ✅ | — | — |
| Masters (Config) | ✅ | — | — | — |

---

## 🎨 Design System

- **Fonts**: `Manrope` (body) · `Space Grotesk` (display headings)
- **Theme**: Fully themeable via CSS custom properties in `src/styles.css`
- **UI**: Built on shadcn/ui with 29 Radix UI-powered components
- **Animations**: Page transitions and micro-interactions powered by Motion
- **Responsive**: Mobile-first layout; sidebar collapses on small screens via `useIsMobile`

---

## 📝 Notes

- **Mock Data**: All data (users, requests, assets, etc.) lives in `src/data/mock.js` and is persisted to `localStorage`. Clearing browser storage resets all data to seeded defaults.
- **No Backend Required**: The frontend is fully self-contained and runs without an API server.
- **Route Tree**: `src/routeTree.gen.ts` is auto-generated by the TanStack Router Vite plugin on every `dev`/`build` run — do **not** edit it manually.
- **Session Storage**: Auth sessions are stored in `localStorage` (remember me) or `sessionStorage` (session only), keyed as `servicedesk.auth`.
