param([string]$Configuration = "Release")
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$verification = Join-Path ([IO.Path]::GetTempPath()) ("mkx-template-" + [guid]::NewGuid().ToString("N"))
$packageOutput = Join-Path $verification "packages"
$hive = Join-Path $verification "hive"
$generated = Join-Path $verification "Acme.Starter"
Push-Location $root
try {
    & ./template-content/scripts/verify.ps1 -Configuration $Configuration
    dotnet pack Mkx.Templates.Blazor.csproj -c $Configuration -o $packageOutput
    if ($LASTEXITCODE -ne 0) { throw "Packing failed." }
    $package = Get-ChildItem -LiteralPath $packageOutput -Filter *.nupkg | Select-Object -First 1
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $entries = @($zip.Entries | ForEach-Object FullName)
        $forbidden = $entries | Where-Object { $_ -match '/(bin|obj|TestResults|node_modules|artifacts|\.vs|\.git)/|/(appsettings\.Local\.json|\.env)$|\.(user|pfx|log)$|/\.env\.(?!example$)' }
        if ($forbidden) { throw "Unexpected package entries: $($forbidden -join ', ')" }
        foreach ($required in @('content/AGENTS.md','content/README.md','content/Directory.Packages.props','content/.env.example','content/scripts/verify.ps1','content/src/Server/Mkx.Templates.Server/wwwroot/fonts/IRANSANS/IRANSANS-font-face.css','content/src/Server/Mkx.Templates.Server/wwwroot/fonts/IRANSANS/woff2/IRANSansWeb(FaNum)_Medium.woff2','content/src/Server/Mkx.Templates.Server/wwwroot/fonts/Google/google-fonts.css')) {
            if ($required -notin $entries) { throw "Package is missing $required" }
        }
    } finally { $zip.Dispose() }
    dotnet new install $package.FullName --debug:custom-hive $hive
    if ($LASTEXITCODE -ne 0) { throw "Isolated installation failed." }
    dotnet new mkx-blazor -n Acme.Starter -o $generated --debug:custom-hive $hive --no-update-check
    if ($LASTEXITCODE -ne 0) { throw "Instantiation failed." }
    if (-not (Test-Path -LiteralPath (Join-Path $generated '.env.example'))) { throw "Generated configuration example is missing." }
    foreach ($font in @('IRANSANS/IRANSANS-font-face.css','IRANSANS/woff2/IRANSansWeb(FaNum)_Medium.woff2','Google/google-fonts.css')) {
        if (-not (Test-Path -LiteralPath (Join-Path $generated "src/Server/Acme.Starter.Server/wwwroot/fonts/$font"))) { throw "Generated font is missing: $font" }
    }
    $generatedSettings = Get-Content -LiteralPath (Join-Path $generated 'src/Server/Acme.Starter.Server/appsettings.json') -Raw | ConvertFrom-Json
    $generatedConnection = $generatedSettings.ConnectionStrings.'Acme.Starter'
    if ($generatedConnection -notmatch 'Database=Acme\.Starter;' -or $generatedSettings.ConnectionStrings.PSObject.Properties.Name -contains 'Mkx.Templates' -or $generatedSettings.ConnectionStrings.PSObject.Properties.Name -contains 'Default') {
        throw "Connection key/database did not use the generated project name."
    }
    [xml]$sourceProject = Get-Content -LiteralPath 'template-content/src/Server/Mkx.Templates.Server/Mkx.Templates.Server.csproj' -Raw
    [xml]$generatedProject = Get-Content -LiteralPath (Join-Path $generated 'src/Server/Acme.Starter.Server/Acme.Starter.Server.csproj') -Raw
    if ($sourceProject.Project.PropertyGroup.UserSecretsId -eq $generatedProject.Project.PropertyGroup.UserSecretsId) { throw "Generated project shares the template UserSecretsId." }
    Push-Location $generated
    try { & ./scripts/verify.ps1 -Configuration $Configuration } finally { Pop-Location }
    Write-Host "Template verified. Package and generated solution: $verification"
} finally { Pop-Location }
