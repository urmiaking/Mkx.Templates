# Performance budget and measurement

These are target budgets, not measured claims: aim for LCP <=2.5s, INP <=200ms and CLS <=0.1 on representative production traffic. For a no-prerender WASM application, record UI-ready separately from document paint. Use a release baseline for transferred compressed bytes; require an explanation for a >10% startup payload increase. Keep initial list pages <=25 rows and server maximum <=100.

Glass rules are executable in app.css: mobile/coarse pointers use opaque surface backgrounds while preserving established gradients and typography, cards/dialogs have no backdrop blur, desktop appbar has one 8px blur under @supports, OS reduced motion disables it, and page wrappers have no permanent will-change. Repeated content must not accumulate effects.

Measure Release on the same target phone, browser, data and network for: cold boot, warm boot, long-list scroll, drawer open/close, dialog with mobile keyboard, light/dark and OS reduced motion. Record UI-ready time, transferred bytes, interaction latency, dropped frames, paint/composite and memory. Compare normal/reduced motion after caching is controlled. Use at least five runs and median; keep traces out of the template package. Viewport emulation is layout coverage only.

Keep full ICU until a measured locale-specific reduction preserves fa/PersianCalendar/number/date behavior. Evaluate prerender with auth-state transfer and all SSR/JS/service boundaries; it is not a safe automatic single-line switch. PWA offers a public offline page, not offline authenticated CRUD.
