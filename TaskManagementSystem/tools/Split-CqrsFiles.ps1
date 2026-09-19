param(
    [string[]]$Modules = @(),
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$modulesRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\src\Modules')).Path
$skipModules = @('Identity')

function Find-MatchingBrace([string]$Text, [int]$OpenIndex) {
    $depth = 0
    $inString = $false
    $escape = $false
    for ($i = $OpenIndex; $i -lt $Text.Length; $i++) {
        $ch = $Text[$i]
        if ($inString) {
            if ($escape) { $escape = $false }
            elseif ($ch -eq '\') { $escape = $true }
            elseif ($ch -eq '"') { $inString = $false }
            continue
        }
        if ($ch -eq '"') { $inString = $true; continue }
        if ($ch -eq '{') { $depth++ }
        elseif ($ch -eq '}') {
            $depth--
            if ($depth -eq 0) { return $i }
        }
    }
    throw "Unbalanced braces"
}

function Get-TypeBlocks([string]$Content, [string]$Kind) {
    $blocks = @()
    $pattern = '(?m)^public\s+sealed\s+(record|class)\s+'
    $matches = [regex]::Matches($Content, $pattern)
    foreach ($match in $matches) {
        $subtype = $match.Groups[1].Value
        if ($Kind -eq 'record' -and $subtype -ne 'record') { continue }
        if ($Kind -eq 'class' -and $subtype -ne 'class') { continue }

        $start = $match.Index
        $braceIndex = $Content.IndexOf('{', $match.Index + $match.Length)
        if ($braceIndex -lt 0) {
            $semi = $Content.IndexOf(';', $match.Index + $match.Length)
            $end = $semi + 1
        }
        else {
            $end = (Find-MatchingBrace $Content $braceIndex) + 1
        }
        $blocks += ,@($start, $end, $Content.Substring($start, $end - $start).Trim())
    }
    return $blocks
}

function Write-CqrsFile([string]$Path, [string]$Namespace, [string[]]$Usings, [string]$Body) {
    $lines = @()
    foreach ($u in $Usings) { $lines += "using $u;" }
    $lines += ''
    $lines += "namespace $Namespace;"
    $lines += ''
    $lines += $Body
    $lines += ''
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, ($lines -join "`n") + "`n", $utf8)
}

function Split-CqrsFile([string]$Path) {
    $content = Get-Content -Path $Path -Raw -Encoding UTF8
    if ($content -notmatch 'IRequestHandler<') { return $false }

    $stem = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    $dir = Split-Path $Path
    $handlerPath = Join-Path $dir "$stem`Handler.cs"
    if (Test-Path $handlerPath) { return $false }

    if ($content -match 'namespace\s+([\w.]+)\s*;') {
        $namespace = $Matches[1]
    }
    else { throw "No namespace in $Path" }

    $originalUsings = [regex]::Matches($content, '(?m)^using\s+([^;]+);') | ForEach-Object { $_.Groups[1].Value }

    $records = @(Get-TypeBlocks $content 'record')
    $classes = @(Get-TypeBlocks $content 'class')
    if ($records.Count -eq 0) { Write-Host "SKIP (no record): $Path"; return $false }

    $handlers = @()
    $validators = @()
    foreach ($block in $classes) {
        if ($block.Count -lt 3) { continue }
        $text = [string]$block[2]
        if ($text -match ': IRequestHandler<') { $handlers += ,@($block) }
        elseif ($text -match ': AbstractValidator<') { $validators += ,@($block) }
    }
    if ($handlers.Count -eq 0) { Write-Host "SKIP (no handler): $Path"; return $false }

    $dtoParts = @()
    foreach ($block in $records) {
        if ($block.Count -ge 3) { $dtoParts += [string]$block[2] }
    }
    $dtoText = $dtoParts -join "`n`n"
    $handlerText = [string]$handlers[0][2]
    $validatorText = if ($validators.Count -gt 0) { [string]$validators[0][2] } else { $null }

    $validatorPath = Join-Path $dir "$stem`Validator.cs"

    if ($validatorText -and (Test-Path $validatorPath)) { $validatorText = $null }

    $dtoUsings = @(
        'TaskManagementSystem.BuildingBlocks.Application',
        'TaskManagementSystem.BuildingBlocks.Domain'
    )
    foreach ($u in $originalUsings) {
        if ($u -like 'TaskManagementSystem.Modules.*' -and $namespace -like "*$($u.Split('.')[2])*") {
            if ($dtoUsings -notcontains $u) { $dtoUsings += $u }
        }
        $simple = $u.Split('.')[-1]
        if ($simple -and $dtoText -match "\b$([regex]::Escape($simple))\b") {
            if ($dtoUsings -notcontains $u) { $dtoUsings += $u }
        }
    }

    $handlerUsings = @($originalUsings | Where-Object { $_ -ne 'FluentValidation' })
    if ($handlerUsings -notcontains 'MediatR') { $handlerUsings = @('MediatR') + $handlerUsings }

    if ($DryRun) {
        Write-Host "SPLIT: $Path"
        return $true
    }

    Write-CqrsFile $Path $namespace $dtoUsings $dtoText
    Write-CqrsFile $handlerPath $namespace $handlerUsings $handlerText
    if ($validatorText) {
        Write-CqrsFile $validatorPath $namespace @('FluentValidation') $validatorText
    }

    $suffix = if ($validatorText) { ' + validator' } else { '' }
    Write-Host "OK: $([System.IO.Path]::GetFileName($Path)) -> handler$suffix"
    return $true
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
            if (Split-CqrsFile $_.FullName) { $script:count++ }
        }
    }
}

Write-Host "`nProcessed $count files"
