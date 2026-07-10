import { Link } from "@tanstack/react-router";
import { useState } from "react";
import {
  Ticket,
  ShieldCheck,
  Wrench,
  Building,
  Users,
  Play,
  ChevronDown,
  Check,
  ArrowRight,
  Laptop,
  Activity,
  Lock,
  HelpCircle,
  Clock,
  Sparkles,
  Award,
  Terminal,
  MousePointerClick,
  Layers,
  ChevronRight,
  Heart,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function LandingPage() {
  const [activeFaq, setActiveFaq] = useState(null);

  // Smooth scroll handler
  const scrollToSection = (id) => {
    const element = document.getElementById(id);
    if (element) {
      element.scrollIntoView({ behavior: "smooth" });
    }
  };

  const features = [
    { icon: Ticket, title: "Raise Service Requests", desc: "Easily submit and catalog IT, facility, or administrative request tickets." },
    { icon: Clock, title: "Track Request Status", desc: "Real-time timeline tracking from creation, technician pickup, to closure." },
    { icon: Building, title: "Department Management", desc: "Organize tickets, assets, and members by custom corporate departments." },
    { icon: ShieldCheck, title: "Role-Based Access Control", desc: "Granular access permissions for Admins, HODs, Technicians, and Requestors." },
    { icon: Wrench, title: "Technician Assignment", desc: "Smart assignment mappings based on request category and desk expertise." },
    { icon: Award, title: "Approval Workflows", desc: "Automated routing of technical or resource requests to department HODs." },
    { icon: Sparkles, title: "Real-Time Notifications", desc: "Instant visual and email alerts regarding SLA breaches and ticket updates." },
    { icon: Activity, title: "Analytics Dashboard", desc: "Beautiful visualizations of monthly load, team capacity, and resolution trends." },
    { icon: Lock, title: "Secure Authentication", desc: "State-of-the-art credential encryption and session metadata validation." },
    { icon: Layers, title: "Comprehensive History", desc: "Complete historical log of all resolved and archived service requests." },
  ];

  const steps = [
    { step: "01", title: "User Creates Request", desc: "Requestor submits details, department, and priority level." },
    { step: "02", title: "System Routes Request", desc: "Request is categorized and sent to the respective department queue." },
    { step: "03", title: "HOD Reviews (If Required)", desc: "Department head approves resource allocation or access requests." },
    { step: "04", title: "Technician Assigned", desc: "An expert technician is assigned manually or via auto-mapping." },
    { step: "05", title: "Work In Progress", desc: "Technician resolves the issue and posts replies for updates." },
    { step: "06", title: "Request Completed", desc: "Technician marks the ticket resolved after verifying functionality." },
    { step: "07", title: "Verification & Closure", desc: "Requestor closes the ticket or provides satisfaction feedback." },
  ];

  const modules = [
    {
      role: "Admin",
      color: "from-primary/20 to-primary/5 border-primary/20 text-primary",
      desc: "Central controller of the entire support ecosystem.",
      features: ["Manage users & authorization", "Configure department nodes", "View SLA & monthly reports", "Configure system masters"],
    },
    {
      role: "HOD",
      color: "from-info/20 to-info/5 border-info/20 text-info",
      desc: "Manages department resources and reviews allocations.",
      features: ["Approve or reject requests", "Assign department technicians", "Monitor department workloads", "Review operational health"],
    },
    {
      role: "Technician",
      color: "from-pink-500/20 to-pink-500/5 border-pink-500/25 text-pink-500",
      desc: "Desk experts resolving assigned service requests.",
      features: ["View active workloads", "Update request resolution status", "Post technical update replies", "Review asset history"],
    },
    {
      role: "Requestor",
      color: "from-teal-500/20 to-teal-500/5 border-teal-500/25 text-teal-500",
      desc: "End users submitting and tracking requests.",
      features: ["Create service request tickets", "Track real-time resolution", "Provide closure feedback", "View historical requests"],
    },
  ];

  const benefits = [
    { title: "Fast Request Processing", desc: "Minimize turnaround times with automated routing and technician notifications." },
    { title: "Transparent Workflows", desc: "End-to-end audit trails ensure complete visibility of ticket ownership." },
    { title: "Better Communication", desc: "Discuss issues directly inside the ticket with threaded updates." },
    { title: "Reduced Paperwork", desc: "Banish spreadsheets and physical forms with a unified digital database." },
    { title: "Centralized Management", desc: "One platform to manage service requests, assets, users, and masters." },
    { title: "Improved Productivity", desc: "Clear tasks and SLA tracking keep technicians focused and efficient." },
  ];

  const stats = [
    { value: "500+", label: "Requests Processed" },
    { value: "20+", label: "Departments" },
    { value: "150+", label: "Employees" },
    { value: "98%", label: "Resolution Rate" },
  ];

  const testimonials = [
    {
      quote: "Implementing this system cut our support response times in half. Technicians get tasks instantly, and users love the transparent tracking progress.",
      author: "Aditi Rao",
      role: "Head of IT Operations",
    },
    {
      quote: "Extremely intuitive. Managing hardware requests and HOD approvals is now fully automated. We saved hours of back-and-forth emails.",
      author: "Vikram Malhotra",
      role: "Operations Manager",
    },
    {
      quote: "The interface is gorgeous, and it works perfectly on my tablet. I can resolve requests on the go with zero friction.",
      author: "Sanjay Sen",
      role: "Senior Desktop Support Technician",
    },
  ];

  const faqs = [
    {
      q: "How do I create a new service request?",
      a: "Simply sign up and log in as a Requestor. Navigate to the Service Requests section, click 'New Request', fill out the department, priority, and ticket details, and submit. You will receive a unique tracking ID instantly.",
    },
    {
      q: "Who approves my service request?",
      a: "Requests that require special software licenses, access permissions, or physical assets are automatically routed to your department's HOD (Head of Department) for approval before being assigned to a technician.",
    },
    {
      q: "How can I track the progress of my request?",
      a: "Once logged in, click on your request from the Service Requests list. You will see a detailed visual timeline mapping out when it was raised, when it was assigned, comments posted by your technician, and its current state.",
    },
    {
      q: "Can technicians update status and reply directly?",
      a: "Yes! Technicians have a dedicated panel where they can see their assigned tasks, update the status (e.g. In Progress, Completed), and post replies or attach logs directly to the request timeline to communicate with you.",
    },
  ];

  return (
    <div className="min-h-screen bg-background text-foreground selection:bg-primary selection:text-primary-foreground font-sans">
      {/* Sticky Landing Header */}
      <header className="sticky top-0 z-50 flex h-16 items-center justify-between border-b bg-background/85 px-4 backdrop-blur-md md:px-8">
        <div className="flex items-center gap-2.5">
          <div className="grid size-9 place-items-center rounded-xl bg-primary text-primary-foreground shadow-md shadow-primary/10">
            <Laptop className="size-4.5" />
          </div>
          <span className="font-display text-sm font-black tracking-tight bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
            ServiceDesk
          </span>
        </div>

        <nav className="hidden items-center gap-6 text-xs font-semibold text-muted-foreground md:flex">
          <button onClick={() => scrollToSection("about")} className="hover:text-primary transition-colors cursor-pointer">About</button>
          <button onClick={() => scrollToSection("features")} className="hover:text-primary transition-colors cursor-pointer">Features</button>
          <button onClick={() => scrollToSection("works")} className="hover:text-primary transition-colors cursor-pointer">Workflow</button>
          <button onClick={() => scrollToSection("modules")} className="hover:text-primary transition-colors cursor-pointer">Modules</button>
          <button onClick={() => scrollToSection("faqs")} className="hover:text-primary transition-colors cursor-pointer">FAQs</button>
        </nav>

        <div className="flex items-center gap-2.5">
          <Button asChild variant="ghost" className="rounded-xl h-9 px-4 text-xs font-bold transition-all hover:bg-accent cursor-pointer">
            <Link to="/login">Login</Link>
          </Button>
          <Button asChild className="rounded-xl h-9 px-4.5 text-xs font-black shadow-sm shadow-primary/15 cursor-pointer">
            <Link to="/signup">Sign Up</Link>
          </Button>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative flex flex-col items-center justify-center py-20 px-4 text-center overflow-hidden border-b border-border/40">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary/5 via-transparent to-transparent -z-10" />
        <div className="max-w-3xl space-y-6">
          <div className="inline-flex items-center gap-1.5 rounded-full bg-primary/5 border border-primary/15 px-3.5 py-1 text-[10px] font-bold text-primary tracking-wide uppercase">
            <MousePointerClick className="size-3" /> Digital Service Desk Platform
          </div>
          <h1 className="text-4xl sm:text-5xl md:text-6xl font-black font-display tracking-tight leading-[1.1] text-slate-800 dark:text-slate-100">
            IT Service Request <br className="hidden sm:inline" />
            <span className="bg-gradient-to-r from-primary via-primary/95 to-indigo-500 bg-clip-text text-transparent">
              Management System
            </span>
          </h1>
          <p className="text-sm sm:text-base md:text-lg font-bold text-slate-600 dark:text-slate-400">
            "Smart, Fast and Efficient Service Request Management Platform"
          </p>
          <p className="max-w-xl mx-auto text-xs sm:text-sm text-muted-foreground leading-relaxed font-medium">
            Empower your team with a centralized portal to raise request tickets, track real-time resolution timelines, handle HOD approvals, and manage corporate assets seamlessly.
          </p>

          <div className="pt-4 flex flex-col sm:flex-row items-center justify-center gap-3">
            <Button asChild size="lg" className="rounded-2xl h-11 px-6 font-extrabold text-sm w-full sm:w-auto shadow-md shadow-primary/20 cursor-pointer">
              <Link to="/login" className="flex items-center gap-2">
                Get Started Now <ArrowRight className="size-4" />
              </Link>
            </Button>
            <Button
              onClick={() => scrollToSection("about")}
              variant="outline"
              size="lg"
              className="rounded-2xl h-11 px-6 font-bold text-sm w-full sm:w-auto bg-card/20 border-border/70 cursor-pointer"
            >
              Learn More
            </Button>
          </div>
        </div>

        <button
          onClick={() => scrollToSection("about")}
          className="mt-16 text-muted-foreground/60 hover:text-primary transition-colors cursor-pointer animate-bounce flex flex-col items-center gap-1"
        >
          <span className="text-[10px] font-bold uppercase tracking-wider">Scroll Details</span>
          <ChevronDown className="size-4" />
        </button>
      </section>

      {/* About Section */}
      <section id="about" className="py-20 px-4 max-w-5xl mx-auto border-b border-border/40 space-y-12">
        <div className="text-center max-w-2xl mx-auto space-y-3">
          <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
            About the System
          </h2>
          <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
          <p className="text-xs sm:text-sm text-muted-foreground font-medium">
            Modernizing how enterprise support request operations handle user issues.
          </p>
        </div>

        <div className="grid gap-8 md:grid-cols-2 pt-4">
          <div className="space-y-4">
            <h3 className="text-base sm:text-lg font-extrabold text-slate-800 dark:text-slate-200">
              What is a Service Request Management System?
            </h3>
            <p className="text-xs sm:text-sm text-muted-foreground leading-relaxed font-medium">
              A Service Request Management System (SRMS) is a centralized, digital workspace designed to handle, process, and track service requests raised by members of an organization. From technical bugs and software license provisioning to facility repairs and hardware assignments, the platform aligns all requests into automated channels.
            </p>
            <p className="text-xs sm:text-sm text-muted-foreground leading-relaxed font-medium">
              By replacing unstructured email chains, chats, and spreadsheets with an active queue, the system coordinates actions between requestors, managers, and support technicians.
            </p>
          </div>

          <div className="space-y-4">
            <h3 className="text-base sm:text-lg font-extrabold text-slate-800 dark:text-slate-200">
              Why Organizations Need It
            </h3>
            <p className="text-xs sm:text-sm text-muted-foreground leading-relaxed font-medium">
              Manual service workflows lead to delays, SLA breaches, and poor employee productivity. Organizations need a systemized platform to establish transparent lines of communication, monitor support team capacity, map automated technician routing, and track HOD approvals for physical assets.
            </p>
            <div className="space-y-2 pt-1.5">
              {[
                "Simplifies request handling through status tracking",
                "Ensures real-time visibility with timeline updates",
                "Establishes role-based permissions and secure sessions",
                "Reduces paperwork with central asset and user databases",
              ].map((benefit) => (
                <div key={benefit} className="flex items-center gap-2 text-xs font-bold text-slate-700 dark:text-slate-300">
                  <Check className="size-4 text-primary shrink-0" />
                  <span>{benefit}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Key Features Section */}
      <section id="features" className="py-20 px-4 bg-muted/20 border-b border-border/40">
        <div className="max-w-5xl mx-auto space-y-12">
          <div className="text-center max-w-2xl mx-auto space-y-3">
            <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
              Key Features
            </h2>
            <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
            <p className="text-xs sm:text-sm text-muted-foreground font-medium">
              Uncompromising capabilities built to power enterprise service workflows.
            </p>
          </div>

          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 pt-4">
            {features.map((feat) => {
              const Icon = feat.icon;
              return (
                <div
                  key={feat.title}
                  className="group rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-5 hover:border-primary/50 transition-all duration-300 hover:shadow-card shadow-sm"
                >
                  <div className="grid size-10 place-items-center rounded-xl bg-primary/10 text-primary border border-primary/5 group-hover:scale-105 transition-transform duration-300">
                    <Icon className="size-5" />
                  </div>
                  <h3 className="mt-4 text-sm font-extrabold text-slate-800 dark:text-slate-150">
                    {feat.title}
                  </h3>
                  <p className="mt-1.5 text-xs text-muted-foreground font-semibold leading-relaxed">
                    {feat.desc}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* How it Works Section */}
      <section id="works" className="py-20 px-4 max-w-5xl mx-auto border-b border-border/40 space-y-12">
        <div className="text-center max-w-2xl mx-auto space-y-3">
          <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
            How The System Works
          </h2>
          <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
          <p className="text-xs sm:text-sm text-muted-foreground font-medium">
            From creation to closure: a transparent and streamlined request cycle.
          </p>
        </div>

        {/* Timeline Component */}
        <div className="relative pt-6 max-w-lg mx-auto">
          {/* Vertical line */}
          <div className="absolute left-6 top-6 bottom-6 w-0.5 bg-border/60" />

          <div className="space-y-8">
            {steps.map((st) => (
              <div key={st.step} className="relative flex gap-5 items-start pl-12 group">
                {/* Timeline node */}
                <div className="absolute left-2.5 top-0.5 grid size-7 place-items-center rounded-full bg-background border border-primary/40 text-primary text-[10px] font-black group-hover:bg-primary group-hover:text-primary-foreground group-hover:scale-105 transition-all duration-300">
                  {st.step}
                </div>
                <div className="space-y-0.5">
                  <h3 className="text-xs sm:text-sm font-extrabold text-slate-800 dark:text-slate-200">
                    {st.title}
                  </h3>
                  <p className="text-xs text-muted-foreground font-medium leading-relaxed">
                    {st.desc}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* System Modules Section */}
      <section id="modules" className="py-20 px-4 bg-muted/20 border-b border-border/40">
        <div className="max-w-5xl mx-auto space-y-12">
          <div className="text-center max-w-2xl mx-auto space-y-3">
            <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
              System Modules
            </h2>
            <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
            <p className="text-xs sm:text-sm text-muted-foreground font-medium">
              Tailored workspaces specifically designed for each user role.
            </p>
          </div>

          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4 pt-4">
            {modules.map((mod) => (
              <div
                key={mod.role}
                className={cn(
                  "rounded-2xl border bg-gradient-to-b p-5 flex flex-col justify-between shadow-sm",
                  mod.color,
                )}
              >
                <div className="space-y-3">
                  <h3 className="font-display text-lg font-black">{mod.role}</h3>
                  <p className="text-xs font-semibold text-muted-foreground leading-relaxed">
                    {mod.desc}
                  </p>
                  <div className="h-px bg-border/40 w-full" />
                  <ul className="space-y-2.5 pt-1.5">
                    {mod.features.map((feat) => (
                      <li key={feat} className="flex gap-2 items-start text-xs font-semibold text-slate-700 dark:text-slate-350">
                        <Check className="size-3.5 mt-0.5 shrink-0" />
                        <span>{feat}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Why Choose Us Section */}
      <section className="py-20 px-4 max-w-5xl mx-auto border-b border-border/40 space-y-12">
        <div className="text-center max-w-2xl mx-auto space-y-3">
          <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
            Why Choose This System
          </h2>
          <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
          <p className="text-xs sm:text-sm text-muted-foreground font-medium">
            Discover the direct benefits of deploying a digital request management ecosystem.
          </p>
        </div>

        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 pt-4">
          {benefits.map((ben) => (
            <div key={ben.title} className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-5 space-y-2 shadow-sm">
              <h3 className="text-xs sm:text-sm font-extrabold text-slate-800 dark:text-slate-200">
                {ben.title}
              </h3>
              <p className="text-xs text-muted-foreground font-medium leading-relaxed">
                {ben.desc}
              </p>
            </div>
          ))}
        </div>
      </section>

      {/* Statistics Section */}
      <section className="py-16 px-4 bg-primary text-primary-foreground border-b border-primary/10">
        <div className="max-w-5xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          {stats.map((stat) => (
            <div key={stat.label} className="space-y-1">
              <p className="text-3xl sm:text-4xl font-black font-display tracking-tight">
                {stat.value}
              </p>
              <p className="text-[10px] sm:text-xs font-bold uppercase tracking-wider text-primary-foreground/80">
                {stat.label}
              </p>
            </div>
          ))}
        </div>
      </section>

      {/* Testimonials Section */}
      <section className="py-20 px-4 max-w-5xl mx-auto border-b border-border/40 space-y-12">
        <div className="text-center max-w-2xl mx-auto space-y-3">
          <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
            User Testimonials
          </h2>
          <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
          <p className="text-xs sm:text-sm text-muted-foreground font-medium">
            Hear from operations, technicians, and employees using the support desk.
          </p>
        </div>

        <div className="grid gap-6 md:grid-cols-3 pt-4">
          {testimonials.map((test) => (
            <div
              key={test.author}
              className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-5 flex flex-col justify-between shadow-sm relative"
            >
              <p className="text-xs sm:text-sm italic text-slate-600 dark:text-slate-350 leading-relaxed font-semibold">
                "{test.quote}"
              </p>
              <div className="mt-4 pt-3 border-t border-border/25">
                <p className="text-xs font-extrabold text-slate-850 dark:text-slate-100">
                  {test.author}
                </p>
                <p className="text-[10px] font-bold text-muted-foreground/80 uppercase tracking-wider mt-0.5">
                  {test.role}
                </p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* FAQ Section */}
      <section id="faqs" className="py-20 px-4 bg-muted/20 border-b border-border/40">
        <div className="max-w-3xl mx-auto space-y-12">
          <div className="text-center max-w-2xl mx-auto space-y-3">
            <h2 className="font-display text-2xl sm:text-3xl font-black text-slate-800 dark:text-slate-100">
              Frequently Asked Questions
            </h2>
            <div className="h-1 w-12 bg-primary mx-auto rounded-full" />
            <p className="text-xs sm:text-sm text-muted-foreground font-medium">
              Common questions regarding ticket creation, approval, and routing.
            </p>
          </div>

          <div className="space-y-4 pt-4">
            {faqs.map((faq, index) => {
              const active = activeFaq === index;
              return (
                <div
                  key={index}
                  className="rounded-xl border border-border/50 bg-card/60 overflow-hidden transition-all shadow-sm"
                >
                  <button
                    onClick={() => setActiveFaq(active ? null : index)}
                    className="w-full flex items-center justify-between gap-4 p-4 text-left font-bold text-xs sm:text-sm text-slate-800 dark:text-slate-100 hover:text-primary transition-colors cursor-pointer"
                  >
                    <span>{faq.q}</span>
                    <ChevronRight className={cn("size-4 shrink-0 transition-transform duration-300", active && "rotate-95")} />
                  </button>
                  {active && (
                    <div className="p-4 pt-0 border-t border-border/25 text-xs text-muted-foreground font-semibold leading-relaxed">
                      {faq.a}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Call To Action */}
      <section className="py-20 px-4 text-center border-b border-border/40 bg-[radial-gradient(ellipse_at_bottom,_var(--tw-gradient-stops))] from-primary/5 via-transparent to-transparent">
        <div className="max-w-2xl mx-auto space-y-6">
          <h2 className="text-3xl font-black font-display tracking-tight text-slate-800 dark:text-slate-100">
            Ready to Streamline Your IT Workload?
          </h2>
          <p className="text-xs sm:text-sm text-muted-foreground max-w-lg mx-auto leading-relaxed font-medium">
            Join other departments resolving hardware, software, and administrative requests. Sign in to start managing and tracking request queues.
          </p>
          <div className="pt-2 flex flex-col sm:flex-row items-center justify-center gap-3">
            <Button asChild size="lg" className="rounded-2xl h-11 px-7 font-extrabold text-sm w-full sm:w-auto shadow-md shadow-primary/20 cursor-pointer">
              <Link to="/login">Sign In to Dashboard</Link>
            </Button>
            <Button asChild variant="outline" size="lg" className="rounded-2xl h-11 px-7 font-bold text-sm w-full sm:w-auto bg-card/20 border-border/70 cursor-pointer">
              <Link to="/signup">Register Account</Link>
            </Button>
          </div>
        </div>
      </section>

      {/* Footer Section */}
      <footer className="py-12 px-4 md:px-8 border-t bg-muted/10">
        <div className="max-w-5xl mx-auto flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2">
            <div className="grid size-8 place-items-center rounded-lg bg-primary/10 text-primary">
              <Laptop className="size-4" />
            </div>
            <span className="font-display text-xs font-black tracking-tight text-foreground">
              IT ServiceDesk
            </span>
          </div>

          <div className="flex flex-wrap items-center justify-center gap-6 text-xs text-muted-foreground font-semibold">
            <button onClick={() => scrollToSection("about")} className="hover:text-primary transition-colors cursor-pointer">About</button>
            <button onClick={() => scrollToSection("features")} className="hover:text-primary transition-colors cursor-pointer">Features</button>
            <button onClick={() => scrollToSection("works")} className="hover:text-primary transition-colors cursor-pointer">Workflow</button>
            <Link to="/login" className="hover:text-primary transition-colors">Login</Link>
            <Link to="/signup" className="hover:text-primary transition-colors">Register</Link>
          </div>

          <div className="text-[11px] text-muted-foreground/75 font-semibold text-center md:text-right">
            <p>© {new Date().getFullYear()} IT ServiceDesk. All rights reserved.</p>
            <p className="mt-1 flex items-center justify-center md:justify-end gap-1.5">
              Made with <Heart className="size-3 text-destructive fill-destructive" /> for IT Service Request Management
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
