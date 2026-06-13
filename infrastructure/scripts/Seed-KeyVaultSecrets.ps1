[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$VaultName,

    [Parameter(Mandatory = $true)]
    [string]$ManifestPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw 'Azure CLI (az) is required and was not found in PATH.'
}

if (-not (Test-Path -LiteralPath $ManifestPath)) {
    throw "Manifest file not found: $ManifestPath"
}

$manifestContent = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
if (-not $manifestContent.secrets) {
    throw 'Manifest must contain a top-level ''secrets'' array.'
}

foreach ($secret in $manifestContent.secrets) {
    if (-not $secret.name -or -not $secret.value) {
        throw 'Each secret entry must contain ''name'' and ''value'' properties.'
    }

    $secretName = [string]$secret.name
    $secretValue = [string]$secret.value

    if ($PSCmdlet.ShouldProcess("$VaultName/$secretName", 'Set Key Vault secret')) {
        az keyvault secret set `
            --vault-name $VaultName `
            --name $secretName `
            --value $secretValue | Out-Null

        Write-Host "Seeded secret: $secretName" -ForegroundColor Green
    }
}

Write-Host "Completed Key Vault secret seeding for vault: $VaultName" -ForegroundColor Cyan
