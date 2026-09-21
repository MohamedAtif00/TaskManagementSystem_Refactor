# Generates Apidog test scenario for all GET endpoints with response-time logging.
# Usage: ./scripts/generate-apidog-get-timing-scenario.ps1

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$openApiPath = Join-Path $repoRoot "src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json"
$scriptsDir = Join-Path $repoRoot "src/Api/TaskManagementSystem.Api/openapi/apidog/scripts"
$outputPath = Join-Path $repoRoot "src/Api/TaskManagementSystem.Api/openapi/apidog/test-scenario/get-queries-timing.json"

$logTimingScript = Get-Content (Join-Path $scriptsDir "log-response-time.js") -Raw
$summaryScript = Get-Content (Join-Path $scriptsDir "print-timing-summary.js") -Raw
$saveTokenScript = Get-Content (Join-Path $scriptsDir "save-access-token.js") -Raw

$openApi = Get-Content $openApiPath -Raw | ConvertFrom-Json

function Get-ResolvedPath {
    param([string]$Template)

    $resolved = $Template

    # Context-specific {id} mapping (longest prefix wins via ordered rules)
    $idRules = @(
        @{ Pattern = '^/curriculum/years/\{id\}'; Var = 'yearId' }
        @{ Pattern = '^/curriculum/projects/\{id\}'; Var = 'projectId' }
        @{ Pattern = '^/curriculum/terms/\{id\}'; Var = 'termId' }
        @{ Pattern = '^/curriculum/subject-groups/\{id\}'; Var = 'subjectGroupId' }
        @{ Pattern = '^/curriculum/subjects/\{id\}'; Var = 'subjectId' }
        @{ Pattern = '^/curriculum/units/\{id\}'; Var = 'unitId' }
        @{ Pattern = '^/curriculum/lessons/\{id\}'; Var = 'lessonId' }
        @{ Pattern = '^/curriculum/learning-objectives/\{id\}'; Var = 'loId' }
        @{ Pattern = '^/workflows/schemas/\{id\}'; Var = 'schemaId' }
        @{ Pattern = '^/workflows/nodes/\{id\}'; Var = 'nodeId' }
        @{ Pattern = '^/workflows/steps/\{id\}'; Var = 'stepId' }
        @{ Pattern = '^/organization/teams/\{id\}'; Var = 'teamId' }
        @{ Pattern = '^/organization/sections/\{id\}'; Var = 'sectionId' }
        @{ Pattern = '^/identity/users/\{id\}'; Var = 'userId' }
        @{ Pattern = '^/identity/permissions/\{id\}'; Var = 'permissionId' }
        @{ Pattern = '^/identity/roles/\{id\}'; Var = 'roleId' }
        @{ Pattern = '^/sprints/\{id\}'; Var = 'sprintId' }
        @{ Pattern = '^/tickets/\{id\}'; Var = 'ticketId' }
        @{ Pattern = '^/notifications/\{id\}'; Var = 'notificationId' }
        @{ Pattern = '^/hr/leave/leave-requests/\{id\}'; Var = 'leaveRequestId' }
        @{ Pattern = '^/hr/permissions/\{id\}'; Var = 'hrPermissionId' }
        @{ Pattern = '^/hr/work-from-home/\{id\}'; Var = 'wfhRequestId' }
        @{ Pattern = '^/hr/forgot-clock/\{id\}'; Var = 'forgotClockId' }
        @{ Pattern = '^/hr/holidays/\{id\}'; Var = 'holidayId' }
    )

    foreach ($rule in $idRules) {
        if ($Template -match $rule.Pattern) {
            $resolved = $resolved -replace '\{id\}', "{{$($rule.Var)}}"
            break
        }
    }

    # Named path parameters
    $namedParams = @(
        'yearId', 'projectId', 'termId', 'subjectGroupId', 'subjectId', 'subjectId',
        'unitId', 'lessonId', 'loId', 'schemaId', 'nodeId', 'sprintId', 'ticketId', 'userId'
    )
    foreach ($name in $namedParams) {
        $resolved = $resolved -replace "\{$name\}", "{{$name}}"
    }

    # sprints/{id} already handled; sprints/{sprintId}/tickets
    $resolved = $resolved -replace '\{sprintId\}', '{{sprintId}}'

    return $resolved
}

