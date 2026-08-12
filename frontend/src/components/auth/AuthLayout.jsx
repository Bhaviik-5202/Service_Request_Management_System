import { Link, useNavigate } from "@tanstack/react-router";
import {
  Headset,
  Eye,
  EyeOff,
  ArrowRight,
  Mail,
  Lock,
  User,
  Phone,
  Briefcase,
  Building2,
  CheckCircle2,
  Sparkles,
} from "lucide-react";
import { useForm } from "react-hook-form";
import { motion } from "motion/react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth";
import { cn } from "@/lib/utils";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { departments } from "@/data/mock";

const FEATURES = [
  { icon: CheckCircle2, text: "Track requests in real-time" },
  { icon: CheckCircle2, text: "Smart approval workflows" },
  { icon: CheckCircle2, text: "Analytics & SLA reporting" },
];

export function AuthLayout({ title, subtitle, children, mode = "login" }) {
  return (
    <div className="min-h-screen bg-background flex">
      {/* Left decorative panel */}
      <div className="relative hidden lg:flex lg:w-[52%] xl:w-[55%] flex-col overflow-hidden">
        {/* Gradient background */}
        <div className="absolute inset-0 bg-gradient-to-br from-[oklch(0.15_0.04_262)] via-[oklch(0.18_0.05_265)] to-[oklch(0.12_0.03_258)]" />

        {/* Animated orbs */}
        <div className="absolute top-[-10%] right-[-5%] size-[500px] rounded-full bg-[oklch(0.55_0.18_258)] opacity-[0.07] blur-[80px] animate-pulse" />
        <div className="absolute bottom-[-15%] left-[-10%] size-[600px] rounded-full bg-[oklch(0.62_0.17_258)] opacity-[0.06] blur-[100px] animate-pulse" style={{ animationDelay: "1.5s" }} />
        <div className="absolute top-[40%] left-[30%] size-[300px] rounded-full bg-[oklch(0.68_0.14_235)] opacity-[0.05] blur-[60px] animate-pulse" style={{ animationDelay: "3s" }} />

        {/* Grid pattern overlay */}
        <div
          className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage: `linear-gradient(oklch(0.9 0 0) 1px, transparent 1px), linear-gradient(90deg, oklch(0.9 0 0) 1px, transparent 1px)`,
            backgroundSize: "40px 40px",
          }}
        />

        {/* Content */}
        <div className="relative z-10 flex flex-col h-full p-10 xl:p-14">
          {/* Logo */}
          <div className="flex items-center gap-3">
            <div className="grid size-10 place-items-center rounded-xl bg-[oklch(0.55_0.18_258)] shadow-lg shadow-[oklch(0.55_0.18_258)/0.4]">
              <Headset className="size-5 text-white" />
            </div>
            <div>
              <p className="font-display text-base font-bold text-white">ServiceDesk</p>
              <p className="text-[11px] text-white/50">IT Request Management</p>
            </div>
          </div>

          {/* Main content */}
          <div className="flex-1 flex flex-col justify-center max-w-md">
            <motion.div
              initial={{ opacity: 0, y: 24 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.1 }}
            >
              <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-white/[0.06] border border-white/10 mb-6">
                <Sparkles className="size-3 text-[oklch(0.75_0.15_258)]" />
                <span className="text-xs font-medium text-white/70">Trusted by 500+ organizations</span>
              </div>

              <h2 className="font-display text-4xl xl:text-5xl font-bold leading-[1.15] text-white">
                Every request
                <br />
                <span className="bg-gradient-to-r from-[oklch(0.72_0.16_258)] to-[oklch(0.75_0.14_210)] bg-clip-text text-transparent">
                  tracked &amp; resolved.
                </span>
              </h2>

              <p className="mt-5 text-sm leading-relaxed text-white/55 max-w-sm">
                Raise, assign, approve and resolve service requests across IT, Maintenance and
                Housekeeping — with real-time dashboards for every team.
              </p>

              <div className="mt-8 space-y-3">
                {FEATURES.map((f, i) => (
                  <motion.div
                    key={f.text}
                    initial={{ opacity: 0, x: -16 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ duration: 0.4, delay: 0.3 + i * 0.1 }}
                    className="flex items-center gap-3"
                  >
                    <div className="grid size-5 shrink-0 place-items-center rounded-full bg-[oklch(0.55_0.18_258)]/30">
                      <f.icon className="size-3 text-[oklch(0.72_0.16_258)]" />
                    </div>
                    <span className="text-sm text-white/60">{f.text}</span>
                  </motion.div>
                ))}
              </div>
            </motion.div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-3 max-w-md">
            {[
              { v: "664+", l: "Requests resolved" },
              { v: "9.4h", l: "Avg. resolution" },
              { v: "98%", l: "SLA compliance" },
            ].map((s, i) => (
              <motion.div
                key={s.l}
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, delay: 0.6 + i * 0.1 }}
                className="rounded-2xl bg-white/[0.04] border border-white/[0.07] p-4 backdrop-blur-sm"
              >
                <p className="font-display text-xl font-bold text-white">{s.v}</p>
                <p className="mt-1 text-[11px] text-white/45">{s.l}</p>
              </motion.div>
            ))}
          </div>

          <p className="mt-6 text-[11px] text-white/30">
            © 2026 ServiceDesk. All rights reserved.
          </p>
        </div>
      </div>

      {/* Right form panel */}
      <div className="flex-1 flex flex-col items-center justify-center px-4 py-12 sm:px-8 bg-background">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.45 }}
          className="w-full max-w-[420px]"
        >
          {/* Mobile logo */}
          <div className="mb-8 flex items-center gap-2.5 lg:hidden">
            <div className="grid size-9 place-items-center rounded-xl bg-primary shadow-md shadow-primary/25">
              <Headset className="size-4.5 text-primary-foreground" />
            </div>
            <p className="font-display text-lg font-bold">ServiceDesk</p>
          </div>

          {/* Heading */}
          <div className="mb-8">
            <h1 className="text-2xl font-bold tracking-tight text-foreground">{title}</h1>
            <p className="mt-1.5 text-sm text-muted-foreground">{subtitle}</p>
          </div>

          {children}
        </motion.div>
      </div>
    </div>
  );
}

