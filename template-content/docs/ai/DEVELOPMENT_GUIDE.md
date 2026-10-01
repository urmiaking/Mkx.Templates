# Build a feature using the executable reference

Read [ARCHITECTURE.md](ARCHITECTURE.md), search for equivalents, then inspect the Test feature. Copy its flow, not placeholder pseudocode. There is one aggregate and one HTTP contract with separate server/client implementations.

| Step | Reference under `src/` |
|---|---|
| Business invariants, mutation, version | `Core/Mkx.Templates.Domain/TestAggregate/Test.cs` |
| EF lengths, ID conversion, concurrency | `Core/Mkx.Templates.Infrastructure/Configurations/TestConfiguration.cs` |
| Bounded search/sort/page and tracked lookup | `Core/Mkx.Templates.Infrastructure/Specifications/Tests/` |
| DTOs and service contract | `Shared/Mkx.Templates.Shared/DTOs/Tests/`, `Abstractions/ITestService.cs` |
| URL builders and route templates | `Shared/Mkx.Templates.Shared/Routes/ApiUrls.cs`, `ApiRoutes.cs` |
| Read/manage capability definitions | `Shared/Mkx.Templates.Shared/Authorization/` |
| Validation, orchestration, mapping | `Server/Mkx.Templates.Application/Validators/Tests/`, `Services/TestService.cs`, `Mappers/TestMapper.cs` |
| Thin authorized controller | `Server/Mkx.Templates.Server/Controllers/TestsController.cs` |
| Async HTTP/status handling | `Client/Mkx.Templates.Client/Services/TestClientService.cs` |
| Server-data table, URL search, errors/retry | `Client/Mkx.Templates.Client/Pages/Tests.razor` and `.razor.cs` |
| Save-in-place dialog, validation, dirty guard | `Client/Mkx.Templates.Client/Components/Tests/TestEditor.razor` and `.razor.cs` |
| Executable behavior checks | `Tests/Mkx.Templates.Tests/DomainAndContractsTests.cs`, `ApiTests.cs` (from repository root) |

The table uses `PagedList<T>` and normalized `RequestFilter` (default 25, max 100). Search is debounced and reflected in the URL. Supported sorting is Name with stable ID tie-breaking. A Guid Version changes on update; PUT/DELETE require the last observed version and return 409 when stale. EF's concurrency token also protects the race between read and write. A failed request is distinct from an empty result.

Create/update validators execute on the server. Domain also guards its invariants for non-HTTP callers. The UI validates basic constraints, displays server field errors and prevents duplicate submission. The editor performs the request before closing, preserves input on failure and guards navigation/cancel with unsaved changes.

For a new capability: add an AppPolicies constant, register it in AppPolicyProvider, enforce it on the API and use it in the UI. Both Client and Server register the shared provider. Unknown policies fail closed. RoleSeeder grants registered policy claims to the Administrators role when seeding is enabled; tests must also cover missing claims.

Add a migration only for schema changes, inspect it and test persistence. Use [FEATURE_CHECKLIST.md](FEATURE_CHECKLIST.md) and [VERIFICATION.md](VERIFICATION.md) before reporting completion. Delete the sample only after establishing an equivalent tested reference for the generated product.
