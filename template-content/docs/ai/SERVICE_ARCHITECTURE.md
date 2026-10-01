# Contracts, DI and error flow

Shared owns service contracts and DTOs. Application implements use cases; Server controllers delegate; Client implements the same contracts via HttpClient and ApiUrls. Interactive WASM uses HTTP. Static SSR account pages use host services and cannot depend on client-only storage/auth state.

Use `[ScopedService]` for ordinary implementation/validator/repository scanning. Register framework services and typed HTTP clients explicitly. Do not register a scanned class again. Dispose per-request HttpResponseMessage and propagate CancellationToken to HTTP/EF/validation calls.

`TestClientService` is the executable HTTP reference:

```csharp
using var response = await client.GetAsync(ApiUrls.Tests.Get(id), cancellationToken);
if (!response.IsSuccessStatusCode)
    throw await HttpRequestFailedException.GetExceptionAsync(response, cancellationToken);
return await response.Content.ReadFromJsonAsync<GetTestResponse>(jsonOptions, cancellationToken)
    ?? throw new UnexpectedHttpResponseException();
```

The same-origin AntiforgeryHandler gets an identity-bound request token for each mutation and sends X-CSRF-TOKEN. Global MVC antiforgery validation protects POST/PUT/DELETE, including logout. Do not cache tokens across account changes or bypass the handler for app API calls.

Expected failures use typed exceptions: validation/BadRequest/Argument = 400, Unauthorized = 401, Forbidden = 403, NotFound = 404, optimistic concurrency = 409. Unexpected failures = 500 with a generic public detail. Middleware emits application/problem+json, instance and traceId. MVC model validation returns ValidationProblemDetails. Never serialize a base ProblemDetails type when a derived validation object contains errors.

Client exception parsing is asynchronous and preserves field errors/status. New UI data flows use `TryRequestAsync<TService,TResponse>` and check Succeeded before updating state. LastRequestError and ValidationErrors support inline retry/forms. Compatibility SendRequestAsync overloads remain for existing pages, but their default-valued return must not be interpreted as success. A successful callback runs only after a successful action.

User-management saves perform checked Identity operations inside the existing ITransactionContext. DTOs have server-validated constraints; role/claim names must be registered/built-in. Authorization changes invalidate affected security stamps. UserDialog saves before returning a successful result. Avoid closing a dialog and then attempting persistence in its parent.
