# ADR 0004: Interactive WebAssembly Application Pages

**Status:** Accepted

Interactive application pages use Interactive WebAssembly without prerendering; server-sensitive authentication/account surfaces may use static SSR. Client feature services call backend APIs. Static SSR pages cannot assume interactive scoped client services. Render-mode changes must evaluate auth state, loading UX and service resolution.
