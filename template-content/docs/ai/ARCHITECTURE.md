# Architecture Guide: Mkx.Templates

This project follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles to separate business logic from technical concerns (UI, databases, frameworks, APIs).

---

## 1. Architectural Layers & Responsibilities

The system is organized into a nested concentric directory structure where dependency flow is inward toward the **Domain** layer. 

```
[ Presentation (Client / Server Host) ] ──> [ Application Layer ] ──> [ Domain (Core) ]
                                                   │
                                                   └───> [ Infrastructure (Data Access) ]
```

### Core Layer (Business Core)
1. **`Mkx.Templates.Domain`**
   - **Responsibility**: Contains the core business concepts, aggregate roots, entities, value objects, domain services, domain events, and core exceptions.
   - **Dependencies**: Depends only on `Mkx.Templates.Sdk.Server.Domain`. It has **no** knowledge of database engines, Web APIs, HTTP connections, or Blazor.
2. **`Mkx.Templates.Infrastructure`**
   - **Responsibility**: Manages data persistence. Houses the EF Core `AppDbContext`, database migrations, entity mappings (configurations), concrete repository implementations, and specifications.
   - **Dependencies**: Depends on `Mkx.Templates.Domain` and `Mkx.Templates.Sdk.Server.Infrastructure`.

### Presentation & Orchestration Layers
3. **`Mkx.Templates.Application`**
   - **Responsibility**: Implements the system use cases. Coordinates retrieving domain aggregates via repositories, executing domain mutations, saving changes, mapping domain models to DTOs, and validating request objects.
   - **Dependencies**: Depends on `Mkx.Templates.Domain`, `Mkx.Templates.Infrastructure`, and `Mkx.Templates.Shared`.
4. **`Mkx.Templates.Server` (Host)**
   - **Responsibility**: Exposes Web APIs (Controllers) and hosts the Blazor Web App. Configures the ASP.NET Core middleware pipeline (authentication, CORS, Serilog, rate limiting) and serves static web assets (CSS/JS).
   - **Dependencies**: Depends on `Mkx.Templates.Application` and references `Mkx.Templates.Client` (to bundle WebAssembly DLLs for client-side execution).

### Frontend & Shared Layers
5. **`Mkx.Templates.Client` (Blazor WASM)**
   - **Responsibility**: Contains frontend client pages, UI components, client-side routing, and client-side implementation of services (sending HTTP requests to backend Web APIs).
   - **Dependencies**: Depends on `Mkx.Templates.Shared` and frontend UI libraries (MudBlazor). Runs in WebAssembly.
6. **`Mkx.Templates.Shared`**
   - **Responsibility**: Serves as a lightweight contract library compiled for both client and server. Contains Data Transfer Objects (DTOs), service interfaces, API route constants, and routing helpers.
   - **Dependencies**: Refered to by all presentation and application projects.

---

## 2. Polymorphic Interface Segregation & Blazor Interactive Auto Mode

To support Blazor's **Interactive Auto Mode** (which dynamically switches between Server pre-rendering and Client-side WebAssembly execution), the application resolves service contracts polymorphically:

- **Service Interfaces** (e.g., `IFeatureService`) are defined inside `Mkx.Templates.Shared/Abstractions/`.
- **Server Implementation** is placed inside `Mkx.Templates.Application/Services/` (e.g., `FeatureService` directly querying repositories/DB).
- **Client Implementation** is placed inside `Mkx.Templates.Client/Services/` (e.g., `FeatureClientService` making JSON API HTTP calls to backend endpoints).
- Both implementations are registered using SDK auto-scanning attributes, allowing UI components (pages) to call the same interface without worrying about where the code executes.

---

## 3. Route Constants & Endpoint Mapping

To maintain strict alignment between backend controllers and frontend client services, routes are never hardcoded as string literals in components.
- REST Route templates are defined in `Mkx.Templates.Shared/Routes/ApiRoutes.cs` (used by controllers for routing attributes and client services for HTTP paths).
- Functional URL builders are defined in `Mkx.Templates.Shared/Routes/ApiUrls.cs` (to format routes with arguments like IDs or query parameters).
- UI navigation routes are defined in `Mkx.Templates.Shared/Routes/ClientRoutes.cs` (used to navigate the client app and specify routing on Blazor component pages).

---

## 4. Specification Pattern

To prevent repository classes from bloating with custom query methods (e.g., `GetActiveProductsByCategoryId`), database queries are written as strongly-typed specifications:
- Specifications derive from `SpecificationBase<TEntity>` (provided in the Sdk).
- They encapsulate query logic (where criteria, includes, ordering, pagination) into a single, unit-testable class.
- The Repository accepts the specification and applies it to the EF Core queryable.
