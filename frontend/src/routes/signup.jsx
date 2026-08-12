import { createFileRoute } from "@tanstack/react-router";
import { AuthLayout, SignupForm } from "@/components/auth/AuthLayout";

export const Route = createFileRoute("/signup")({
  head: () => ({
    meta: [
      { title: "Create Account — ServiceDesk" },
      {
        name: "description",
        content: "Register a new account on ServiceDesk to submit and manage IT requests.",
      },
    ],
  }),
  component: SignupPage,
});

function SignupPage() {
  return (
    <AuthLayout
      title="Create your account"
      subtitle="Join ServiceDesk to start managing your IT requests."
    >
      <SignupForm />
    </AuthLayout>
  );
}
