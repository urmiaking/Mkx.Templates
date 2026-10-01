# Mkx.Templates.Blazor

A .NET 10 Blazor solution template with Persian RTL MudBlazor UI, SQL Server, Identity and a tested agent-oriented development path. Application source and durable instructions live in `template-content/`.

## Validate the application and package

```powershell
./scripts/verify-template.ps1
```

Requires .NET 10 SDK and Node.js 22+. The script builds/tests the source, packs the template, inspects package exclusions, installs into an isolated template hive, generates a solution named `Acme.Starter`, then builds/tests that generated solution. It never changes your normal template installation.

## Package and install

```powershell
dotnet pack Mkx.Templates.Blazor.csproj -c Release
dotnet new install bin/Release/Mkx.Templates.Blazor.1.0.0.nupkg
```

For editable local installation, use `dotnet new install ./template-content`.

## Generate

```powershell
dotnet new mkx-blazor -n MyCompany.MyProject -o MyCompany.MyProject
```

`-o` explicitly selects the output directory. The generated solution has six numbered folders: Server, Client, Shared, Core, Sdk and Tests (13 projects). It includes its own README, agent guide, package versions, EF tool manifest and optional local SQL compose configuration. Follow the generated README to configure database and first administrator secrets; no fixed administrator credentials ship with the template.

Pushes to `master` run template verification and publish a versioned package to NuGet after verification succeeds. Pull requests run verification without publishing.
