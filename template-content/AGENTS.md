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
| [PROJECT_SETUP.md](docs/ai/PROJECT_SETUP.md) | New-project branding, icons and optional CRUD sample |
| [CONVENTIONS.md](docs/ai/CONVENTIONS.md) | Naming, placement and coding conventions |
| [GUARDRAILS.md](docs/ai/GUARDRAILS.md) | Architectural prohibitions and change constraints |
| [FEATURE_CHECKLIST.md](docs/ai/FEATURE_CHECKLIST.md) | Checklist for adding a business feature |
| [SERVICE_ARCHITECTURE.md](docs/ai/SERVICE_ARCHITECTURE.md) | Shared contracts, client/server services, error flow |
| [AUTHENTICATION.md](docs/ai/AUTHENTICATION.md) | Authentication, SSR/WASM boundary and authorization |
| [DATABASE.md](docs/ai/DATABASE.md) | EF Core, repositories, specs, migrations and seeders |
| [UI_GUIDE.md](docs/ai/UI_GUIDE.md) | Native Blazor UX, responsive behavior, forms and styling |
| [DOMAIN_KNOWLEDGE.md](docs/ai/DOMAIN_KNOWLEDGE.md) | Business glossary/invariants/workflows |
| [VERIFICATION.md](docs/ai/VERIFICATION.md) | Definition of done |
| [TOOLS.md](docs/ai/TOOLS.md) | Build/test/EF CLI commands |
| [OPERATIONS.md](docs/ai/OPERATIONS.md) | Defaults, deployment and optional providers |
| [PERFORMANCE.md](docs/ai/PERFORMANCE.md) | Mobile effects, budgets and measurement |
| [SKILLS.md](docs/ai/SKILLS.md) | Compact high-frequency rules |
| [decisions/](docs/ai/decisions/) | Architectural decision records |

## Architecture in One Minute
- Domain: business model and invariants.
- Infrastructure: EF Core, persistence, repositories and specifications.
- Application: use-case orchestration, validation and mapping.
- Shared: DTOs, service contracts, routes and authorization definitions.
- Server: APIs, middleware and hosting.
- Client: Blazor WASM UI and HTTP implementations of shared contracts.

Business rules stay out of controllers and UI. Reusable queries use specifications. Ordinary DI follows attribute scanning; framework services and typed HTTP clients are explicitly registered. Application deliberately references Infrastructure; EF persistence stays in Infrastructure; shared Identity models intentionally belong to SDK Domain and may be used by Core Domain.

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

The UI follows a calm Persian RTL workspace: Vazirmatn, navy/teal light accents and vivid jade dark accents, opaque surfaces, short interaction feedback and responsive layouts. Preserve its shared tokens and accessible interactions.

Jade is the default palette; ocean, iris and amber also have paired light/dark tokens. Preserve the independent persisted palette and mode preferences and the draggable edge appearance control described in UI_GUIDE.md.

Use semantic HTML and the native Blazor primitives in Components/UI. The desktop sidebar is permanently 238px wide. Below 960px, full-height navigation overlays content and reserves no horizontal space; the bottom bar includes a raised central action. See UI_GUIDE.md for forms, dialogs and styling.

Page-entry motion applies on phones, tablets and desktop, including headers, grouped bodies/tables and identity cards. Preserve the operating system's reduced-motion preference; do not disable motion based only on viewport size.

## Starting a Generated Product
When creating a new product from this template, replace its default application icon and brand mark with an identity appropriate to that product before reporting completion. Update favicon, PWA/Apple icons, manifest and visible branding together; follow [PROJECT_SETUP.md](docs/ai/PROJECT_SETUP.md). Preserve the executable SampleItem reference until a real, tested business feature replaces it, unless the user chooses generation without the sample.

## Authorization
Policy-based authorization is preferred for capabilities. Adding a policy normally requires an `AppPolicies` constant, registration in `AppPolicyProvider`, server enforcement, and appropriate client behavior. Client hiding alone is never security.

## Verification
A task is not complete merely because code was written. Follow [VERIFICATION.md](docs/ai/VERIFICATION.md).

```powershell
dotnet build Mkx.Templates.slnx
dotnet test --solution Mkx.Templates.slnx
./scripts/verify.ps1
```

Never claim a build, test, migration, runtime or browser check passed unless actually executed. If blocked, report the exact blocker.

## Documentation Maintenance
Keep this file concise and navigational. Put detailed durable knowledge in `docs/ai/`. Update docs when stable behavior changes. Record business knowledge in `DOMAIN_KNOWLEDGE.md` and cross-cutting decisions in `docs/ai/decisions/`. Never store secrets or transient debugging/machine state here.
