import { createFileRoute, redirect } from "@tanstack/react-router";
import { AuthLayout, LoginForm } from "@/components/auth/AuthLayout";

export const Route = createFileRoute("/login")({
  head: () => ({
    meta: [
      { title: "Sign in — ServiceDesk" },
      {
        name: "description",
        content: "Sign in to ServiceDesk to raise and manage IT service requests.",
      },
    ],
  }),
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
