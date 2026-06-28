# Antigravity Developer Guide: Mkx.Templates.Blazor

Welcome to the Mkx.Templates.Blazor solution. This document summarizes the overall clean architecture, individual project responsibilities, folder structures, backend domain logic, and custom styling patterns.

---

## 1. Project Overview & Architecture

This repository is built as a modern, clean-architecture template for .NET 10 Blazor Web Apps, using the modern XML-based `.slnx` solution configuration.

### Architectural Principles
- **Clean Architecture & DDD (Domain-Driven Design)**: Core business logic (Domain) is isolated at the center, completely independent of external databases, UI frameworks, or APIs.
- **Interface Segregation**: Service contracts reside in a Shared project. The host Server registers the backend implementation, while the Client WASM application registers the API HTTP client implementation polymorphically.
- **Specification Pattern**: Database queries are decoupled from repositories using Specification classes, making queries strongly-typed, reusable, and unit-testable.
- **Assembly Scanning**: Dependency injection is handled automatically through custom SDK attributes (e.g., `[ScopedService]`, `[TransientService]`, `[SingletonService]`), eliminating manual registration noise in `Program.cs`.

---

## 2. Directory Structure & Project Responsibilities

```
d:\source\Mkx.Templates.Blazor
├── template-content/
│   ├── Mkx.Templates.Blazor.slnx   # XML solution configuration file
│   └── src/
│       ├── Core/
│       │   ├── Mkx.Templates.Domain/          # Domain layer (Entities, Aggregates, Value Objects)
│       │   └── Mkx.Templates.Infrastructure/  # Infrastructure layer (EF Core DbContext, Configurations, Repositories, Migrations)
│       ├── Server/
│       │   ├── Mkx.Templates.Application/     # Application services, Mapster mappings, Validators
│       │   └── Mkx.Templates.Server/          # Server Host (Controllers, Middlewares, Program.cs, CSS/Assets)
│       ├── Client/
│       │   └── Mkx.Templates.Client/          # Blazor client (Pages, UI Components, Client Services)
│       ├── Shared/
│       │   └── Mkx.Templates.Shared/          # Shared DTOs, Interface contracts, API and Client Routes
│       ├── Sdk/                               # Shared SDK containing base building blocks for Domain, Infrastructure, API, and Client
│       └── Tests/                             # Unit and Integration test projects
└── AGENTS.md                                  # This Developer Guide
```

### Detailed Project Responsibilities

#### 1. Sdk Layer
Provides foundational models, exception classes, helper extensions, and core base configurations:
- **Sdk.Server.Domain**: Base domain models (e.g., `EntityBase<TId>`).
- **Sdk.Server.Infrastructure**: Base repositories and specification wrappers.
- **Sdk.Server.Api**: Common API controllers (`ApiControllerBase`), validation filters, and JSON serialization.
- **Sdk.Shared**: Foundational exceptions (like `HttpRequestFailedException`) and routing helpers.

#### 2. Core Layer (Domain & Infrastructure)
- **Mkx.Templates.Domain**: Contains the core business aggregates, entities, value objects, and domain services. Depends only on `Sdk.Server.Domain`.
- **Mkx.Templates.Infrastructure**: Implements persistence. Contains `AppDbContext` (configured for PostgreSQL/SQL Server), EF Core entity mapping configurations, database migrations, concrete repository implementations, and specifications.

#### 3. Shared Layer
- **Mkx.Templates.Shared**: A lightweight project compiled to both client and server. Houses DTO records (such as `GetTestResponse`), service interfaces (`ITestService`), `ApiRoutes`, and `ClientRoutes` configuration structures.

#### 4. Server Layer (Application & Server Host)
- **Mkx.Templates.Application**: Houses the orchestration logic. Contains implementation of shared service interfaces (`TestService`), Mapster mapping configurations (`IRegister`), FluentValidation validators, and application exceptions.
- **Mkx.Templates.Server**: The entry point hosting the ASP.NET Core Web App. Configures the middleware pipeline (Serilog, CORS, authentication, static assets, etc.), exposes Web API controllers (`TestsController`), and acts as the Blazor Web App host (delivering prerendered static HTML and client WASM assemblies).

