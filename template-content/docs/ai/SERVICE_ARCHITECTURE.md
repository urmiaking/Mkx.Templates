# Service Architecture & Interactive Auto Polymorphism: Mkx.Templates

This document details the mechanics of Service registrations, Dependency Injection (DI) scanning, client-server polymorphic resolution under **Blazor Interactive Auto rendering mode**, and exception/error handling.

---

## 1. Blazor Render Mode & WASM Loading

The application uses **InteractiveWebAssembly mode (no prerender)** for all interactive pages (everything except SSR auth pages like Login and AccessDenied). The render mode is configured in `App.razor.cs` and can be switched back to `InteractiveAutoRenderMode(true)` by changing a single line — the architecture is designed for reversibility.

```
                            ┌──────────────────────┐
                            │   IFeatureService    │  (Shared Layer)
                            └──────────┬───────────┘
                                       │
                                       ▼ (Resolved on WASM Client)
                              ┌──────────────────────┐
                              │ FeatureClientService │ (Client Layer)
                              └──────────┬───────────┘
                                         │
                                         │ HTTP REST Call
                                         ▼
                              ┌──────────────────────┐
                              │  FeaturesController  │ (Server Layer)
                              └──────────────────────┘
```

1. **WASM Loading Splash Screen**: Since prerendering is disabled, the server sends an empty HTML shell. A Cloudflare-style splash screen (logo + progress bar) is shown until the WASM runtime boots and the first Blazor component renders. The splash is automatically dismissed by `BaseLayout.OnAfterRenderAsync`.
2. **WebAssembly Execution**: The client container resolves `IFeatureService` to the client-side implementation (`FeatureClientService` in `Mkx.Templates.Client`), which communicates with the server via HTTP REST API calls.
3. **API Controllers**: The client services target REST controllers in the server layer (derived from `ApiControllerBase` in `Mkx.Templates.Server.Controllers`), which in turn invoke the server-side `FeatureService`.
4. **Authentication State**: The client uses a dual-strategy `PersistentAuthenticationStateProvider` that reads from `PersistentComponentState` (prerender on) or falls back to the `/api/Account/auth-state` API endpoint (prerender off).

---

## 2. Shared Routes & URL Builders

All API endpoints and UI pages are configured with strict route constants inside the `Shared` layer to keep client navigation and backend controllers in sync:
- **REST Route Templates**: Defined inside `Mkx.Templates.Shared.Routes.ApiRoutes`. These are used by API controllers (e.g., `[Route(ApiRoutes.Features.Base)]`) and HTTP clients.
- **Client Route Helpers**: Defined inside `Mkx.Templates.Shared.Routes.ApiUrls`. These are functional builders that accept route parameters (like IDs or query params) and build client-safe URL strings. **Client services must always use ApiUrls to construct endpoint paths rather than directly appending routes.**
- **Client Page Routes**: Defined inside `Mkx.Templates.Shared.Routes.ClientRoutes`. These constants map page UI addresses (e.g. `ClientRoutes.Tests.Index`) and are bound as route attributes in Blazor components (e.g. `@attribute [Route(ClientRoutes.Tests.Index)]`).

---

## 3. Client HTTP Implementation Code (Reference Model)

The following is the client-side implementation model of `I[Feature]Service` (`[Feature]ClientService`), showing how to construct HTTP requests (both GET and POST returning void) using `ApiUrls` helper endpoints:

```csharp
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Sdk.Shared.Exceptions;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.[Feature]s;
using Mkx.Templates.Shared.Routes;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mkx.Templates.Client.Services;

[ScopedService]
public class [Feature]ClientService(HttpClient client, JsonSerializerOptions jsonOptions) : I[Feature]Service
{
    // GET Demonstration returning DTO
    public async Task<Get[Feature]Response> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.[Feature]s.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<Get[Feature]Response>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    // POST Demonstration returning void (Task)
    public async Task CreateAsync(Create[Feature]Request request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.[Feature]s.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
```

---

## 4. Assembly Scanning & Dependency Injection

Registration is handled automatically by decorating concrete classes with custom SDK lifetime attributes:

- **`[ScopedService]`**: Registers the service with a scoped lifetime. Used by both `FeatureService` (Server) and `FeatureClientService` (Client).
- **`[TransientService]`**: Registers the service with a transient lifetime.
- **`[SingletonService]`**: Registers the service with a singleton lifetime.

---

## 5. Exception & Error Handling Flow

The error handling flow spans both server and client layers:
- **Server Side**: Controllers process calls. If validations or logic fail, application exceptions (like `NotFoundException` or `ValidationException`) are thrown. The server global exception middleware converts them to Problem Details JSON (`404 Not Found`, `400 Bad Request`).
- **Client Side**: If an HTTP call fails (`!response.IsSuccessStatusCode`), the client service converts the status code to a typed client exception (e.g. `HttpRequestFailedException` or `HttpRequestValidationException`).
- **UI Interaction**: Components calling services via `SendRequestAsync` in `AppComponentBase` automatically catch these exceptions and display formatted toast alerts to the user, resetting the loading state safely.
