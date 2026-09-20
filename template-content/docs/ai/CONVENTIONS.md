# Coding & Project Conventions

Before creating services, repositories, components, helpers, exceptions, DTOs, routes, policies, CSS classes, specifications, or abstractions, search for an equivalent and inspect the closest implementation. Reuse established patterns first.

## Naming
- Aggregates/entities: singular nouns; IDs: `[Entity]Id`.
- Contracts: `I[Feature]Service`; server: `[Feature]Service`; WASM: `[Feature]ClientService`.
- DTOs: `Create[Feature]Request`, `Update[Feature]Request`, `Get[Feature]Response`.
- EF: `[Entity]Configuration`; validators: `[Request]Validator`; Mapster: `[Feature]Mapper`; specs use intent-revealing names.

## C# and Placement
Respect nullable reference types and propagate `CancellationToken` through I/O. Use `Guid.CreateVersion7()` for new Guid-backed domain IDs unless requirements dictate otherwise. Prefer immutable transport records. Do not expose aggregate setters for binding/mapping convenience.

Domain owns invariants. Infrastructure owns EF/persistence/specifications. Shared owns DTOs/contracts/routes/policies. Application owns use cases/validation/mapping. Server owns HTTP/hosting. Client owns Blazor and HTTP implementations.

Never hardcode routes when `ApiRoutes`, `ApiUrls`, or `ClientRoutes` applies. Use SDK lifetime attributes and do not duplicate auto-scanned DI registrations. Non-trivial Blazor pages use markup + code-behind, `AppComponentBase`, and `SendRequestAsync`. Prefer MudBlazor, existing components, central theme-aware CSS, and MudBlazor palette variables.