#### 5. Client Layer
- **Mkx.Templates.Client**: Runs in WebAssembly (WASM). Contains razor views (`Tests.razor`), custom UI components (`AppBar.razor`, `Drawer.razor`), theme settings (`ColorPalettes.cs`), and HTTP-based implementations of shared services (`TestClientService`) which retrieve data from Server APIs.

---

## 3. The `Test` Aggregate Flow (From Aggregate to Client UI)

The `Test` aggregate showcases the end-to-end clean architecture flow:

```
[ Domain Aggregate ] <--- Managed by ---> [ Infrastructure Configurations/Specs/Repositories ]
                                                               |
                                                  (Shared DTO & Interface)
                                                               |
                                            [ Application Service (Server-side) ]
                                                               |
                                                 [ API Controller Endpoint ]
                                                               |
                                                    [ Client Service (WASM) ]
                                                               |
                                                    [ Blazor UI Component ]
```

### Detailed Execution & Data Mapping

#### 1. Domain Aggregate Definition
`Test` (defined in [Test.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Core/Mkx.Templates.Domain/TestAggregate/Test.cs)) encapsulates its data structure and mutations:
- Inherits `EntityBase<TestId>` with a strongly-typed `TestId` struct wrapping a Guid.
- Exposes private setters for properties (`Name`, `Description`) to prevent direct modification from outside.
- Instantiated via a static factory method `Create(name, description)` (generating a new `Guid.CreateVersion7()`) and mutated via the `Update(name, description)` method.
- Contains a private parameterless constructor for EF Core ORM materialization.

#### 2. Infrastructure Configuration & Querying
- **EF Core Mapping**: `TestConfiguration.cs` configures the database schema for the entity, mapping the strong-typed `TestId` using a value converter.
- **Specifications**: Reusable queries are written as specifications (e.g., [AllTestsSpecification.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Core/Mkx.Templates.Infrastructure/Specifications/Tests/AllTestsSpecification.cs) or [TestById.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Core/Mkx.Templates.Infrastructure/Specifications/Tests/TestById.cs)) which inherit from `SpecificationBase<Test>`.
- **Repository**: `TestRepository` queries `Test` aggregates by applying the specification criteria onto `IQueryable<Test>`.

#### 3. Shared Contracts
- **Interface**: [ITestService.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Shared/Mkx.Templates.Shared/Abstractions/ITestService.cs) exposes async contract methods (`GetAllAsync`, `GetAsync`) returning a list or single instance of DTOs.
- **DTO**: [GetTestResponse.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Shared/Mkx.Templates.Shared/DTOs/Tests/GetTestResponse.cs) is a lightweight, immutable C# `record` used as the response data carrier.

#### 4. Backend Application Logic
- **Implementation**: [TestService.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Server/Mkx.Templates.Application/Services/TestService.cs) implements `ITestService`. Decorated with `[ScopedService]` for DI scanning.
- **Mapping**: [TestMapper.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Server/Mkx.Templates.Application/Mappers/TestMapper.cs) implements Mapster's `IRegister` to project the domain model `Test` properties directly into the `GetTestResponse` DTO structure.

#### 5. Server Controller exposing the API
- **Endpoint**: [TestsController.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Server/Mkx.Templates.Server/Controllers/TestsController.cs) derives from `ApiControllerBase` and routes to `ApiRoutes.Tests.Base` (`/api/tests`).
- Returns HTTP Status `200 OK` with JSON serialized `GetTestResponse` lists or instances.

#### 6. Client HTTP Service Implementation
- **Client Service**: [TestClientService.cs](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Client/Mkx.Templates.Client/Services/TestClientService.cs) implements `ITestService` on the frontend WASM app.
- Resolves HTTP endpoints using routing helpers (`ApiUrls.Tests.Get(id)`), parses HTTP status codes, and raises custom exceptions (like `HttpRequestFailedException`) if calls fail.

