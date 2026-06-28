# Agent Guidelines & Skills: Mkx.Templates

Follow these rules and best practices at all times when adding or refactoring code inside this project.

---

## 1. Domain Aggregate Encapsulation
- **Aggregate Isolation**: Ensure aggregate properties have `private set` or `protected set` access. Mutate entities solely through internal domain methods (e.g., `Update(name, description)`) to ensure business invariants are validated.
- **Factory Methods**: Avoid exposing public constructors for aggregate roots. Use static `Create(...)` factory methods.
- **Identities**: Utilize strong-typed record structs wrapping a Guid for entity IDs (e.g. `public readonly record struct FeatureId(Guid Value);`).

---

## 2. API Controllers & Routing
- **Thin Controllers**: Controllers should only handle incoming HTTP request parsing, model validations, and outgoing responses. Delegate all orchestrations and business logic to Application Services.
- **Constant Routing**: Do not hardcode route paths in controllers, client services, or page links. Always define and refer to routes inside `Mkx.Templates.Shared/Routes/ApiRoutes.cs` (for REST routes), `Mkx.Templates.Shared/Routes/ApiUrls.cs` (for client HTTP URL formatting), and `Mkx.Templates.Shared/Routes/ClientRoutes.cs` (for page layout routing).

---

## 3. UI Components & Styling (MudBlazor)
- **Centralized Styling**: Write all custom styling (e.g., glassmorphism, glowing shines, radial gradients) in the central `app.css` stylesheet located under `src/Server/Mkx.Templates.Server/wwwroot/css/app.css` instead of writing inline CSS or component-scoped CSS.
- **CSS Variables for Theming**: Bind custom class definitions to native MudBlazor variables such as `rgba(var(--mud-palette-surface-rgb), 0.45)` or `var(--mud-palette-primary)`. This allows layouts to dynamically adapt to light/dark modes and remain resilient during Server-Side Rendering (SSR) page runs.
- **Prerendering Compatibility**: Do not inject scoped client services like `ThemeService` or interactive state providers inside static SSR pages (such as Login, AccessDenied, or NotFound). Rely entirely on standard theme-driven CSS layout variables inside the stylesheet.

---

## 4. UI Call Invocation (SendRequestAsync)
- Do not inject and call HTTP client services or raw application services directly using simple try-catch blocks in Blazor UI components.
- Always inherit from `AppComponentBase` and call services inside the `SendRequestAsync(...)` wrapper.
- Utilize the overloads accepting `afterSend` and `onFailure` callback arguments to bind state mutations directly on successful execution and define fallback/cleanup behavior on failure.
- This manages the `IsBusy` rendering state, handles token cancellations, and automatically intercepts, formats, and displays standard business exception toasts (e.g. invalid arguments, validation errors, resource not found) inside MudBlazor Snackbars.
