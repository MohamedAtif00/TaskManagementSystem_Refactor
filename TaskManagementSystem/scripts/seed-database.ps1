# Seeds integration / local dev test user (TST001) into TaskManagementSystem SQL Server.
# Usage:
#   ./scripts/seed-database.ps1 [-ConnectionString "..."] [-WhatIf] [-AllowAnyDatabase]

param(
    [string]$ConnectionString = "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True",
    [switch]$WhatIf,
    [switch]$AllowAnyDatabase
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$seedScript = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/Seeds/002_IntegrationTestData.sql"
$migratorProject = Join-Path $repoRoot "src/Database/DatabaseMigrator/DatabaseMigrator.csproj"

function Get-DatabaseName([string]$ConnectionString) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $ConnectionString
    return $builder.InitialCatalog
}

$dbName = Get-DatabaseName $ConnectionString
if (-not $AllowAnyDatabase -and $dbName -notmatch 'TaskManagementSystem|TmsTests_') {
    throw "Refusing to seed database '$dbName'. Pass -AllowAnyDatabase to override."
}

Write-Host "Target database: $dbName"
Write-Host "Connection: $($ConnectionString -replace 'Password=[^;]+', 'Password=***')"

if ($WhatIf) {
    Write-Host "[WhatIf] Would run seed via DatabaseMigrator --seed:"
    Write-Host "  $seedScript"
    exit 0
}

if (-not (Test-Path $seedScript)) {
    throw "Seed script not found: $seedScript"
}

Write-Host "Running integration test seed..."
dotnet run --project $migratorProject -- `
    --seed `
    $ConnectionString `
    $seedScript

Write-Host "Done."
