import { createFileRoute } from "@tanstack/react-router";
import { AuthLayout, LoginForm } from "@/components/auth/AuthLayout";

export const Route = createFileRoute("/login")({
  component: LoginPage,
});

function LoginPage() {
  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Enter your credentials to access your dashboard."
    >
      <LoginForm />
    </AuthLayout>
  );
}
