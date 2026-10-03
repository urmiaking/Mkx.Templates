param([string]$Configuration = "Release")
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet build Mkx.Templates.slnx -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Solution build failed." }
    $results = Join-Path ([IO.Path]::GetTempPath()) ("mkx-tests-" + [guid]::NewGuid().ToString("N"))
    dotnet test --solution Mkx.Templates.slnx -c $Configuration --no-build -- --results-directory $results --report-xunit-trx --report-xunit-trx-filename tests.trx
    if ($LASTEXITCODE -ne 0) { throw "Tests failed." }
    $reports = Get-ChildItem -LiteralPath $results -Filter *.trx -Recurse
    $executed = 0
    foreach ($report in $reports) {
        [xml]$trx = Get-Content -LiteralPath $report.FullName
        $executed += [int]$trx.TestRun.ResultSummary.Counters.executed
    }
    if ($executed -eq 0) { throw "No tests executed." }
    node --test Tests/browser/pwa.test.mjs
    if ($LASTEXITCODE -ne 0) { throw "PWA behavior tests failed." }
    Write-Host "Verified $executed .NET tests and PWA behavior tests. Results: $results"
} finally { Pop-Location }
