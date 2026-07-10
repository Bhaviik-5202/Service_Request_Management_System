import { createFileRoute } from "@tanstack/react-router";
import { useState, useEffect, useMemo } from "react";
import {
  User,
  Shield,
  Bell,
  Key,
  Mail,
  Phone,
  Building,
  Calendar,
  Lock,
  Camera,
  Activity,
  CheckCircle,
  Hash,
  Laptop,
  ArrowLeft,
} from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useAuth } from "@/lib/auth";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_shell/profile")({
  head: () => ({
    meta: [
      { title: "User Profile — ServiceDesk" },
      {
        name: "description",
        content: "View and manage your personal details, security settings, and account activities.",
      },
    ],
  }),
  component: ProfilePage,
});

function ProfilePage() {
  const { user, role, updateProfile } = useAuth();

  // Redirect or show nothing if user is not loaded
  if (!user) {
    return null;
  }

  // Get active profile values
  const activeProfile = useMemo(() => {
    return {
      name: user.name,
      email: user.email,
      role: user.role || role || "Requestor",
      department: user.department || "General",
      phone: user.phone || "+91 98200 11223",
      status: user.status || "Active",
      joined: user.joined || "2024-01-01",
      employeeId: user.employeeId || `EMP-${String(user.id).padStart(4, "0")}`,
      avatar: user.name
        ? user.name
            .split(" ")
            .map((n) => n[0])
            .join("")
            .slice(0, 2)
            .toUpperCase()
        : "US",
      lastLogin: user.lastLogin || new Date().toLocaleString("en-IN", {
        dateStyle: "medium",
        timeStyle: "short",
      }),
    };
  }, [user, role]);

  // Edit Mode state
  const [isEditing, setIsEditing] = useState(false);

  // Form inputs
  const [name, setName] = useState(activeProfile.name);
  const [email, setEmail] = useState(activeProfile.email);
  const [phone, setPhone] = useState(activeProfile.phone);

  // Password inputs
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // Sync state if user changes (e.g. role switch)
  useEffect(() => {
    setName(activeProfile.name);
    setEmail(activeProfile.email);
    setPhone(activeProfile.phone);
  }, [activeProfile]);

  // Recent activity list state
  const [activities, setActivities] = useState([
    { id: 1, action: "Logged In", desc: "Successfully signed into session", time: "10 mins ago", icon: Laptop, color: "text-blue-500 bg-blue-500/10" },
    { id: 2, action: "Updated Profile", desc: "Modified phone number & department details", time: "1 hour ago", icon: User, color: "text-teal-500 bg-teal-500/10" },
    { id: 3, action: "Created Request", desc: "Opened new ticket SR-2026-1041 (Laptop issue)", time: "2 hours ago", icon: Activity, color: "text-orange-500 bg-orange-500/10" },
    { id: 4, action: "Changed Password", desc: "Updated security credentials", time: "Yesterday", icon: Lock, color: "text-purple-500 bg-purple-500/10" },
    { id: 5, action: "Updated Request", desc: "Added remark on ticket SR-2026-1038", time: "3 days ago", icon: CheckCircle, color: "text-green-500 bg-green-500/10" },
  ]);

  // Save changes handler
  const handleSaveProfile = () => {
    if (!name.trim()) {
      toast.error("Name cannot be empty");
      return;
    }
    if (!email.trim() || !email.includes("@")) {
      toast.error("Please enter a valid email address");
      return;
    }

    updateProfile({
      name,
      email,
      phone,
    });

    // Add new activity
    setActivities((prev) => [
      {
        id: Date.now(),
        action: "Updated Profile",
        desc: "Modified personal information details",
        time: "Just now",
        icon: User,
        color: "text-teal-500 bg-teal-500/10",
      },
      ...prev,
    ]);

    setIsEditing(false);
    toast.success("Profile saved successfully");
  };

  // Cancel changes handler
  const handleCancelProfile = () => {
    setName(activeProfile.name);
    setEmail(activeProfile.email);
    setPhone(activeProfile.phone);
    setIsEditing(false);
    toast.info("Changes discarded");
  };

  // Change password handler
  const handleUpdatePassword = () => {
    if (!currentPassword) {
      toast.error("Please enter your current password");
      return;
    }
    if (newPassword.length < 6) {
      toast.error("New password must be at least 6 characters long");
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error("Passwords do not match");
      return;
    }

    // Persist password update
    updateProfile({
      password: newPassword,
    });

    // Add new activity
    setActivities((prev) => [
      {
        id: Date.now(),
        action: "Changed Password",
        desc: "Security credentials updated successfully",
        time: "Just now",
        icon: Lock,
        color: "text-purple-500 bg-purple-500/10",
      },
      ...prev,
    ]);

    setCurrentPassword("");
    setNewPassword("");
    setConfirmPassword("");
    toast.success("Password updated successfully");
  };

  // Profile image upload handler (mock)
  const handlePhotoUpload = () => {
    toast.success("Profile picture updated (Demo mode)");
    // Add new activity
    setActivities((prev) => [
      {
        id: Date.now(),
        action: "Uploaded Photo",
        desc: "Updated account profile picture",
        time: "Just now",
        icon: Camera,
        color: "text-pink-500 bg-pink-500/10",
      },
      ...prev,
    ]);
  };

  return (
    <div className="mx-auto max-w-5xl space-y-6 pb-12">
      <PageHeader
        title="Profile"
        description="View and manage your account settings, credentials, and activities"
        crumbs={[{ label: "Profile" }]}
      />

      <div className="grid gap-6 md:grid-cols-3">
        {/* Left Column: Large Profile Card & Account Info */}
        <div className="space-y-6 md:col-span-1">
          {/* Large Profile Card */}
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card flex flex-col items-center text-center relative overflow-hidden">
            <div className="absolute top-0 left-0 w-full h-20 bg-gradient-to-r from-primary/10 via-primary/5 to-transparent border-b border-primary/5" />
            
            <div className="relative mt-4">
              <Avatar className="size-24 border-4 border-card ring-4 ring-primary/10 shadow-lg">
                <AvatarFallback className="bg-primary text-3xl font-black text-primary-foreground">
                  {activeProfile.avatar}
                </AvatarFallback>
              </Avatar>
              <button
                onClick={handlePhotoUpload}
                className="absolute bottom-0 right-0 p-1.5 rounded-full bg-primary text-primary-foreground border-2 border-card shadow hover:scale-105 transition-transform cursor-pointer"
                title="Change Photo"
              >
                <Camera className="size-4" />
              </button>
            </div>

            <div className="mt-4 space-y-1">
              <h3 className="text-lg font-black text-slate-800 dark:text-slate-100">
                {activeProfile.name}
              </h3>
              <p className="text-xs text-muted-foreground font-semibold flex items-center justify-center gap-1">
                <Building className="size-3 text-primary" /> {activeProfile.department}
              </p>
              <div className="mt-2.5 flex items-center justify-center gap-2">
                <span className="text-[10px] font-bold text-primary bg-primary/5 dark:bg-primary/10 px-2.5 py-0.5 rounded-full border border-primary/10">
                  {activeProfile.role}
                </span>
                <span className="text-[10px] font-bold text-success bg-success/5 dark:bg-success/10 px-2.5 py-0.5 rounded-full border border-success/10">
                  {activeProfile.status}
                </span>
              </div>
            </div>

            <div className="mt-6 pt-4 border-t border-border/30 w-full text-left space-y-3.5 text-xs">
              <div className="flex justify-between font-semibold">
                <span className="text-muted-foreground">Joined Desk:</span>
                <span className="text-foreground">{activeProfile.joined}</span>
              </div>
              <div className="flex justify-between font-semibold">
                <span className="text-muted-foreground">ID Number:</span>
                <span className="text-foreground">{activeProfile.employeeId}</span>
              </div>
            </div>
          </div>

          {/* Account Information Card */}
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-4">
            <h4 className="text-[10px] font-black text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
              <Hash className="size-3.5 text-primary" /> Account Metadata
            </h4>
            
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-1 py-1 border-b border-border/20">
                <span className="text-xs font-bold text-muted-foreground">Employee ID</span>
                <span className="text-xs font-semibold text-right text-foreground">{activeProfile.employeeId}</span>
              </div>
              <div className="grid grid-cols-2 gap-1 py-1 border-b border-border/20">
                <span className="text-xs font-bold text-muted-foreground">Department</span>
                <span className="text-xs font-semibold text-right text-foreground">{activeProfile.department}</span>
              </div>
              <div className="grid grid-cols-2 gap-1 py-1 border-b border-border/20">
                <span className="text-xs font-bold text-muted-foreground">Status</span>
                <span className="text-xs font-semibold text-right text-success">{activeProfile.status}</span>
              </div>
              <div className="grid grid-cols-2 gap-1 py-1 border-b border-border/20">
                <span className="text-xs font-bold text-muted-foreground">Member Since</span>
                <span className="text-xs font-semibold text-right text-foreground">{activeProfile.joined}</span>
              </div>
              <div className="grid grid-cols-2 gap-1 py-1">
                <span className="text-xs font-bold text-muted-foreground">Last Login</span>
                <span className="text-xs font-semibold text-right text-foreground leading-normal">{activeProfile.lastLogin}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Right Column: Personal Info, Security & Activity */}
        <div className="space-y-6 md:col-span-2">
          {/* Personal Information Card */}
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-5">
            <div className="flex items-center justify-between">
              <h4 className="text-[10px] font-black text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
                <User className="size-3.5 text-primary" /> Personal Information
              </h4>
              {!isEditing ? (
                <Button
                  variant="outline"
                  size="sm"
                  className="rounded-xl h-8 text-xs font-bold px-3.5 cursor-pointer bg-muted/20 border-border/60 hover:bg-muted/40"
                  onClick={() => setIsEditing(true)}
                >
                  Edit Profile
                </Button>
              ) : (
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="rounded-xl h-8 text-xs font-bold px-3 cursor-pointer bg-muted/20 border-border/60"
                    onClick={handleCancelProfile}
                  >
                    Cancel
                  </Button>
                  <Button
                    size="sm"
                    className="rounded-xl h-8 text-xs font-bold px-3 cursor-pointer shadow-sm shadow-primary/15"
                    onClick={handleSaveProfile}
                  >
                    Save
                  </Button>
                </div>
              )}
            </div>

            <div className="grid gap-4.5 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="fullName" className="text-xs font-bold text-slate-700 dark:text-slate-300">
                  Full Name
                </Label>
                <Input
                  id="fullName"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  disabled={!isEditing}
                  className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-sm font-medium disabled:opacity-85 disabled:cursor-not-allowed"
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="emailAddress" className="text-xs font-bold text-slate-700 dark:text-slate-300">
                  Email Address
                </Label>
                <Input
                  id="emailAddress"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={!isEditing}
                  className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-sm font-medium disabled:opacity-85 disabled:cursor-not-allowed"
                />
              </div>

              <div className="space-y-1.5 sm:col-span-2">
                <Label htmlFor="mobileNumber" className="text-xs font-bold text-slate-700 dark:text-slate-300">
                  Mobile Number
                </Label>
                <Input
                  id="mobileNumber"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  disabled={!isEditing}
                  className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-sm font-medium disabled:opacity-85 disabled:cursor-not-allowed"
                />
              </div>
            </div>
          </div>

          {/* Security & Password Card */}
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-6">
            <h4 className="text-[10px] font-black text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
              <Key className="size-3.5 text-primary" /> Credentials & Security
            </h4>

            <div className="grid gap-6 sm:grid-cols-2">
              {/* Security info list */}
              <div className="space-y-4">
                <div className="rounded-xl border border-primary/10 bg-primary/5 dark:bg-primary/10 p-4 space-y-3 text-xs">
                  <div className="flex justify-between font-semibold">
                    <span className="text-muted-foreground">Security Role:</span>
                    <span className="text-primary font-bold">{activeProfile.role}</span>
                  </div>
                  <div className="flex justify-between font-semibold">
                    <span className="text-muted-foreground">Account Status:</span>
                    <span className="text-success font-bold">{activeProfile.status}</span>
                  </div>
                  <div className="flex justify-between font-semibold">
                    <span className="text-muted-foreground">Last Login IP:</span>
                    <span className="text-foreground font-bold">192.168.1.42</span>
                  </div>
                  <div className="flex justify-between font-semibold">
                    <span className="text-muted-foreground">Session Status:</span>
                    <span className="text-success font-bold flex items-center gap-1">
                      <span className="size-1.5 rounded-full bg-success animate-pulse" /> Active
                    </span>
                  </div>
                </div>
              </div>

              {/* Password update form */}
              <div className="space-y-4">
                <div className="space-y-1.5">
                  <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                    Current Password
                  </Label>
                  <Input
                    type="password"
                    placeholder="••••••••"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    className="h-9 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-xs"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                    New Password
                  </Label>
                  <Input
                    type="password"
                    placeholder="••••••••"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    className="h-9 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-xs"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                    Confirm New Password
                  </Label>
                  <Input
                    type="password"
                    placeholder="••••••••"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className="h-9 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary text-xs"
                  />
                </div>

                <div className="pt-2 flex justify-end">
                  <Button
                    size="sm"
                    className="rounded-xl h-9 text-xs font-bold px-4 cursor-pointer shadow-sm shadow-primary/10"
                    onClick={handleUpdatePassword}
                  >
                    Update Password
                  </Button>
                </div>
              </div>
            </div>
          </div>

          {/* Recent Activity Card */}
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-5">
            <h4 className="text-[10px] font-black text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
              <Activity className="size-3.5 text-primary" /> Recent Activity Log
            </h4>

            <div className="space-y-4">
              {activities.map((act) => {
                const Icon = act.icon;
                return (
                  <div key={act.id} className="flex gap-3 text-xs items-start">
                    <div className={cn("grid size-8 shrink-0 place-items-center rounded-lg border border-border/10", act.color)}>
                      <Icon className="size-4" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-extrabold text-foreground">{act.action}</p>
                      <p className="text-muted-foreground mt-0.5 font-medium">{act.desc}</p>
                    </div>
                    <span className="text-[10px] text-muted-foreground/60 font-semibold">{act.time}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
