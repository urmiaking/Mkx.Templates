# Coding & Project Conventions

Before creating services, repositories, components, helpers, exceptions, DTOs, routes, policies, CSS classes, specifications, or abstractions, search for an equivalent and inspect the closest implementation. Reuse established patterns first.

## Naming
- Aggregates/entities: singular nouns; IDs: `[Entity]Id`.
- Contracts: `I[Feature]Service`; server: `[Feature]Service`; WASM: `[Feature]ClientService`.
- DTOs: `Create[Feature]Request`, `Update[Feature]Request`, `Get[Feature]Response`.
- EF: `[Entity]Configuration`; validators: `[Request]Validator`; Mapster: `[Feature]Mapper`; specs use intent-revealing names.

## NuGet dependencies
Manage exact stable package versions in `Directory.Packages.props`; keep project `PackageReference` entries versionless. Upgrade Microsoft ASP.NET Core/EF Core packages together on the application's supported .NET release. Check NuGet.org for current releases and run restore, the solution tests, and template generation verification after upgrades. Transitive pinning is enabled: `System.Security.Cryptography.Pkcs` is pinned to the patched .NET 10 version because SqlClient still requests a vulnerable 9.x minimum. Update parent dependencies first, retain an explicit pin while it is needed, and audit transitive packages rather than suppressing vulnerability warnings.

## C# and Placement
Respect nullable reference types and propagate `CancellationToken` through I/O. Use `Guid.CreateVersion7()` for new Guid-backed domain IDs unless requirements dictate otherwise. Prefer immutable transport records. Do not expose aggregate setters for binding/mapping convenience.

Domain owns invariants. Infrastructure owns EF/persistence/specifications. Shared owns DTOs/contracts/routes/policies. Application owns use cases/validation/mapping. Server owns HTTP/hosting. Client owns Blazor and HTTP implementations.

Never hardcode routes when `ApiRoutes`, `ApiUrls`, or `ClientRoutes` applies. Use SDK lifetime attributes and do not duplicate auto-scanned DI registrations. Non-trivial Blazor pages use markup + code-behind, `AppComponentBase`, and `TryRequestAsync` (explicit success; legacy `SendRequestAsync` remains for compatibility). Use semantic HTML, the existing Components/UI primitives, central app.css and --ui-* theme tokens.