export function LoginForm() {
  const navigate = useNavigate();
  const { signIn } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [remember, setRemember] = useState(true);
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const onSubmit = async (data) => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 600));
    const success = signIn(data.email, data.password, remember);
    setIsLoading(false);

    if (success) {
      toast.success("Welcome back!", {
        description: "Successfully signed in to ServiceDesk.",
      });
      navigate({ to: "/", replace: true });
    } else {
      toast.error("Invalid credentials", {
        description: "Please check your email and password.",
      });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      {/* Email */}
      <div className="space-y-1.5">
        <Label htmlFor="login-email" className="text-sm font-medium">
          Work Email
        </Label>
        <div className="relative">
          <Mail className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="login-email"
            type="email"
            placeholder="you@company.com"
            className="h-11 rounded-xl pl-10 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("email", {
              required: "Email is required",
              pattern: {
                value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                message: "Invalid email format",
              },
            })}
          />
        </div>
        {errors.email && (
          <p className="text-xs text-destructive flex items-center gap-1 mt-1">
            {errors.email.message}
          </p>
        )}
      </div>

      {/* Password */}
      <div className="space-y-1.5">
        <div className="flex items-center justify-between">
          <Label htmlFor="login-password" className="text-sm font-medium">
            Password
          </Label>
          <Link
            to="/forgot-password"
            className="text-xs font-medium text-primary hover:text-primary/80 transition-colors"
          >
            Forgot password?
          </Link>
        </div>
        <div className="relative">
          <Lock className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="login-password"
            type={showPassword ? "text" : "password"}
            placeholder="Enter your password"
            className="h-11 rounded-xl pl-10 pr-11 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("password", { required: "Password is required" })}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
          >
            {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
          </button>
        </div>
        {errors.password && (
          <p className="text-xs text-destructive mt-1">{errors.password.message}</p>
        )}
      </div>

      {/* Remember me */}
      <div className="flex items-center gap-2.5">
        <Checkbox
          id="remember"
          checked={remember}
          onCheckedChange={(checked) => setRemember(!!checked)}
        />
        <Label
          htmlFor="remember"
          className="text-sm font-normal text-muted-foreground cursor-pointer select-none"
        >
          Keep me signed in
        </Label>
      </div>

      {/* Submit */}
      <Button
        type="submit"
        disabled={isLoading}
        className="h-11 w-full rounded-xl font-semibold gap-2 shadow-md shadow-primary/20 hover:shadow-lg hover:shadow-primary/25 transition-all cursor-pointer"
      >
        {isLoading ? (
          <span className="flex items-center gap-2">
            <span className="size-4 rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground animate-spin" />
            Signing in…
          </span>
        ) : (
          <span className="flex items-center gap-2">
            Sign in
            <ArrowRight className="size-4" />
          </span>
        )}
      </Button>

      <p className="text-center text-xs text-muted-foreground pt-1">
        Don't have an account?{" "}
        <Link to="/signup" className="font-semibold text-primary hover:underline">
          Create one
        </Link>
      </p>
    </form>
  );
}

