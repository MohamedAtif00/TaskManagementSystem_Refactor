param(
    [string[]]$Modules = @()
)

$ErrorActionPreference = 'Stop'
$modulesRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\src\Modules')).Path
$skipModules = @('Identity')

function Write-DtoFile([string]$Path, [string]$Namespace, [string[]]$Usings, [string[]]$Records) {
    $lines = @()
    foreach ($u in ($Usings | Select-Object -Unique)) { $lines += "using $u;" }
    $lines += ''
    $lines += "namespace $Namespace;"
    $lines += ''
    $lines += ($Records -join "`n`n")
    $lines += ''
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, ($lines -join "`n") + "`n", $utf8)
}

function Get-MinimalUsings([string[]]$OriginalUsings, [string]$DtoText, [string]$Namespace) {
    $usings = @(
        'TaskManagementSystem.BuildingBlocks.Application',
        'TaskManagementSystem.BuildingBlocks.Domain'
    )
    foreach ($u in $OriginalUsings) {
        if ($u -like 'TaskManagementSystem.Modules.*') {
            if ($namespace -like "*$($u.Split('.')[2])*") {
                if ($usings -notcontains $u) { $usings += $u }
            }
        }
        $simple = $u.Split('.')[-1]
        if ($simple -and $DtoText -match "\b$([regex]::Escape($simple))\b") {
            if ($usings -notcontains $u) { $usings += $u }
        }
    }
    return $usings
}

$count = 0
Get-ChildItem $modulesRoot -Directory | Sort-Object Name | ForEach-Object {
    $moduleName = $_.Name -replace '^TaskManagementSystem\.Modules\.', ''
    if ($skipModules -contains $moduleName) { return }
    if ($Modules.Count -gt 0 -and ($Modules -notcontains $moduleName)) { return }

    $features = Join-Path $_.FullName "TaskManagementSystem.Modules.$moduleName\Features"
    if (-not (Test-Path $features)) { return }

    foreach ($pattern in @('*Command.cs', '*Query.cs')) {
        Get-ChildItem $features -Recurse -Filter $pattern | Sort-Object FullName | ForEach-Object {
            if ($_.Name -match 'Handler\.cs$|Validator\.cs$') { return }

            $content = Get-Content $_.FullName -Raw -Encoding UTF8
            if ($content -notmatch 'public\s+sealed\s+class') { return }

            if ($content -notmatch 'namespace\s+([\w.]+)\s*;') { return }
            $namespace = $Matches[1]
            $originalUsings = [regex]::Matches($content, '(?m)^using\s+([^;]+);') | ForEach-Object { $_.Groups[1].Value }

            $records = [regex]::Matches($content, '(?ms)^public\s+sealed\s+record\s+.+?;') |
                ForEach-Object { $_.Value.Trim() }
            if ($records.Count -eq 0) {
                Write-Host "WARN no records: $($_.FullName)"
                return
            }

            $dtoText = $records -join "`n`n"
            $dtoUsings = Get-MinimalUsings $originalUsings $dtoText $namespace
            Write-DtoFile $_.FullName $namespace $dtoUsings $records
            Write-Host "TRIM: $($_.Name)"
            $script:count++
        }
    }
}

Write-Host "`nTrimmed $count files"