#### 7. Blazor WASM View Integration
- **Page Component**: [Tests.razor](file:///d:/source/Mkx.Templates.Blazor/template-content/src/Client/Mkx.Templates.Client/Pages/Tests.razor) inherits from `AppComponentBase` and renders a MudBlazor `MudTable` representing tests.
- **Request Delegation**: UI triggers requests via:
  ```csharp
  var result = await SendRequestAsync<ITestService, List<GetTestResponse>>(
      action: (service, cancellationToken) => service.GetAllAsync(cancellationToken)
  );
  ```
- **Helper Mechanics**: `AppComponentBase.SendRequestAsync` manages the screen's loading state (`IsBusy` flag), registers cancellation tokens, handles UI thread exception interception, and triggers Snackbar error toasts for standard business failures.

---

## 4. Design System & Styling (app.css)

All layout-specific, custom glassmorphic and animation styles are centralized in the Server's `app.css` file (`template-content/src/Server/Mkx.Templates.Server/wwwroot/css/app.css`) rather than scoped CSS files.

### Global CSS Best Practices
- **CSS Variables for Theming**: To ensure styles are connection-resilient and compatible with Server-Side Rendering (SSR) pages (like Login and AccessDenied), styles should be entirely pure-CSS based utilizing native MudBlazor variables such as `rgba(var(--mud-palette-primary-rgb), opacity)`.
- **Glassmorphism**: Premium iOS-like frosted-glass effects are created using:
  - `backdrop-filter: blur(40px) saturate(200%) !important;`
  - A subtle gradient background combining primary and surface variables at high transparency (e.g., `0.3` to `0.5`).
  - Solid border strokes and internal white inset shadows: `box-shadow: ..., inset 0 1px 0 rgba(255, 255, 255, 0.35)`.

---

## 5. Core UI Elements & Layouts

### Brand Text & Shine Animation
The brand text logo (e.g. `Mkx.Templates`) uses the `.brand-text` class which implements a premium glowing shine animation:
- **Gradient**: Bound to the active theme's primary color `var(--mud-palette-primary)` with a bright white shine in the center (`rgba(255, 255, 255, 0.9)`).
- **Animation**: The `shine` keyframe animation translates the `background-position` from `120% center` to `-20% center` over a `6s` duration.
- **Tighter Bounds**: Keeping translation range tight (e.g., `120%` to `-20%`) rather than wide offsets keeps the reflection active across the letters rather than spending half the time hidden off-screen.
- **Shorthand / Important Override**: Do not define `background-position` with `!important` on the class level, as it overrides and locks keyframe animation translations.

### Empty Box Layout (Static SSR Safe)
`EmptyBoxLayout.razor` is the layout wrapper for static SSR pages (Login, AccessDenied, NotFound).
- **No ThemeService Injection**: Because these pages render in static SSR mode, they cannot depend on interactive scoped client services like `ThemeService` in C#. Instead, the theme layout adapts natively through standard CSS variables.
- **Dynamic Background**: The background class `.box-bg` utilizes a radial gradient blending the theme's primary color into the main background:
  `radial-gradient(circle at 12% 12%, rgba(var(--mud-palette-primary-rgb), 0.15) 0%, var(--mud-palette-background) 75%)`
- **Glass Card**: The card is styled under `.glass-card` as a semi-translucent pane:
  - Light mode: Soft translucent white background.
  - Dark mode: Dark slate translucent background.

### Frosted Drawer
Defined in `Drawer.razor` with custom classes `.fancy-drawer.light-mode` and `.fancy-drawer.dark-mode`:
- **Light Mode Drawer**: Uses a translucent frosted appearance (`background-color: rgba(var(--mud-palette-surface-rgb), 0.45)`) combined with a primary border stroke.
- **Borders & Buttons**: In light mode, shortcut icon buttons (`.shortcut-btn`) have primary-tinted backgrounds and borders (`rgba(var(--mud-palette-primary-rgb), 0.22)`) so they stand out. The bottom exit button (`.close-drawer-btn`) uses a stronger divider border.

### Reconnect Modal
A custom connection-status handler in `ReconnectModal.razor` and `ReconnectModal.razor.css`:
- **UX Styling**: Uses a glassmorphic card with a backdrop blur overlay (`#components-reconnect-modal`).
- **Persian Copywriting**: Replaced default messages with polite, inline Persian copy (e.g., "اتصال شما قطع شده است. تلاش مجدد در X ثانیه...").
- **Display Mechanics**: Active state paragraph overrides (`display: block !important`) force the inline sentence flow while inactive states are hidden using (`display: none !important`).

---

## 6. Compilation & Verification

Before finalizing changes, compile the entire solution file:
```powershell
dotnet build template-content/Mkx.Templates.slnx
```
The client and server applications run seamlessly on the local development environment (`http://localhost:7007` and `https://localhost:7006`).
