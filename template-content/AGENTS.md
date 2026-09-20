# AI Agent Guide: Mkx.Templates

This repository is designed to be extended by coding agents. This file is the entry point, not the complete manual. Use progressive disclosure: read the documents relevant to the task instead of loading every guide.

## Instruction Priority
1. Follow the user's explicit requirement.
2. Follow this `AGENTS.md`.
3. Follow the relevant `docs/ai/*.md` source of truth.
4. Follow the closest existing implementation of the same concept.
5. Follow established repository conventions.
6. Follow framework/library conventions.
7. Introduce a new pattern only when none of the above cleanly handles the requirement.

When code and documentation disagree, inspect current code and verify behavior. Do not blindly reproduce stale documentation. Update the authoritative documentation when stable behavior or conventions change.

## Required Preflight
Before creating an abstraction, service, component, route, policy, DTO, helper, exception, repository, specification, or CSS class, search for an existing equivalent and inspect the nearest reference implementation. Keep changes focused.

## Documentation Map
| Document | Use it for |
|---|---|
| [ARCHITECTURE.md](docs/ai/ARCHITECTURE.md) | Layer boundaries and dependency flow |
| [DEVELOPMENT_GUIDE.md](docs/ai/DEVELOPMENT_GUIDE.md) | End-to-end feature implementation |
| [CONVENTIONS.md](docs/ai/CONVENTIONS.md) | Naming, placement and coding conventions |
| [GUARDRAILS.md](docs/ai/GUARDRAILS.md) | Architectural prohibitions and change constraints |
| [FEATURE_CHECKLIST.md](docs/ai/FEATURE_CHECKLIST.md) | Checklist for adding a business feature |
| [SERVICE_ARCHITECTURE.md](docs/ai/SERVICE_ARCHITECTURE.md) | Shared contracts, client/server services, error flow |
| [AUTHENTICATION.md](docs/ai/AUTHENTICATION.md) | Authentication, SSR/WASM boundary and authorization |
| [DATABASE.md](docs/ai/DATABASE.md) | EF Core, repositories, specs, migrations and seeders |
| [UI_GUIDE.md](docs/ai/UI_GUIDE.md) | MudBlazor UX, responsive behavior, forms and styling |
| [DOMAIN_KNOWLEDGE.md](docs/ai/DOMAIN_KNOWLEDGE.md) | Business glossary/invariants/workflows |
| [VERIFICATION.md](docs/ai/VERIFICATION.md) | Definition of done |
| [TOOLS.md](docs/ai/TOOLS.md) | Build/test/EF CLI commands |
| [SKILLS.md](docs/ai/SKILLS.md) | Compact high-frequency rules |
| [decisions/](docs/ai/decisions/) | Architectural decision records |

## Architecture in One Minute
- Domain: business model and invariants.
- Infrastructure: EF Core, persistence, repositories and specifications.
- Application: use-case orchestration, validation and mapping.
- Shared: DTOs, service contracts, routes and authorization definitions.
- Server: APIs, middleware and hosting.
- Client: Blazor WASM UI and HTTP implementations of shared contracts.

Business rules stay out of controllers and UI. Reusable queries use specifications. DI follows attribute scanning.

## Standard Feature Path
```text
Domain aggregate
 -> EF configuration
 -> specifications/repository as needed
 -> Shared DTOs + service contract + routes
 -> Application service + validation + mapping
 -> API controller
 -> Client HTTP service
 -> Blazor page/component
 -> tests + migration + verification as applicable
```
Use [FEATURE_CHECKLIST.md](docs/ai/FEATURE_CHECKLIST.md).

## UI and Render Modes
Interactive application pages run in Interactive WebAssembly mode without prerendering. Authentication/account surfaces may use static SSR. Do not inject interactive-only client services into static SSR pages.

Use MudBlazor and existing UI patterns. Current source code is authoritative for exact drawer/layout parameters; desktop navigation is Mini/collapsible and mobile navigation must remain overlay/temporary.

## Authorization
Policy-based authorization is preferred for capabilities. Adding a policy normally requires an `AppPolicies` constant, registration in `AppPolicyProvider`, server enforcement, and appropriate client behavior. Client hiding alone is never security.

## Verification
A task is not complete merely because code was written. Follow [VERIFICATION.md](docs/ai/VERIFICATION.md).

```powershell
dotnet build Mkx.Templates.slnx
dotnet test
```

Never claim a build, test, migration, runtime or browser check passed unless actually executed. If blocked, report the exact blocker.

## Documentation Maintenance
Keep this file concise and navigational. Put detailed durable knowledge in `docs/ai/`. Update docs when stable behavior changes. Record business knowledge in `DOMAIN_KNOWLEDGE.md` and cross-cutting decisions in `docs/ai/decisions/`. Never store secrets or transient debugging/machine state here.
