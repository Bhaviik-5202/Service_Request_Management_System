import { createFileRoute } from "@tanstack/react-router";
import {
  Moon,
  Sun,
  User,
  Shield,
  Palette,
  Bell,
  Key,
  Mail,
  AlertTriangle,
  Boxes,
  CheckSquare,
  Loader2,
} from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { useTheme } from "@/lib/theme";
import { useAuth } from "@/lib/auth";
import { getUserSettingsByUserId, updateUserSettings, updateUser } from "@/services/api";
import { useState, useEffect } from "react";
import { cn } from "@/lib/utils";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/settings")({
  component: SettingsPage,
});

function SettingsPage() {
  const { theme, setTheme } = useTheme();
  const { user, updateProfile } = useAuth();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [twoFactor, setTwoFactor] = useState(false);
  const [requestUpdates, setRequestUpdates] = useState(true);
  const [approvalAlerts, setApprovalAlerts] = useState(true);
  const [slaWarnings, setSlaWarnings] = useState(true);
  const [assetEvents, setAssetEvents] = useState(false);
  const [emailDigest, setEmailDigest] = useState(false);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    async function loadSettings() {
      if (!user?.userId) {
        setLoading(false);
        return;
      }
      try {
        const s = await getUserSettingsByUserId(user.userId).catch(() => null);
        if (s) {
          setTwoFactor(s.twoFactorEnabled || false);
          setRequestUpdates(s.requestUpdates || true);
          setApprovalAlerts(s.approvalAlerts || true);
          setSlaWarnings(s.slaWarnings || true);
          setAssetEvents(s.assetEvents || false);
          setEmailDigest(s.emailDigest || false);
          if (s.themePreference) setTheme(s.themePreference);
        }
      } catch (err) {
      } finally {
        setLoading(false);
      }
    }
    loadSettings();
  }, [user]);

  const handleUpdatePassword = async () => {
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

    setSaving(true);
    try {
      await updateProfile({ password: newPassword });
      toast.success("Password updated successfully!");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      toast.error(err.message || "Failed to update password.");
    } finally {
      setSaving(false);
    }
  };

  const handleSavePreferences = async (newPrefs = {}) => {
    if (!user?.userId) return;
    try {
      await updateUserSettings(user.userId, {
        userId: user.userId,
        themePreference: newPrefs.theme || theme,
        twoFactorEnabled: newPrefs.twoFactor ?? twoFactor,
        requestUpdates: newPrefs.requestUpdates ?? requestUpdates,
        approvalAlerts: newPrefs.approvalAlerts ?? approvalAlerts,
        slaWarnings: newPrefs.slaWarnings ?? slaWarnings,
        assetEvents: newPrefs.assetEvents ?? assetEvents,
        emailDigest: newPrefs.emailDigest ?? emailDigest,
      }).catch(() => null);
      toast.success("Preferences updated successfully");
    } catch (err) {
    }
  };

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <PageHeader
        title="Settings"
        description="Manage your account and preferences"
        crumbs={[{ label: "Settings" }]}
      />

      <Tabs defaultValue="security" className="w-full">
        <TabsList className="w-full justify-start overflow-x-auto rounded-xl bg-muted/65 p-1 border h-10 gap-1 mb-6">
          <TabsTrigger
            value="security"
            className="rounded-lg h-8 cursor-pointer text-xs font-bold px-4.5 flex items-center gap-1.5"
          >
            <Shield className="size-3.5" /> Security
          </TabsTrigger>
          <TabsTrigger
            value="theme"
            className="rounded-lg h-8 cursor-pointer text-xs font-bold px-4.5 flex items-center gap-1.5"
          >
            <Palette className="size-3.5" /> Theme
          </TabsTrigger>
          <TabsTrigger
            value="notifications"
            className="rounded-lg h-8 cursor-pointer text-xs font-bold px-4.5 flex items-center gap-1.5"
          >
            <Bell className="size-3.5" /> Notifications
          </TabsTrigger>
        </TabsList>

        <TabsContent value="security" className="outline-none">
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-6">
            <div className="space-y-4">
              <div>
                <h3 className="font-display text-base font-bold text-foreground flex items-center gap-1.5">
                  <Key className="size-4.5 text-primary" /> Change Password
                </h3>
                <p className="text-xs text-muted-foreground mt-0.5 font-medium">
                  Protect your account with a unique, secure password.
                </p>
              </div>

              <div className="grid max-w-lg gap-4">
                <div className="space-y-1.5">
                  <Label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                    Current Password
                  </Label>
                  <Input
                    type="password"
                    placeholder="••••••••"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary"
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
                    className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary"
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
                    className="h-10 rounded-xl bg-background/50 border-border/50 focus-visible:ring-primary"
                  />
                </div>
              </div>
            </div>

            <Separator className="border-border/30" />

            {/* 2FA switch container */}
            <div className="rounded-xl border border-primary/20 bg-primary/5 dark:bg-primary/10 p-5 flex items-center justify-between gap-4">
              <div className="space-y-1">
                <p className="text-sm font-extrabold text-foreground flex items-center gap-1.5">
                  <Shield className="size-4 text-primary" /> Two-Factor Authentication (2FA)
                </p>
                <p className="text-xs text-muted-foreground leading-relaxed max-w-md font-medium">
                  Add an extra layer of protection to your account.
                </p>
              </div>
              <Switch
                checked={twoFactor}
                onCheckedChange={(val) => {
                  setTwoFactor(val);
                  handleSavePreferences({ twoFactor: val });
                }}
                className="cursor-pointer shrink-0"
              />
            </div>

            <div className="mt-6 pt-4 border-t border-border/30 flex justify-end">
              <Button
                disabled={saving}
                className="rounded-xl h-10 font-bold px-5 cursor-pointer shadow-sm shadow-primary/10"
                onClick={handleUpdatePassword}
              >
                {saving && <Loader2 className="size-4 animate-spin mr-2" />}
                Update Password
              </Button>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="theme" className="outline-none">
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-5">
            <div>
              <h3 className="font-display text-base font-bold text-foreground flex items-center gap-1.5">
                <Palette className="size-4.5 text-primary" /> Appearance Theme
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5 font-medium">
                Choose a visual style that matches your environment preference.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-4 max-w-lg pt-2">
              {["light", "dark"].map((t) => {
                const isActive = theme === t;
                return (
                  <button
                    key={t}
                    onClick={() => {
                      setTheme(t);
                      handleSavePreferences({ theme: t });
                    }}
                    className={cn(
                      "group rounded-2xl border-2 p-5 text-left transition-all duration-300 relative overflow-hidden cursor-pointer w-full",
                      isActive
                        ? "border-primary bg-primary/5 shadow-md shadow-primary/5"
                        : "border-border/60 hover:border-primary/45 bg-muted/5",
                    )}
                  >
                    <div
                      className={cn(
                        "grid size-11 place-items-center rounded-xl transition-all duration-300 group-hover:scale-105",
                        t === "light"
                          ? "bg-warning/10 text-warning border border-warning/15"
                          : "bg-sidebar text-sidebar-accent-foreground border border-sidebar-border shadow-inner",
                      )}
                    >
                      {t === "light" ? <Sun className="size-5.5" /> : <Moon className="size-5.5" />}
                    </div>
                    <p className="mt-4 text-sm font-extrabold capitalize text-slate-800 dark:text-slate-100">
                      {t} Mode
                    </p>
                    <p className="text-xs text-muted-foreground mt-1 font-medium leading-relaxed">
                      {t === "light" ? "Bright and clean layout." : "Easy on the eyes in the dark."}
                    </p>
                    {isActive && (
                      <div className="absolute top-3 right-3 size-2 rounded-full bg-primary" />
                    )}
                  </button>
                );
              })}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="notifications" className="outline-none">
          <div className="rounded-2xl border border-border/50 bg-card/45 backdrop-blur-md p-6 shadow-card space-y-5">
            <div>
              <h3 className="font-display text-base font-bold text-foreground flex items-center gap-1.5">
                <Bell className="size-4.5 text-primary" /> Notification Preferences
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5 font-medium">
                Configure when and how you want to be notified about workspace actions.
              </p>
            </div>

            <div className="divide-y divide-border/30">
              {[
                {
                  t: "Request updates",
                  d: "Status changes and technician replies on your requests",
                  val: requestUpdates,
                  setVal: setRequestUpdates,
                  key: "requestUpdates",
                  icon: Mail,
                },
                {
                  t: "Approval alerts",
                  d: "New requests waiting for your approval",
                  val: approvalAlerts,
                  setVal: setApprovalAlerts,
                  key: "approvalAlerts",
                  icon: CheckSquare,
                },
                {
                  t: "SLA warnings",
                  d: "Requests approaching or breaching SLA deadlines",
                  val: slaWarnings,
                  setVal: setSlaWarnings,
                  key: "slaWarnings",
                  icon: AlertTriangle,
                },
                {
                  t: "Asset events",
                  d: "Assignments, returns and warranty expiries",
                  val: assetEvents,
                  setVal: setAssetEvents,
                  key: "assetEvents",
                  icon: Boxes,
                },
                {
                  t: "Email digest",
                  d: "A daily summary of service desk activity",
                  val: emailDigest,
                  setVal: setEmailDigest,
                  key: "emailDigest",
                  icon: Mail,
                },
              ].map((n, idx) => {
                const Icon = n.icon;
                return (
                  <div
                    key={n.t}
                    className={cn(
                      "flex items-center justify-between gap-4 py-4.5",
                      idx === 0 && "pt-2",
                      idx === 4 && "pb-0",
                    )}
                  >
                    <div className="flex items-start gap-3 min-w-0">
                      <div className="grid size-9 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary border border-primary/5 mt-0.5">
                        <Icon className="size-4.5" />
                      </div>
                      <div className="space-y-0.5 min-w-0">
                        <p className="text-sm font-extrabold text-foreground">{n.t}</p>
                        <p className="text-xs text-muted-foreground font-medium leading-relaxed">
                          {n.d}
                        </p>
                      </div>
                    </div>
                    <Switch
                      checked={n.val}
                      onCheckedChange={(val) => {
                        n.setVal(val);
                        handleSavePreferences({ [n.key]: val });
                      }}
                      className="cursor-pointer shrink-0"
                    />
                  </div>
                );
              })}
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
