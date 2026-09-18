param(
    [ValidateSet('env', 'scenario', 'all')]
    [string]$Target = 'all',
    [string]$Scenario = 'all-transactions',
    [switch]$OpenFiles
)

$ErrorActionPreference = 'Stop'
$apidogRoot = Join-Path $PSScriptRoot '../src/Api/TaskManagementSystem.Api/openapi/apidog'
$apidogRoot = [System.IO.Path]::GetFullPath($apidogRoot)

$envPostman = Join-Path $apidogRoot 'environment.local-dev.postman.json'
$scenarioFile = if ($Scenario -eq 'get-queries') {
    Join-Path $apidogRoot 'test-scenario/get-queries-timing.json'
} else {
    Join-Path $apidogRoot 'test-scenario/all-transactions-timing.json'
}

function Write-ImportStep {
    param([int]$Number, [string]$Text)
    Write-Host ""
    Write-Host "$Number. $Text" -ForegroundColor Cyan
}

Write-Host "=== TMS Apidog import helper ===" -ForegroundColor Green
Write-Host "Apidog folder: $apidogRoot"

if ($Target -in @('env', 'all')) {
    if (-not (Test-Path $envPostman)) {
        throw "Missing environment file: $envPostman"
    }

    Write-ImportStep 1 "Import environment (Postman format)"
    Write-Host "   File: $envPostman"
    Write-Host "   In Apidog: Environments (gear icon) -> Import -> Postman -> select the file above."
    Write-Host "   Do NOT use Project Settings -> Import for the environment file."
}

if ($Target -in @('scenario', 'all')) {
    if (-not (Test-Path $scenarioFile)) {
        throw "Missing test scenario file: $scenarioFile"
    }

    Write-ImportStep 2 "Import test scenario"
    Write-Host "   File: $scenarioFile"
    Write-Host "   In Apidog: Tests -> + (or folder ...) -> Import -> Apidog format -> select the file above."
    Write-Host "   Alternative: right-click a test folder -> Import Data -> Apidog."
}

Write-ImportStep 3 "Select environment before running"
Write-Host "   Choose 'TMS Local Dev' in the test scenario run panel."
Write-Host "   Ensure API is running at http://localhost:61173 and TST001 is seeded."

Write-ImportStep 4 "Run"
Write-Host "   Open the imported scenario and click Run."
Write-Host "   Login step sets accessToken and runSuffix automatically."

if ($OpenFiles) {
    foreach ($path in @($envPostman, $scenarioFile)) {
        if ($Target -eq 'env' -and $path -ne $envPostman) { continue }
        if ($Target -eq 'scenario' -and $path -ne $scenarioFile) { continue }
        if (Test-Path $path) {
            Start-Process $path
        }
    }
}

Write-Host ""
Write-Host "Done. Copy the paths above into Apidog import dialogs." -ForegroundColor Green
