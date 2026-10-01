# Operations and optional integrations

## Configuration defaults

| Key | Default/contract |
|---|---|
| ConnectionStrings:Mkx.Templates | Base appsettings has local Windows integrated SQL without a password; template generation replaces both key/database with the project name; override for your deployment |
| Database:ApplyMigrationsOnStartup | false in production; true in Development |
| Database:SeedOnStartup | false in production; true in Development |
| BootstrapAdmin:Enabled | false; when enabled requires Username/Password secrets |
| Features:Pwa | true; registration only outside Development |
| Logging:UseSqlStore | true; enables both SQL sink and protected log UI when true |
| ReverseProxy:KnownProxies | explicit trusted IPs; keep framework loopback defaults, never trust arbitrary forwarded addresses |
| Sms:Endpoint | empty; SMS workflows fail rather than pretend to send |
| Sms:ApiKey | optional secret sent as X-API-Key |

All SQL consumers (EF, optional SQL logs/UI and Rebus) use the project-named connection key. There is no fallback to `Default`, which could silently select unrelated shared configuration. Base appsettings supplies a password-free local SQL example even when the environment is Production; deployment configuration should override it. User-secrets load automatically in Development. Database isolation depends on the database name/SQL server, while secret isolation depends on the generated UserSecretsId; the configuration key alone is not a global isolation boundary.

No CORS is needed for this same-origin template. If adding a separate frontend, define explicit origins, cookie/CSRF strategy and tests rather than permissive wildcard credentialed CORS. Use TLS at the edge and configure proxy trust before relying on client IP rate limiting. The host adds nosniff, referrer and permissions headers, HSTS outside Development and no-store on API responses. A strict CSP is product/deployment specific because Blazor/WebAssembly, import maps and MudBlazor have script/style requirements.

Auth/verification mutations are limited to 20 per minute per client IP; other mutations to 100. Safe reads are unthrottled. Rejections return 429 and Retry-After. These in-process limits and SMS cooldowns are single-instance defaults; multiple replicas require a shared limiter/cooldown mechanism. Never log request bodies, passwords, OTPs, passkey challenges or provider API keys.

`/health/live` does not query storage; `/health/ready` checks SQL connectivity and pending migrations. Keep readiness available to your orchestrator; do not expose sensitive diagnostic payloads. Apply reviewed SQL scripts/bundles with deployment permissions before replicas start. Runtime credentials should not have schema-altering permissions.

MapStaticAssets owns the fingerprinted/compressed public asset endpoints and cache headers. Do not add a global static-cache middleware: successful dynamic HTML, account responses and health checks must not inherit public asset caching.

## SMS adapter

SmsSender is a typed HttpClient with a 15-second timeout. Configure an HTTPS webhook receiving JSON `{ phoneNumber, message }`, optionally authenticated with X-API-Key. A 2xx means accepted, not delivered; non-2xx is a failure. Configure your provider adapter when its contract differs. No automatic retry is applied to sends because retries can duplicate messages; add idempotency/delivery status using the chosen provider's contract. Replace ISmsSender in DI rather than duplicating scanning registrations.

## Observability and business audit

ProblemDetails includes traceId/instance; ASP.NET Core supplies Activity-based request tracing and runtime meters. Console application logs are enabled; SQL storage/UI is optional. Connect your chosen OpenTelemetry/exporter outside Domain and avoid high-cardinality identity/PII metric tags. Durable business audit is distinct from diagnostics: add an append-only audit store inside the same transaction as the business mutation, with actor/action/resource/time and retention/access policy. Do not claim console logs constitute that store.

## Product-specific extension seams

NotificationsDrawer intentionally ships empty. Add a Shared notification contract, bounded list/read APIs, per-user persistent records, server enforcement, a single client state service and count-based badge; test cross-user access. Drag visuals, if introduced, should run in DOM pointer events/requestAnimationFrame and send one completed action to Blazor, not render on every touchmove.

Email/provider delivery, upload/storage, exports, durable jobs, tenancy and Outbox are optional product modules. Choose them only when the product requires their permissions, quotas, storage/retention and delivery semantics. Reuse the tested CRUD path and document the choice in an ADR. They are not hidden mandatory dependencies of this template.
