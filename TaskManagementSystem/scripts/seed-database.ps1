# Seeds integration / local dev data into TaskManagementSystem SQL Server:
#   002_IntegrationTestData.sql  TST001 + Integration Test Team
#   003_LoCodeCurriculum.sql     sample LOs (Mth_5R_1A_01_04_02, ...)
# Usage:
#   ./scripts/seed-database.ps1 [-ConnectionString "..."] [-WhatIf] [-AllowAnyDatabase]

param(
    [string]$ConnectionString = "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True",
    [switch]$WhatIf,
    [switch]$AllowAnyDatabase
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$seedFolder = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/Seeds"
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
    Write-Host "  $seedFolder"
    exit 0
}

if (-not (Test-Path $seedFolder)) {
    throw "Seed folder not found: $seedFolder"
}

Write-Host "Running top-level seed scripts (TST001 + LO codes)..."
dotnet run --project $migratorProject -- `
    --seed `
    $ConnectionString `
    $seedFolder

Write-Host "Done."
