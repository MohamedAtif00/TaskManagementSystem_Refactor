# Seeds optional dev heavy-load data (SEED_* rows) into TaskManagementSystem SQL Server.
# Usage:
#   ./scripts/seed-heavy-load.ps1 [-ConnectionString "..."] [-Clear] [-WhatIf] [-AllowAnyDatabase]

param(
    [string]$ConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True",
    [switch]$Clear,
    [switch]$WhatIf,
    [switch]$AllowAnyDatabase
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$seedDir = Join-Path $repoRoot "src/Database/TaskManagementSystem.Database/Scripts/Seeds/heavy-load"
$clearScript = Join-Path $seedDir "002_ClearHeavyLoad.sql"
$seedScript = Join-Path $seedDir "001_SeedHeavyLoad.sql"

function Split-SqlBatches([string]$Script) {
    $Script -split "\r\nGO\r\n|\nGO\n|\r\nGO\n|\nGO\r\n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

function Get-DatabaseName([string]$ConnectionString) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $ConnectionString
    return $builder.InitialCatalog
}

function Invoke-SqlScriptFile {
    param(
        [string]$Path,
        [string]$ConnectionString
    )

    if (-not (Test-Path $Path)) {
        throw "SQL script not found: $Path"
    }

    $script = Get-Content -Path $Path -Raw
    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $connection.Open()
    try {
        foreach ($batch in (Split-SqlBatches $script)) {
            $command = $connection.CreateCommand()
            $command.CommandText = $batch
            $command.CommandTimeout = 600
            [void]$command.ExecuteNonQuery()
        }
    }
    finally {
        $connection.Close()
    }
}

function Get-SeedCounts([string]$ConnectionString) {
    $queries = @{
        Teams = "SELECT COUNT(*) FROM [organization].[Teams] WHERE [Name] LIKE N'SEED_%'"
        Users = "SELECT COUNT(*) FROM [identity].[Users] WHERE [Code] LIKE N'SD%'"
        LearningObjectives = "SELECT COUNT(*) FROM [curriculum].[LearningObjectives] WHERE [Name] LIKE N'SEED_%'"
        Tickets = "SELECT COUNT(*) FROM [ticket].[Tasks] WHERE [Name] LIKE N'SEED_%'"
        Notifications = "SELECT COUNT(*) FROM [notifications].[Notifications] WHERE [Title] LIKE N'SEED_%'"
        Sprints = "SELECT COUNT(*) FROM [sprints].[Sprints] WHERE [Name] LIKE N'SEED_%'"
        LeaveRequests = "SELECT COUNT(*) FROM [hr].[LeaveRequests] WHERE [Reason] = N'SEED_HEAVY_LOAD'"
    }

    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $connection.Open()
    try {
        $results = [ordered]@{}
        foreach ($entry in $queries.GetEnumerator()) {
            $command = $connection.CreateCommand()
            $command.CommandText = $entry.Value
            $results[$entry.Key] = [int]$command.ExecuteScalar()
        }
        return $results
    }
    finally {
        $connection.Close()
    }
}

$dbName = Get-DatabaseName $ConnectionString
if (-not $AllowAnyDatabase -and $dbName -notmatch 'TaskManagementSystem') {
    throw "Refusing to seed database '$dbName'. Pass -AllowAnyDatabase to override."
}

Write-Host "Target database: $dbName"
Write-Host "Connection: $($ConnectionString -replace 'Password=[^;]+', 'Password=***')"

if ($WhatIf) {
    Write-Host "[WhatIf] Would run:"
    if ($Clear) { Write-Host "  - $clearScript" }
    Write-Host "  - $seedScript"
    exit 0
}

if ($Clear) {
    Write-Host "Clearing existing heavy-load seed..."
    Invoke-SqlScriptFile -Path $clearScript -ConnectionString $ConnectionString
}

Write-Host "Running heavy-load seed..."
Invoke-SqlScriptFile -Path $seedScript -ConnectionString $ConnectionString

Write-Host "`nSeed row counts:"
$counts = Get-SeedCounts -ConnectionString $ConnectionString
foreach ($key in $counts.Keys) {
    Write-Host ("  {0,-20} {1}" -f $key, $counts[$key])
}

Write-Host "`nDone. Re-run ./scripts/run-get-query-timing.ps1 to verify GET endpoints."