function New-CustomScriptProcessor {
    param([string]$Script)
    return @{
        type   = 'customScript'
        enable = $true
        data   = $Script
    }
}

function New-ExtractIdProcessor {
    param(
        [string]$JsonPath,
        [string]$VariableName
    )

    $script = @"
var body = pm.response.json();
var value = null;
try {
  var match = JSON.stringify(body).match(/"id"\s*:\s*(\d+)/);
  if (match) { value = parseInt(match[1], 10); }
} catch (e) {}
if (value !== null) {
  pm.environment.set('$VariableName', String(value));
  console.log('Set $VariableName = ' + value);
}
"@

    return New-CustomScriptProcessor -Script $script
}

function New-HttpStep {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [array]$PostProcessors = @(),
        [hashtable]$QueryParams = $null
    )

    $url = "{{baseUrl}}$Path"
    if ($QueryParams -and $QueryParams.Count -gt 0) {
        $qs = ($QueryParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join '&'
        $url = "$url?$qs"
    }

    return @{
        type = 'customHttp'
        name = $Name
        customHttpRequest = @{
            path            = $url
            method          = $Method.ToLowerInvariant()
            auth            = @{
                type   = 'bearer'
                bearer = @{ token = '{{accessToken}}' }
            }
            postProcessors  = @($PostProcessors)
        }
    }
}

$steps = @()

# Bootstrap: login + reset timings
$steps += New-HttpStep -Name '0. Login (save accessToken)' -Method 'POST' -Path '/auth/login' -PostProcessors @(
    (New-CustomScriptProcessor -Script $saveTokenScript),
    (New-CustomScriptProcessor -Script 'pm.environment.set("queryTimings", "[]");')
) 
$steps[-1].customHttpRequest.requestBody = @{
    type = 'application/json'
    data = '{"code":"{{userCode}}"}'
}
$steps[-1].customHttpRequest.auth = $null

# Setup: extract IDs from list endpoints (best-effort)
$setupGets = @(
    @{ Name = 'Setup: teamId'; Path = '/organization/teams'; Var = 'teamId' }
    @{ Name = 'Setup: sectionId'; Path = '/organization/sections'; Var = 'sectionId' }
    @{ Name = 'Setup: schemaId'; Path = '/workflows/schemas'; Var = 'schemaId' }
    @{ Name = 'Setup: yearId'; Path = '/curriculum/years'; Var = 'yearId' }
    @{ Name = 'Setup: sprintId'; Path = '/sprints'; Var = 'sprintId' }
    @{ Name = 'Setup: userId'; Path = '/identity/users'; Var = 'userId' }
    @{ Name = 'Setup: notificationId'; Path = '/notifications'; Var = 'notificationId' }
    @{ Name = 'Setup: leaveRequestId'; Path = '/hr/leave/leave-requests'; Var = 'leaveRequestId' }
    @{ Name = 'Setup: hrPermissionId'; Path = '/hr/permissions'; Var = 'hrPermissionId' }
    @{ Name = 'Setup: wfhRequestId'; Path = '/hr/work-from-home'; Var = 'wfhRequestId' }
    @{ Name = 'Setup: forgotClockId'; Path = '/hr/forgot-clock'; Var = 'forgotClockId' }
)

foreach ($setup in $setupGets) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

# Chained setup after yearId
$chainedSetup = @(
    @{ Name = 'Setup: projectId'; Path = '/curriculum/years/{{yearId}}/projects'; Var = 'projectId'; Requires = 'yearId' }
    @{ Name = 'Setup: nodeId'; Path = '/workflows/schemas/{{schemaId}}/nodes'; Var = 'nodeId'; Requires = 'schemaId' }
)

foreach ($setup in $chainedSetup) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup2 = @(
    @{ Name = 'Setup: termId'; Path = '/curriculum/projects/{{projectId}}/terms'; Var = 'termId' }
    @{ Name = 'Setup: stepId'; Path = '/workflows/nodes/{{nodeId}}/steps'; Var = 'stepId' }
)

foreach ($setup in $chainedSetup2) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup3 = @(
    @{ Name = 'Setup: subjectGroupId'; Path = '/curriculum/terms/{{termId}}/subject-groups'; Var = 'subjectGroupId' }
    @{ Name = 'Setup: ticketId'; Path = '/learning-objectives/{{loId}}/tickets'; Var = 'ticketId' }
)

