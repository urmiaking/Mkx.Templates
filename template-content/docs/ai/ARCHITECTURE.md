# Architecture

The template uses a pragmatic layered architecture with domain models separated from EF configuration. It does not claim strict dependency inversion: Application deliberately references Infrastructure repository contracts and specifications. See decision 0006 before changing that tradeoff.

| Project/area | Responsibility | Allowed dependencies |
|---|---|---|
| Core Domain | Aggregates, strong IDs, business invariants | SDK Domain (including shared Identity models); no EF context/configuration, HTTP or UI |
| SDK Domain | Entity/event primitives and shared Identity models | .NET and Microsoft.Extensions.Identity.Stores |
| Core/SDK Infrastructure | EF context, Identity configuration, repositories, specifications, migrations | Domain and transport-neutral SDK primitives |
| Application | Use cases, validation, mapping, Identity management | Domain, Infrastructure, Shared, SDK Application |
| Shared/SDK Shared | Immutable transport contracts, routes, policy definitions, paging | No EF or server persistence |
| Server | HTTP, cookie/SignInManager adapters, SSR account flows, hosting | Application/Infrastructure and Client assets |
| Client | WASM pages, UI state, HTTP contract implementations | Shared and client libraries; no Infrastructure or DbContext |

Identity entities live in `src/Sdk/Mkx.Templates.Sdk.Server.Domain/Identity/`, the original project ownership. Core Domain can use AppUser/AppRole and their relationships through its SDK Domain reference. The explicit Identity.Stores dependency supplies model base classes; EF contexts, mapping and store implementations stay in Infrastructure. This is an intentional template tradeoff, not a strict Identity-free Domain. Preserve this ownership unless the user requests an architecture change.

`Server/Services/UserAccountService` remains a deliberate host adapter because its account flows use cookies, SignInManager and HttpContext. Pure business rules should still move into Application/Domain as they emerge; moving this entire adapter into Application would introduce a host dependency cycle.

Query intent goes into Infrastructure specifications. The complete Test feature is the reference; [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) maps every file. User/role management uses Identity's own stores with bounded pages and transaction boundaries. Do not wrap every Identity API in a second repository.

DI scanning uses SDK lifetime attributes. Framework providers, authentication state and typed HTTP clients are registered explicitly. A typed client such as SmsSender must not also be decorated for scanning.

Interactive pages use Interactive WebAssembly with `prerender: false`; `/Account` pages use static SSR. This boundary is intentional and tested separately. Switching render modes is an architectural change requiring auth-state, service-registration, storage/JS and SSR checks, not a one-line guarantee.

The host uses same-origin APIs; CORS is not enabled. SQL-backed log UI is enabled by default and administrator protected; Logging:UseSqlStore=false explicitly disables it. Links to /serilog-ui require full server navigation, because it is a middleware surface outside the WASM router. Rate limiting, forwarded headers, health checks and security headers are configured in the Server hosting extensions.
