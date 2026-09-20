# Tools & CLI Guide: Mkx.Templates

Use this guide for repository discovery, build, test, run, and EF Core commands. Commands assume the generated repository root unless stated otherwise.

## 1. Restore & Build
```powershell
dotnet restore Mkx.Templates.slnx
dotnet build Mkx.Templates.slnx
```

For a fast client-only compile while working on Blazor UI:
```powershell
dotnet build src/Client/Mkx.Templates.Client/Mkx.Templates.Client.csproj --no-restore
```

The full solution build is the final compilation check.

## 2. Run
```powershell
dotnet run --project src/Server/Mkx.Templates.Server/Mkx.Templates.Server.csproj
```

Development endpoints are configured by the project launch settings. Do not hardcode an assumed port into application code.

## 3. Repository Discovery
Search before creating duplicate abstractions. PowerShell is always an acceptable fallback when `rg` is unavailable.

Find a symbol/pattern:
```powershell
Get-ChildItem src -Recurse -Include *.cs,*.razor |
    Select-String -Pattern 'IUserManagementService|SendRequestAsync'
```

Find files:
```powershell
Get-ChildItem src -Recurse -Filter '*Repository*.cs'
```

Find route/policy definitions:
```powershell
Get-ChildItem src -Recurse -Include *.cs |
    Select-String -Pattern 'ApiRoutes|ApiUrls|ClientRoutes|AppPolicies'
```

## 4. Entity Framework Migrations
Add:
```powershell
dotnet ef migrations add <MigrationName> --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
```

Remove the latest undeployed migration:
```powershell
dotnet ef migrations remove --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
```

Apply:
```powershell
dotnet ef database update --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
```

Inspect generated migrations before applying them. See [DATABASE.md](DATABASE.md).

The current host may apply pending migrations during startup. Verify current startup configuration before changing that behavior.

## 5. Database Seeders
System/initial metadata uses the established `IDbSeeder` scanning mechanism.
- Put application seeders in the established Application seeders area.
- Use the project's DI scanning convention.
- Make seeders idempotent.
- Use `Order` only for real dependencies between seeders.

## 6. Tests
Run all tests:
```powershell
dotnet test
```

Target a test/class:
```powershell
dotnet test --filter "FullyQualifiedName~YourTestClass"
```

Follow [VERIFICATION.md](VERIFICATION.md) for which checks are required by change type.

## 7. Troubleshooting Build Locks
If build output reports MSB3021/MSB3027 file-lock errors, identify a running server/debug host before changing source code to work around the error.

Example:
```powershell
Get-Process Mkx.Templates.Server,dotnet -ErrorAction SilentlyContinue
```

Stop/restart only the process you intentionally own/control, then rebuild. A locked DLL is an environment issue, not evidence of a source compilation failure.

## 8. Command Reporting
Agents must report commands truthfully:
- exit code 0 / successful output can be reported as passed;
- blocked/not-run checks must be reported as such;
- do not infer runtime/browser success from a compile-only check.