foreach ($setup in $chainedSetup3) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup4 = @(
    @{ Name = 'Setup: subjectId'; Path = '/curriculum/subject-groups/{{subjectGroupId}}/subjects'; Var = 'subjectId' }
)

foreach ($setup in $chainedSetup4) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup5 = @(
    @{ Name = 'Setup: unitId'; Path = '/curriculum/subjects/{{subjectId}}/units'; Var = 'unitId' }
)

foreach ($setup in $chainedSetup5) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup6 = @(
    @{ Name = 'Setup: lessonId'; Path = '/curriculum/units/{{unitId}}/lessons'; Var = 'lessonId' }
)

foreach ($setup in $chainedSetup6) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

$chainedSetup7 = @(
    @{ Name = 'Setup: loId'; Path = '/curriculum/lessons/{{lessonId}}/learning-objectives'; Var = 'loId' }
)

foreach ($setup in $chainedSetup7) {
    $steps += New-HttpStep -Name $setup.Name -Method 'GET' -Path $setup.Path -PostProcessors @(
        (New-ExtractIdProcessor -JsonPath '$[0].id' -VariableName $setup.Var)
    )
}

# Collect GET operations from OpenAPI (skip /openapi meta routes)
$getOps = @()
foreach ($pathProp in $openApi.paths.PSObject.Properties) {
    $template = $pathProp.Name
    if ($template -like '/openapi/*') { continue }

    $pathItem = $pathProp.Value
    if (-not $pathItem.get) { continue }

    $op = $pathItem.get
    $getOps += [PSCustomObject]@{
        Template     = $template
        OperationId  = $op.operationId
        Summary      = $op.summary
        ResolvedPath = Get-ResolvedPath -Template $template
    }
}

$getOps = $getOps | Sort-Object Template

$stepIndex = 1
foreach ($op in $getOps) {
    $queryParams = $null

    if ($op.Template -eq '/sprints') {
        $queryParams = @{ archived = 'false' }
    }
    elseif ($op.Template -eq '/notifications') {
        $queryParams = @{ page = '1'; pageSize = '20' }
    }

    $name = ('{0:D2}. GET {1}' -f $stepIndex, $op.Template)
    if ($op.Summary) { $name = "$name - $($op.Summary)" }

    $timingScript = $logTimingScript
    if ($op.OperationId) {
        $opIdLiteral = $op.OperationId.Replace("'", "\'")
        $timingScript = @"
var entry = {
  method: pm.request.method,
  path: pm.request.url.getPath(),
  operationId: '$opIdLiteral',
  status: pm.response.code,
  ms: pm.response.responseTime
};
var timings = JSON.parse(pm.environment.get('queryTimings') || '[]');
timings.push(entry);
pm.environment.set('queryTimings', JSON.stringify(timings));
console.log('[timing] ' + entry.method + ' ' + entry.path + ' -> ' + entry.status + ' (' + entry.ms + ' ms)');
"@
    }

    $steps += New-HttpStep -Name $name -Method 'GET' -Path $op.ResolvedPath -PostProcessors @(
        (New-CustomScriptProcessor -Script $timingScript)
    ) -QueryParams $queryParams

    $stepIndex++
}

# Final summary step
$steps += New-HttpStep -Name 'ZZ. Print timing summary' -Method 'GET' -Path '/auth/about-me' -PostProcessors @(
    (New-CustomScriptProcessor -Script $summaryScript)
)
$steps[-1].customHttpRequest.method = 'post'
$steps[-1].customHttpRequest.requestBody = @{
    type = 'application/json'
    data = '{}'
}

$scenario = @{
    name        = 'TMS GET Query Timing'
    description = @(
        'Runs all GET endpoints from tms-openapi.json and records HTTP response time per request.'
        'Import environment.local-dev.json, run API + migrations, then execute this scenario.'
        'See openapi/apidog/scripts/log-response-time.js and print-timing-summary.js.'
        "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') | GET count: $($getOps.Count)"
    ) -join ' '
    steps       = $steps
}

$json = $scenario | ConvertTo-Json -Depth 20
[System.IO.File]::WriteAllText($outputPath, $json, [System.Text.UTF8Encoding]::new($false))

Write-Host "Generated $($getOps.Count) GET timing steps -> $outputPath"
