# Commands

Run from the generated root unless stated otherwise. Prefer rg/rg --files for discovery. Paths in commands point at executable reference files; inspect them before introducing equivalents.

```powershell
dotnet tool restore
dotnet restore Mkx.Templates.slnx
dotnet build Mkx.Templates.slnx
dotnet test Mkx.Templates.slnx
./scripts/verify.ps1
```

Run the configured local host with `dotnet run --project src/Server/Mkx.Templates.Server`. See the generated README for database and administrator secrets. Do not infer an endpoint from source code when the launch profile prints it.

EF uses the checked-in local dotnet-ef manifest:

```powershell
dotnet ef migrations add MeaningfulName --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
dotnet ef migrations has-pending-model-changes --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
dotnet ef migrations script --idempotent --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server --output artifacts/migrations.sql
```

The design-time host needs ConnectionStrings:Mkx.Templates configured even though adding a migration does not apply it. Review migration Up/Down before applying a database update. Production startup flags are false by default; use reviewed scripts/bundles and deployment credentials.

Docker is optional. From the generated root:

```powershell
docker compose up -d --wait
docker build -t mkx.templates:local -f src/Server/Mkx.Templates.Server/Dockerfile .
```

Compose requires your own `.env` SQL password. The Dockerfile build context is the generated root, which contains central package files and nuget.config. Run Docker only when needed for the authorized task; do not publish an image unless requested.

When output is locked, identify only the server process you own before stopping it. Do not edit source to work around a running host's DLL lock. Report executed checks and exact blockers truthfully.
