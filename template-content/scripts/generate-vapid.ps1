# Generates a unique RFC 8292 P-256 VAPID key pair for THIS application.
# Never commit or include appsettings.Push.local.json in the distributed template.
param(
    [string]$Subject = "mailto:admin@example.com",
    [switch]$Force
)

$ErrorActionPreference = "Stop"
if (-not ($Subject.StartsWith('mailto:') -or $Subject.StartsWith('https://'))) {
    throw "Subject must be a mailto: address or HTTPS URL."
}
$serverDirectory = Join-Path $PSScriptRoot "../src/Server/Mkx.Templates.Server"
$target = Join-Path $serverDirectory "appsettings.Push.local.json"
if ((Test-Path $target) -and -not $Force) {
    Write-Output "Local VAPID configuration already exists; use -Force only to deliberately rotate keys."
    exit 0
}
function To-Base64Url([byte[]]$Bytes) {
    return [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}
$ecdsa = [System.Security.Cryptography.ECDsa]::Create([System.Security.Cryptography.ECCurve]::CreateFromFriendlyName("nistP256"))
try {
    $parameters = $ecdsa.ExportParameters($true)
    $public = [byte[]]::new(65)
    $public[0] = 4
    [Array]::Copy($parameters.Q.X, 0, $public, 1, 32)
    [Array]::Copy($parameters.Q.Y, 0, $public, 33, 32)
    $data = @{ Push = @{
        Enabled = $true
        Subject = $Subject
        PublicKey = To-Base64Url $public
        PrivateKey = To-Base64Url $parameters.D
    } }
    $data | ConvertTo-Json -Depth 5 | Set-Content -Path $target -Encoding utf8
    Write-Output "Generated unique VAPID keys into local ignored appsettings.Push.local.json."
} finally { $ecdsa.Dispose() }