export function SignupForm() {
  const navigate = useNavigate();
  const { signUp } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState("Requestor");
  const [dept, setDept] = useState(departments[0] || "IT");
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm();

  const password = watch("password");

  const onSubmit = async (data) => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 700));
    const success = signUp({
      name: data.name,
      email: data.email,
      phone: data.phone,
      password: data.password,
      role,
      department: dept,
      status: "Active",
    });
    setIsLoading(false);

    if (success) {
      toast.success("Account created!", {
        description: "Welcome to ServiceDesk. Redirecting to your dashboard…",
      });
      navigate({ to: "/", replace: true });
    } else {
      toast.error("Registration failed", {
        description: "A user with this email address already exists.",
      });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {/* Full Name */}
      <div className="space-y-1.5">
        <Label htmlFor="signup-name" className="text-sm font-medium">
          Full Name
        </Label>
        <div className="relative">
          <User className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="signup-name"
            type="text"
            placeholder="e.g. Priya Sharma"
            className="h-11 rounded-xl pl-10 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("name", { required: "Name is required" })}
          />
        </div>
        {errors.name && (
          <p className="text-xs text-destructive mt-1">{errors.name.message}</p>
        )}
      </div>

      {/* Email */}
      <div className="space-y-1.5">
        <Label htmlFor="signup-email" className="text-sm font-medium">
          Email Address
        </Label>
        <div className="relative">
          <Mail className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="signup-email"
            type="email"
            placeholder="you@company.com"
            className="h-11 rounded-xl pl-10 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("email", {
              required: "Email is required",
              pattern: {
                value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                message: "Invalid email format",
              },
            })}
          />
        </div>
        {errors.email && (
          <p className="text-xs text-destructive mt-1">{errors.email.message}</p>
        )}
      </div>

      {/* Phone */}
      <div className="space-y-1.5">
        <Label htmlFor="signup-phone" className="text-sm font-medium">
          Phone Number
        </Label>
        <div className="relative">
          <Phone className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="signup-phone"
            type="text"
            placeholder="+91 98765 43210"
            className="h-11 rounded-xl pl-10 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("phone", { required: "Phone number is required" })}
          />
        </div>
        {errors.phone && (
          <p className="text-xs text-destructive mt-1">{errors.phone.message}</p>
        )}
      </div>

      {/* Role + Department */}
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label className="text-sm font-medium flex items-center gap-1.5">
            <Briefcase className="size-3.5 text-muted-foreground" />
            Role
          </Label>
          <Select value={role} onValueChange={(val) => setRole(val)}>
            <SelectTrigger className="h-11 rounded-xl bg-muted/40 border-border/60">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {["Admin", "HOD", "Technician", "Requestor"].map((r) => (
                <SelectItem key={r} value={r}>
                  {r}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label className="text-sm font-medium flex items-center gap-1.5">
            <Building2 className="size-3.5 text-muted-foreground" />
            Department
          </Label>
          <Select value={dept} onValueChange={setDept}>
            <SelectTrigger className="h-11 rounded-xl bg-muted/40 border-border/60">
              <SelectValue placeholder="Select dept." />
            </SelectTrigger>
            <SelectContent>
              {departments.map((d) => (
                <SelectItem key={d} value={d}>
                  {d}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Password */}
      <div className="space-y-1.5">
        <Label htmlFor="signup-password" className="text-sm font-medium">
          Password
        </Label>
        <div className="relative">
          <Lock className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="signup-password"
            type={showPassword ? "text" : "password"}
            placeholder="Min. 6 characters"
            className="h-11 rounded-xl pl-10 pr-11 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("password", {
              required: "Password is required",
              minLength: { value: 6, message: "Minimum 6 characters" },
            })}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
          >
            {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
          </button>
        </div>
        {errors.password && (
          <p className="text-xs text-destructive mt-1">{errors.password.message}</p>
        )}
      </div>

      {/* Confirm Password */}
      <div className="space-y-1.5">
        <Label htmlFor="signup-confirm" className="text-sm font-medium">
          Confirm Password
        </Label>
        <div className="relative">
          <Lock className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground pointer-events-none" />
          <Input
            id="signup-confirm"
            type="password"
            placeholder="Re-enter your password"
            className="h-11 rounded-xl pl-10 bg-muted/40 border-border/60 focus:bg-background transition-colors"
            {...register("confirm", {
              required: "Please confirm your password",
              validate: (v) => v === password || "Passwords do not match",
            })}
          />
        </div>
        {errors.confirm && (
          <p className="text-xs text-destructive mt-1">{errors.confirm.message}</p>
        )}
      </div>

      {/* Submit */}
      <Button
        type="submit"
        disabled={isLoading}
        className="h-11 w-full rounded-xl font-semibold gap-2 shadow-md shadow-primary/20 hover:shadow-lg hover:shadow-primary/25 transition-all cursor-pointer mt-1"
      >
        {isLoading ? (
          <span className="flex items-center gap-2">
            <span className="size-4 rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground animate-spin" />
            Creating account…
          </span>
        ) : (
          <span className="flex items-center gap-2">
            Create Account
            <ArrowRight className="size-4" />
          </span>
        )}
      </Button>

      <p className="text-center text-xs text-muted-foreground pt-1">
        Already have an account?{" "}
        <Link to="/login" className="font-semibold text-primary hover:underline">
          Sign in
        </Link>
      </p>
    </form>
  );
}
