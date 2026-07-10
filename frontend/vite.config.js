import { defineConfig } from "@lovable.dev/vite-tanstack-config";

export default defineConfig({
  tanstackStart: {
    // Redirect TanStack Start's bundled server entry to src/server.js
    server: { entry: "server" },
  },
  server: {
    watch: {
      ignored: ["**/.output/**", "**/.tanstack/**"],
    },
  },
});
