# Applies DbUp migrations to TaskManagementSystem SQL Server.
# Usage:
#   ./scripts/migrate-database.ps1 [-ConnectionString "..."] [-WhatIf] [-AllowAnyDatabase] [-Seed]

param(
    [string]$ConnectionString = "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True",
    [switch]$WhatIf,
    [switch]$AllowAnyDatabase,
    [switch]$Seed
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$createDbScript = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/CreateDatabase.sql"
$migrationsPath = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/Migrations"
$migratorProject = Join-Path $repoRoot "src/Database/DatabaseMigrator/DatabaseMigrator.csproj"
$seedScript = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/Seeds/002_IntegrationTestData.sql"

function Get-DatabaseName([string]$ConnectionString) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $ConnectionString
    return $builder.InitialCatalog
}

$dbName = Get-DatabaseName $ConnectionString
if (-not $AllowAnyDatabase -and $dbName -notmatch 'TaskManagementSystem|TmsTests_') {
    throw "Refusing to migrate database '$dbName'. Pass -AllowAnyDatabase to override."
}

Write-Host "Target database: $dbName"
Write-Host "Connection: $($ConnectionString -replace 'Password=[^;]+', 'Password=***')"

if ($WhatIf) {
    Write-Host "[WhatIf] Would create database (if missing) and run migrations from:"
    Write-Host "  $migrationsPath"
    if ($Seed) {
        Write-Host "[WhatIf] Would then run seed:"
        Write-Host "  $seedScript"
    }
    exit 0
}

if (-not (Test-Path $migrationsPath)) {
    throw "Migrations folder not found: $migrationsPath"
}

Write-Host "Ensuring database exists..."
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $ConnectionString
$sqlcmdArgs = @("-S", $builder.DataSource, "-C", "-i", $createDbScript)
if ($builder.IntegratedSecurity) { $sqlcmdArgs = @("-E") + $sqlcmdArgs }
elseif ($builder.UserID) { $sqlcmdArgs = @("-U", $builder.UserID, "-P", $builder.Password) + $sqlcmdArgs }
sqlcmd @sqlcmdArgs | Out-Null

Write-Host "Running migrations..."
dotnet run --project $migratorProject -- `
    $ConnectionString `
    $migrationsPath

if ($LASTEXITCODE -ne 0) {
    throw "Migration failed with exit code $LASTEXITCODE"
}

if ($Seed) {
    Write-Host "Running integration test seed..."
    dotnet run --project $migratorProject -- `
        --seed `
        $ConnectionString `
        $seedScript

    if ($LASTEXITCODE -ne 0) {
        throw "Seed failed with exit code $LASTEXITCODE"
    }
}

Write-Host "Done."
