# Development Guide: Mkx.Templates

Use this guide as a step-by-step blueprint to implement any new feature, aggregate root, or business module in the system.

---

## The Generic Feature Implementation Flow

Below is the standard flow for adding a new business entity named `[Feature]` (replace `[Feature]` with your actual aggregate name, e.g. `Product`, `Customer`):

```
[ Domain Aggregate ] ──> [ EF Configuration ] ──> [ Query Specifications ]
                                                        │
[ Blazor UI Page ] <── [ Client HTTP Service ] <── [ API Controller ] <── [ App Service ]
```

### Step 1: Define the Domain Aggregate
Create a new aggregate folder `[Feature]Aggregate/` in `Mkx.Templates.Domain` and define the ID and main Entity:

- **Entity ID**: A readonly record struct wrapping a Guid.
- **Aggregate Root**: Extends `EntityBase<[Feature]Id>`. Use private setters to protect invariants and expose static factory methods (e.g., `Create`) and entity mutation methods (e.g., `Update`).

*Template Example (`[Feature].cs`):*
```csharp
using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Domain.[Feature]Aggregate;

public readonly record struct [Feature]Id(Guid Value);

public class [Feature] : EntityBase<[Feature]Id>
{
    private [Feature]() { } // For EF Core materialization

    private [Feature](string name)
    {
        Id = new [Feature]Id(Guid.CreateVersion7());
        Name = name;
    }

    public static [Feature] Create(string name) => new(name);

    public string Name { get; private set; }

    public void UpdateName(string newName) => Name = newName;
}
```

### Step 2: Database Configuration
In `Mkx.Templates.Infrastructure/Configurations/`, create an EF Core configuration class to define table details, database types, keys, and strong-typed ID conversions.

*Template Example (`[Feature]Configuration.cs`):*
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mkx.Templates.Domain.[Feature]Aggregate;

namespace Mkx.Templates.Infrastructure.Configurations;

internal sealed class [Feature]Configuration : IEntityTypeConfiguration<[Feature]>
{
    public void Configure(EntityTypeBuilder<[Feature]> builder)
    {
        builder.ToTable("[Feature]s");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new [Feature]Id(value));
            
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
    }
}
```

### Step 3: Write Query Specifications
Under `Mkx.Templates.Infrastructure/Specifications/[Feature]s/`, write query specification files to encapsulate query conditions. Do not put LINQ logic directly into your services.

*Template Example (`[Feature]ByIdSpecification.cs`):*
```csharp
using Mkx.Templates.Sdk.Server.Infrastructure.Specifications;
using Mkx.Templates.Domain.[Feature]Aggregate;

namespace Mkx.Templates.Infrastructure.Specifications.[Feature]s;

public class [Feature]ByIdSpecification : SpecificationBase<[Feature]>
{
    public [Feature]ByIdSpecification([Feature]Id id)
    {
        AddCriteria(x => x.Id == id);
    }
}
```

### Step 4: Create the Repository Interface & Implementation
Define repository contracts and concrete persistence classes if custom database operations are needed:
- Define `I[Feature]Repository` in `Mkx.Templates.Infrastructure/Repositories/Abstractions/` extending `IRepository<[Feature]>`.
- Implement `[Feature]Repository` in `Mkx.Templates.Infrastructure/Repositories/`.

### Step 5: Declare Service Contracts and DTOs
Define DTO models and the common interface in `Mkx.Templates.Shared`:
- **DTOs**: Write lightweight immutable C# `record` models under `DTOs/[Feature]s/`.
- **Interface**: Write the interface in `Abstractions/` (e.g., `I[Feature]Service.cs`).

*Template Example (`I[Feature]Service.cs`):*
```csharp
using Mkx.Templates.Shared.DTOs.[Feature]s;

namespace Mkx.Templates.Shared.Abstractions;

public interface I[Feature]Service
{
    Task<List<Get[Feature]Response>> GetAllAsync(CancellationToken cancellationToken = default);
}
```

### Step 6: Implement Server-Side Application Logic
Create the service implementation in `Mkx.Templates.Application/Services/[Feature]Service.cs` and configure mapping:
- Decorate implementation with `[ScopedService]` attribute.
- Map domain aggregates to response DTOs using Mapster configuration in `Mappers/[Feature]Mapper.cs` implementing `IRegister`.

### Step 7: Expose API Controllers
Under `Mkx.Templates.Server/Controllers/`, add a Web API controller extending `ApiControllerBase` to expose the service to REST calls. Use route templates defined in `ApiRoutes`.

### Step 8: Implement Client-Side HTTP Service
Create the client-side service implementation `[Feature]ClientService.cs` in `Mkx.Templates.Client/Services/`:
- Decorate implementation with `[ScopedService]` attribute.
- Use `HttpClient` to fetch data from the Controller endpoints, mapping URLs using `ApiRoutes` or `ApiUrls` helpers.
- Use `HttpRequestFailedException` to raise standard HTTP failure codes.

### Step 9: Integrate with Blazor WASM UI Component (Separate Markup & Code-Behind)
Blazor components must be separated into markup (`.razor`) and partial code-behind class files (`.razor.cs`) under `Mkx.Templates.Client/Pages/[Feature]s/`:

#### 1. Markup File (`[Feature]s.razor`)
Contains page routing, authorization attributes, imports, layout components, and markup templates:
```razor
@attribute [Route(ClientRoutes.[Feature]s.Index)]
@attribute [Authorize]
@inherits AppComponentBase

<PageTitle>Management of [Feature]s</PageTitle>

<MudTable Items="_items" Loading="IsBusy">
    <HeaderContent>
        <MudTh>Name</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Name</MudTd>
    </RowTemplate>
</MudTable>
```

#### 2. Code-Behind File (`[Feature]s.razor.cs`)
Contains the execution logic and state management. Fetch backend data inside `AppComponentBase.SendRequestAsync` using `afterSend` and `onFailure` callback overloads:
```csharp
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.[Feature]s;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Client.Pages.[Feature]s;

public partial class [Feature]s
{
    private List<Get[Feature]Response> _items = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDataAsync()
    {
        await SendRequestAsync<I[Feature]Service, List<Get[Feature]Response>>(
            action: (service, cancellationToken) => service.GetAllAsync(cancellationToken),
            afterSend: (response) => _items = response,
            onFailure: () => _items = []
        );
    }
}
```

---

## System Test Aggregate
The repository comes pre-bundled with a `Test` aggregate matching this exact pattern. This is an illustrative feature example to showcase real-world clean architecture flow. When you begin implementing the actual business requirements, the `Test` aggregates, services, and routes can be safely deleted or replaced.
