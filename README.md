# Mkx.Templates.Blazor

This is a Visual Studio and dotnet CLI template for a Blazor solution utilizing .NET 10 and the modern `.slnx` solution format.

## Template structure

When instantiated, the template generates:
- An XML-based `.slnx` solution file containing five numbered folders:
  - `1. App`
  - `2. Shared`
  - `3. Core`
  - `4. Sdk`
  - `5. Tests`
- A `nuget.config` file.
- A `src/` directory containing the corresponding `App`, `Shared`, `Core`, `Sdk`, and `Tests` directories.

---

## How to Package

To build the NuGet package, run the following command from the root directory:

```powershell
dotnet pack -c Release
```

This will produce the NuGet package file under `bin/Release/Mkx.Templates.Blazor.1.0.0.nupkg`.

---

## How to Install Locally

To install the template from the generated package:

```powershell
dotnet new install bin/Release/Mkx.Templates.Blazor.1.0.0.nupkg
```

Alternatively, to install it in editable development mode from the local directory:

```powershell
dotnet new install ./template-content
```

---

## How to Instantiate

Once installed, you can create a new solution by running:

```powershell
dotnet new mkx-blazor -n MyCompany.MyProject
```

This will create a new folder named `MyCompany.MyProject` containing your `.slnx` solution and folder structure.
