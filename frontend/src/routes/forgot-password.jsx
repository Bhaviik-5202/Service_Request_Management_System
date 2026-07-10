import { createFileRoute, Link } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { ArrowLeft } from "lucide-react";
import { AuthLayout } from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { users, syncLocalStorage } from "@/data/mock";

export const Route = createFileRoute("/forgot-password")({
  head: () => ({
    meta: [
      { title: "Forgot password — ServiceDesk" },
      {
        name: "description",
        content: "Request a password reset link for your ServiceDesk account.",
      },
    ],
  }),
  component: ForgotPasswordPage,
});

function ForgotPasswordPage() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const onSubmit = (data) => {
    syncLocalStorage();
    const matchedUser = users.find(
      (u) => u.email.toLowerCase() === data.email.trim().toLowerCase(),
    );

    if (matchedUser) {
      toast.success("Reset link sent!", {
        description: "Check your inbox for password reset instructions.",
      });
    } else {
      toast.error("Account not found", {
        description: "This email address is not registered in our system.",
      });
    }
  };

  return (
    <AuthLayout
      title="Forgot your password?"
      subtitle="Enter your work email and we'll send you a reset link."
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="email">Work email</Label>
          <Input
            id="email"
            type="email"
            placeholder="you@company.com"
            className="h-10 rounded-xl"
            {...register("email", {
              required: "Email is required",
              pattern: {
                value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                message: "Invalid email format",
              },
            })}
          />

          {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
        </div>
        <Button type="submit" className="h-10 w-full rounded-xl font-semibold cursor-pointer">
          Send reset link
        </Button>
        <Link
          to="/login"
          className="flex items-center justify-center gap-1.5 text-sm font-medium text-primary hover:underline mt-4"
        >
          <ArrowLeft className="size-4" /> Back to sign in
        </Link>
      </form>
    </AuthLayout>
  );
}
