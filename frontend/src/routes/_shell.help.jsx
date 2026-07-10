import { createFileRoute } from "@tanstack/react-router";
import { motion } from "motion/react";
import {
  BookOpen,
  LifeBuoy,
  Mail,
  MessageSquare,
  Phone,
  FileText,
  Rocket,
  Wrench,
} from "lucide-react";
import { useForm } from "react-hook-form";
import { PageHeader } from "@/components/shared/PageHeader";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { faqs } from "@/data/mock";
import { toast } from "sonner";

export const Route = createFileRoute("/_shell/help")({
  head: () => ({
    meta: [
      { title: "Help Center — ServiceDesk" },
      { name: "description", content: "FAQs, documentation and support contacts for ServiceDesk." },
    ],
  }),
  component: HelpPage,
});

const docs = [
  {
    icon: Rocket,
    title: "Getting started",
    desc: "Raise your first request and track it to resolution.",
  },
  { icon: FileText, title: "Approval workflows", desc: "How HOD approvals and escalations work." },
  { icon: Wrench, title: "Technician guide", desc: "Managing assignments, replies and closures." },
  { icon: BookOpen, title: "Admin handbook", desc: "Masters, departments, mappings and reports." },
];

function HelpPage() {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm();

  return (
    <div>
      <PageHeader
        title="Help Center"
        description="Guides, answers and support"
        crumbs={[{ label: "Help Center" }]}
      />

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card"
          >
            <h3 className="flex items-center gap-2 font-display text-base font-bold">
              <LifeBuoy className="size-4 text-primary" /> Frequently Asked Questions
            </h3>
            <Accordion type="single" collapsible className="mt-3">
              {faqs.map((f, i) => (
                <AccordionItem key={i} value={`faq-${i}`}>
                  <AccordionTrigger className="text-left text-sm font-semibold">
                    {f.q}
                  </AccordionTrigger>
                  <AccordionContent className="text-sm leading-relaxed text-muted-foreground">
                    {f.a}
                  </AccordionContent>
                </AccordionItem>
              ))}
            </Accordion>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card"
          >
            <h3 className="flex items-center gap-2 font-display text-base font-bold">
              <BookOpen className="size-4 text-primary" /> Documentation
            </h3>
            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              {docs.map((d) => (
                <button
                  key={d.title}
                  onClick={() => toast.info("Documentation — UI demo")}
                  className="group flex items-start gap-3 rounded-xl border p-4 text-left transition-all hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-md"
                >
                  <div className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary/10 text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
                    <d.icon className="size-4" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-bold">{d.title}</p>
                    <p className="mt-0.5 text-xs text-muted-foreground">{d.desc}</p>
                  </div>
                </button>
              ))}
            </div>
          </motion.div>
        </div>

        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.15 }}
          className="h-fit space-y-4"
        >
          <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card">
            <h3 className="flex items-center gap-2 font-display text-base font-bold">
              <MessageSquare className="size-4 text-primary" /> Contact Support
            </h3>
            <form
              onSubmit={handleSubmit(() => {
                toast.success("Support ticket submitted!");
                reset();
              })}
              className="mt-4 space-y-3"
            >
              <div className="space-y-1.5">
                <Label htmlFor="subject">Subject</Label>
                <Input
                  id="subject"
                  placeholder="What do you need help with?"
                  className="h-10 rounded-xl"
                  {...register("subject", { required: "Subject is required" })}
                />

                {errors.subject && (
                  <p className="text-xs text-destructive">{errors.subject.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="message">Message</Label>
                <Textarea
                  id="message"
                  rows={4}
                  placeholder="Describe your question…"
                  className="rounded-xl"
                  {...register("message", { required: "Message is required" })}
                />

                {errors.message && (
                  <p className="text-xs text-destructive">{errors.message.message}</p>
                )}
              </div>
              <Button type="submit" className="w-full rounded-xl">
                Submit ticket
              </Button>
            </form>
          </div>

          <div className="rounded-2xl border bg-card/40 backdrop-blur-md p-6 shadow-card">
            <p className="text-sm font-bold">Reach us directly</p>
            <div className="mt-3 space-y-3 text-sm">
              <p className="flex items-center gap-2 text-muted-foreground">
                <Mail className="size-4 text-primary" /> support@servicedesk.io
              </p>
              <p className="flex items-center gap-2 text-muted-foreground">
                <Phone className="size-4 text-primary" /> +91 1800 234 567
              </p>
            </div>
            <p className="mt-3 text-xs text-muted-foreground">
              Support hours: Mon–Sat, 9 AM – 7 PM IST
            </p>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
