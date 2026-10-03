# Mkx.Templates

.NET 10 Blazor template with a Persian RTL interface, MudBlazor, SQL Server, Identity, policies and a tested CRUD reference. Start agent work at [AGENTS.md](AGENTS.md). The solution contains 12 application/SDK projects and one executable test project.

## Prerequisites

Install .NET 10 SDK. `global.json` selects a stable 10.0 feature band. Node.js 22+ runs the dependency-free PWA tests. SQL Server is required to run the application; integration tests use isolated in-memory SQLite and need no external database. Optional local SQL setup requires Docker with Linux containers.

## Local start

From this generated directory:

```powershell
dotnet tool restore
dotnet restore Mkx.Templates.slnx
dotnet dev-certs https --trust
```

Windows with a local SQL Server instance can use the password-free integrated-security connection in `src/Server/Mkx.Templates.Server/appsettings.json`; no user-secret is required for that setup. The connection key and database name are `Mkx.Templates`. When creating a project with `-n Acme.Starter`, both become `Acme.Starter`, and a new UserSecretsId is generated. For other installations, edit that connection or optionally override `ConnectionStrings:Mkx.Templates` with user-secrets in Development:

```powershell
$server = 'src/Server/Mkx.Templates.Server'
dotnet user-secrets set 'ConnectionStrings:Mkx.Templates' 'Server=127.0.0.1,14333;Database=Mkx.Templates;User ID=sa;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=true' --project $server
```

The SQL password must be your own. For the optional container, copy `.env.example` to `.env`, fill its password, then run `docker compose up -d --wait`. The port binds to localhost. SQL Developer is for development; choose appropriate licensing and credentials for deployment. Keep `.env` out of source control.

For ordinary local execution, use the Development launch profile:

```powershell
dotnet run --project src/Server/Mkx.Templates.Server --launch-profile Server
```

Running without a launch profile or running the published DLL defaults to Production, which does not automatically apply migrations or load user-secrets. The base local connection still works when SQL Server is available.

There is no preconfigured administrator or fixed password. To bootstrap your first administrator:

```powershell
$server = 'src/Server/Mkx.Templates.Server'
$adminPassword = Read-Host 'Choose an administrator passphrase (at least 12 characters)' -MaskInput
dotnet user-secrets set 'BootstrapAdmin:Enabled' 'true' --project $server
dotnet user-secrets set 'BootstrapAdmin:Username' 'your-admin-name' --project $server
dotnet user-secrets set 'BootstrapAdmin:Password' $adminPassword --project $server
$adminPassword = $null
dotnet run --project $server
```

Use the HTTPS URL printed by the launch profile. Development applies pending migrations and seeds built-in role policies. Bootstrap is opt-in and creates an account only when the configured username does not exist. After the first start, set `BootstrapAdmin:Enabled=false` and remove `BootstrapAdmin:Password` from secrets. Identity validates the password; invalid credentials stop seeding with a clear startup error.

## Verify and extend

```powershell
./scripts/verify.ps1
```

This builds Release, executes .NET tests, fails when no tests run, and runs PWA behavior tests. From the generated root, `dotnet build Mkx.Templates.slnx` and `dotnet test --solution Mkx.Templates.slnx` are the quick checks. `global.json` selects Microsoft.Testing.Platform for xUnit package 4.x; VSTest runner options such as `--logger` do not apply. The reference starts at `src/Client/Mkx.Templates.Client/Pages/Tests.razor` and is mapped in [DEVELOPMENT_GUIDE.md](docs/ai/DEVELOPMENT_GUIDE.md).

## Production configuration

Override the local SQL connection for the deployment host, using `ConnectionStrings:Mkx.Templates` in your deployment configuration or the environment variable `ConnectionStrings__Mkx.Templates`. User-secrets are loaded automatically only in Development. Configure TLS and trusted reverse proxies. Production defaults disable automatic migrations and seeding; SQL logging/UI is enabled by default. Apply a reviewed migration script/bundle with deployment credentials before starting the application. `/health/live` checks the host; `/health/ready` checks database connectivity and pending migrations. Use [OPERATIONS.md](docs/ai/OPERATIONS.md) for deployment, SMS, feature flags and optional integrations.

PWA registration is production-only and controlled by `Features:Pwa`. It caches public static assets and shows an offline information page; authenticated workflows require a connection. Updates wait for a user action. The UI defaults to opaque surfaces on phones and automatically respects the operating system reduced-motion preference.

The original default font is IRANSans (FaNum) Medium; its local assets and the Google font assets used by the Cinzel Decorative brand are included in generated content and published assets. Typography, palette sizes and theme identity should be preserved when optimizing performance. Vazirmatn remains a fallback; its SIL OFL license is bundled under `wwwroot/fonts/vazir/OFL.txt`.

SQL logging and the administrator-only `/serilog-ui` page are enabled by default (`Logging:UseSqlStore=true`). Navigate to this page with a full server load. Set the flag to false only when deliberately disabling this feature.
