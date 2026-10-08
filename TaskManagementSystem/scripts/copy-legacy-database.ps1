# Creates the new database, applies its schema, then copies legacy dbo data into it.
# The running API does not create the database or copy rows.
#
# Usage (on the SQL Server that already has the legacy database):
#   ./scripts/copy-legacy-database.ps1 -Server YOUR_SQL
#
# Defaults:
#   source SystemAdminDB_Test_v4
#   target SystemAdminDB_Test_v5

param(
    [string]$Server = ".",
    [string]$SourceDatabase = "SystemAdminDB_Test_v4",
    [string]$TargetDatabase = "SystemAdminDB_Test_v5"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$migrateScript = Join-Path $PSScriptRoot "migrate-database.ps1"
$importScript = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/DataMigration/001_ImportLegacy.sql"

function Assert-SafeDbName([string]$Name, [string]$Label) {
    if ($Name -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
        throw "$Label '$Name' contains unsupported characters."
    }
}

Assert-SafeDbName $SourceDatabase "Source database"
Assert-SafeDbName $TargetDatabase "Target database"
if ($SourceDatabase -eq $TargetDatabase) {
    throw "Source and target database must be different."
}
if (-not (Test-Path $importScript)) {
    throw "Import script not found: $importScript"
}

Write-Host "Checking that [$SourceDatabase] exists on $Server..."
$exists = sqlcmd -S $Server -E -C -d master -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$SourceDatabase') IS NULL THEN 0 ELSE 1 END;"
if ($LASTEXITCODE -ne 0) {
    throw "Could not query SQL Server '$Server'."
}
if (($exists | Out-String) -notmatch '(?m)^\s*1\s*$') {
    throw "Source database [$SourceDatabase] was not found on $Server."
}

$connectionString = "Server=$Server;Database=$TargetDatabase;Trusted_Connection=True;TrustServerCertificate=True"
Write-Host "Creating [$TargetDatabase] and applying the new schema..."
& $migrateScript -ConnectionString $connectionString -AllowAnyDatabase
if ($LASTEXITCODE -ne 0) {
    throw "Schema migration failed with exit code $LASTEXITCODE."
}

Write-Host "Copying [$SourceDatabase] into [$TargetDatabase]..."
sqlcmd -S $Server -E -C -I -v "SourceDb=$SourceDatabase" -v "TargetDb=$TargetDatabase" -i $importScript
if ($LASTEXITCODE -ne 0) {
    throw "Legacy import failed with exit code $LASTEXITCODE."
}

Write-Host "Done. Point the API connection string at Database=$TargetDatabase."
