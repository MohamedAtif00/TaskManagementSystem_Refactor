param(
    [ValidateSet('get', 'all')]
    [string]$Mode = 'all',
    [string]$OpenApiPath = "$PSScriptRoot/../src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json",
    [string]$TransactionDataPath = "$PSScriptRoot/../src/Api/TaskManagementSystem.Api/openapi/apidog/transaction-data.json",
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
$OpenApiPath = (Resolve-Path $OpenApiPath).Path
$TransactionDataPath = (Resolve-Path $TransactionDataPath).Path

if (-not $OutputPath) {
    $fileName = if ($Mode -eq 'get') { 'get-queries-timing.json' } else { 'all-transactions-timing.json' }
    $OutputPath = Join-Path (Split-Path $OpenApiPath) "apidog/test-scenario/$fileName"
}

$openapi = Get-Content -Raw -Path $OpenApiPath | ConvertFrom-Json
$transactionData = Get-Content -Raw -Path $TransactionDataPath | ConvertFrom-Json

$timingScript = @'
var entry = {
  method: pm.request.method,
  path: pm.request.url.getPath(),
  operationId: '{OPERATION_ID}',
  status: pm.response.code,
  ms: pm.response.responseTime
};
var timings = JSON.parse(pm.environment.get('queryTimings') || '[]');
timings.push(entry);
pm.environment.set('queryTimings', JSON.stringify(timings));
console.log('[timing] ' + entry.method + ' ' + entry.path + ' -> ' + entry.status + ' (' + entry.ms + ' ms)');
'@

$saveTokenScript = @'
pm.test("Login/refresh returns 200", function () {
  pm.response.to.have.status(200);
});
const body = pm.response.json();
if (!body || !body.accessToken) {
  throw new Error("Response JSON missing accessToken field.");
}
pm.environment.set("accessToken", body.accessToken);
pm.environment.set("runSuffix", String(Date.now()));
pm.environment.set("queryTimings", "[]");
console.log("accessToken saved; runSuffix=" + pm.environment.get("runSuffix"));
'@

$setupIdScript = @'
var body = pm.response.json();
var value = null;
try {
  var match = JSON.stringify(body).match(/"id"\s*:\s*(\d+)/);
  if (match) { value = parseInt(match[1], 10); }
} catch (e) {}
if (value !== null) {
  pm.environment.set('{ENV_NAME}', String(value));
  console.log('Set {ENV_NAME} = ' + value);
}
'@

$saveCreatedIdScript = @'
var body = pm.response.json();
var value = null;
try {
  if (body && body.id) { value = body.id; }
  else {
    var match = JSON.stringify(body).match(/"id"\s*:\s*(\d+)/);
    if (match) { value = parseInt(match[1], 10); }
  }
} catch (e) {}
if (value !== null) {
  pm.environment.set('{ENV_NAME}', String(value));
  console.log('Seed created {ENV_NAME} = ' + value);
}
{TIMING_SCRIPT}
'@

$summaryScript = @'
var timings = JSON.parse(pm.environment.get("queryTimings") || "[]");
if (timings.length === 0) {
  throw new Error("No timings recorded.");
}
timings.sort(function (a, b) { return b.ms - a.ms; });
console.log("=== TMS Request Timing Summary (" + timings.length + " requests) ===");
console.table(timings.map(function (t) {
  return { ms: t.ms, status: t.status, method: t.method, path: t.path, operationId: t.operationId || "" };
}));
var totalMs = timings.reduce(function (sum, t) { return sum + t.ms; }, 0);
var successCount = timings.filter(function (t) { return t.status >= 200 && t.status < 300; }).length;
console.log("Total: " + totalMs + " ms | Avg: " + Math.round(totalMs / timings.length) + " ms | 2xx: " + successCount + "/" + timings.length);
pm.test("At least one request returned 2xx", function () { pm.expect(successCount).to.be.above(0); });
'@

function New-Step {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [string]$OperationId,
        [hashtable]$Extra = @{},
        [switch]$RequiresAuth
    )

    $needsAuth = $RequiresAuth -or ($Method -ne 'POST') -or ($Path -notmatch '/auth/login$')
    $req = @{
        path = "{{baseUrl}}$Path"
        method = $Method.ToLowerInvariant()
        auth = $null
        postProcessors = @()
    }

    if ($OperationId) {
        $req.postProcessors += @{
            enable = $true
            type = 'customScript'
            data = $timingScript.Replace('{OPERATION_ID}', ($OperationId -replace "'", "\'"))
        }
    }

    if ($needsAuth) {
        $req.auth = @{
            type = 'bearer'
            bearer = @{ token = '{{accessToken}}' }
        }
    }

    foreach ($key in $Extra.Keys) {
        $req[$key] = $Extra[$key]
    }

    return @{
        name = $Name
        type = 'customHttp'
        customHttpRequest = $req
    }
}

function Get-RequestBody {
    param([string]$OperationId, $Spec)

    if ($transactionData.bodies.$OperationId) {
        return $transactionData.bodies.$OperationId
    }

    $rb = $Spec.requestBody
    if (-not $rb) { return $null }

    $content = $rb.content.'application/json'
    if (-not $content) { return $null }

    if ($content.example) {
        return ($content.example | ConvertTo-Json -Compress -Depth 20)
    }

    return $null
}

function Convert-OpenApiPath {
    param([string]$Path)

    $result = $Path

    $named = [ordered]@{
        '\{teamId\}' = '{{teamId}}'
        '\{schemaId\}' = '{{schemaId}}'
        '\{nodeId\}' = '{{nodeId}}'
        '\{stepId\}' = '{{stepId}}'
        '\{taskBankId\}' = '{{taskBankId}}'
        '\{yearId\}' = '{{yearId}}'
        '\{projectId\}' = '{{projectId}}'
        '\{termId\}' = '{{termId}}'
        '\{subjectGroupId\}' = '{{subjectGroupId}}'
        '\{subjectId\}' = '{{subjectId}}'
        '\{unitId\}' = '{{unitId}}'
        '\{lessonId\}' = '{{lessonId}}'
        '\{learningObjectiveId\}' = '{{learningObjectiveId}}'
        '\{loId\}' = '{{learningObjectiveId}}'
        '\{sprintId\}' = '{{sprintId}}'
        '\{ticketId\}' = '{{ticketId}}'
        '\{commentId\}' = '{{commentId}}'
        '\{notificationId\}' = '{{notificationId}}'
        '\{userId\}' = '{{userId}}'
        '\{roleId\}' = '{{roleId}}'
        '\{permissionId\}' = '{{permissionId}}'
    }

    foreach ($entry in $named.GetEnumerator()) {
        $result = $result -replace $entry.Key, $entry.Value
    }

    if ($result -like '*/organization/teams/*') { $result = $result -replace '\{id\}', '{{teamId}}' }
    elseif ($result -like '*/organization/sections/*') { $result = $result -replace '\{id\}', '{{sectionId}}' }
    elseif ($result -like '*/identity/roles/*') { $result = $result -replace '\{id\}', '{{roleId}}' }
    elseif ($result -like '*/workflows/schemas/*' -and $result -notlike '*/nodes*') { $result = $result -replace '\{id\}', '{{schemaId}}' }
    elseif ($result -like '*/workflows/nodes/*') { $result = $result -replace '\{id\}', '{{nodeId}}' }
    elseif ($result -like '*/workflows/steps/*') { $result = $result -replace '\{id\}', '{{stepId}}' }
    elseif ($result -like '*/workflows/task-bank/*') { $result = $result -replace '\{id\}', '{{taskBankId}}' }
    elseif ($result -like '*/curriculum/years/*' -and $result -notlike '*/projects*') { $result = $result -replace '\{id\}', '{{yearId}}' }
    elseif ($result -like '*/curriculum/projects/*' -and $result -notlike '*/terms*') { $result = $result -replace '\{id\}', '{{projectId}}' }
    elseif ($result -like '*/curriculum/terms/*' -and $result -notlike '*/subject-groups*') { $result = $result -replace '\{id\}', '{{termId}}' }
    elseif ($result -like '*/curriculum/subject-groups/*' -and $result -notlike '*/subjects*') { $result = $result -replace '\{id\}', '{{subjectGroupId}}' }
    elseif ($result -like '*/curriculum/subjects/*') { $result = $result -replace '\{id\}', '{{subjectId}}' }
    elseif ($result -like '*/curriculum/units/*' -and $result -notlike '*/lessons*') { $result = $result -replace '\{id\}', '{{unitId}}' }
    elseif ($result -like '*/curriculum/lessons/*' -and $result -notlike '*/learning-objectives*') { $result = $result -replace '\{id\}', '{{lessonId}}' }
    elseif ($result -like '*/curriculum/learning-objectives/*') { $result = $result -replace '\{id\}', '{{learningObjectiveId}}' }
    elseif ($result -like '*/sprints/*') { $result = $result -replace '\{id\}', '{{sprintId}}' }
    elseif ($result -like '*/tickets/*') { $result = $result -replace '\{id\}', '{{ticketId}}' }
    elseif ($result -like '*/notifications/*') { $result = $result -replace '\{id\}', '{{notificationId}}' }
    elseif ($result -like '*/hr/holidays/*') { $result = $result -replace '\{id\}', '{{holidayId}}' }
    elseif ($result -like '*/hr/leave/leave-requests/*') { $result = $result -replace '\{id\}', '{{leaveRequestId}}' }
    elseif ($result -like '*/hr/permissions/*') { $result = $result -replace '\{id\}', '{{hrPermissionId}}' }
    elseif ($result -like '*/hr/work-from-home/*') { $result = $result -replace '\{id\}', '{{wfhRequestId}}' }
    elseif ($result -like '*/hr/forgot-clock/*') { $result = $result -replace '\{id\}', '{{forgotClockId}}' }
    else { $result = $result -replace '\{id\}', '{{teamId}}' }

    return $result
}

function Get-ResolvedPath {
    param(
        [string]$OperationId,
        [string]$OpenApiPath
    )

    if ($Mode -eq 'all' -and $transactionData.pathOverrides.$OperationId) {
        return $transactionData.pathOverrides.$OperationId
    }

    return Convert-OpenApiPath $OpenApiPath
}

$operations = @()
foreach ($pathProp in $openapi.paths.PSObject.Properties) {
    $path = $pathProp.Name
    foreach ($methodProp in $pathProp.Value.PSObject.Properties) {
        $method = $methodProp.Name
        if ($method -notin @('get', 'post', 'put', 'patch', 'delete')) { continue }
        if ($Mode -eq 'get' -and $method -ne 'get') { continue }

        $spec = $methodProp.Value
        $operations += [PSCustomObject]@{
            Method = $method.ToUpper()
            Path = $path
            ResolvedPath = Get-ResolvedPath -OperationId $spec.operationId -OpenApiPath $path
            OperationId = $spec.operationId
            Summary = $spec.summary
            Spec = $spec
            Sort = switch ($method) { 'get' { 0 } 'post' { 1 } 'put' { 2 } 'patch' { 3 } 'delete' { 4 } default { 9 } }
        }
    }
}

$operations = $operations | Sort-Object Sort, Path, Method
$seedOperationIds = @($transactionData.seedCreates | ForEach-Object { $_.operationId })

$steps = @()

$steps += @{
    name = '0. Login (save accessToken + runSuffix)'
    type = 'customHttp'
    customHttpRequest = @{
        path = '{{baseUrl}}/auth/login'
        method = 'post'
        auth = $null
        requestBody = @{ type = 'application/json'; data = '{"code":"{{userCode}}"}' }
        postProcessors = @(@{ enable = $true; type = 'customScript'; data = $saveTokenScript })
    }
}

$setups = @(
    @{ Name = 'teamId'; Path = '/organization/teams' }
    @{ Name = 'sectionId'; Path = '/organization/sections' }
    @{ Name = 'schemaId'; Path = '/workflows/schemas' }
    @{ Name = 'schemaTypeId'; Path = '/workflows/schema-types' }
    @{ Name = 'yearId'; Path = '/curriculum/years' }
    @{ Name = 'sprintId'; Path = '/sprints' }
    @{ Name = 'userId'; Path = '/identity/users' }
    @{ Name = 'roleId'; Path = '/identity/roles' }
    @{ Name = 'permissionId'; Path = '/identity/permissions' }
    @{ Name = 'notificationId'; Path = '/notifications' }
    @{ Name = 'leaveRequestId'; Path = '/hr/leave/leave-requests' }
    @{ Name = 'hrPermissionId'; Path = '/hr/permissions' }
    @{ Name = 'wfhRequestId'; Path = '/hr/work-from-home' }
    @{ Name = 'forgotClockId'; Path = '/hr/forgot-clock' }
    @{ Name = 'holidayId'; Path = '/hr/holidays' }
)

foreach ($setup in $setups) {
    $steps += @{
        name = "Setup: $($setup.Name)"
        type = 'customHttp'
        customHttpRequest = @{
            path = "{{baseUrl}}$($setup.Path)"
            method = 'get'
            auth = @{ type = 'bearer'; bearer = @{ token = '{{accessToken}}' } }
            postProcessors = @(@{
                enable = $true
                type = 'customScript'
                data = $setupIdScript.Replace('{ENV_NAME}', $setup.Name)
            })
        }
    }
}

$nestedSetups = @(
    @{ Name = 'projectId'; Path = '/curriculum/years/{{yearId}}/projects' }
    @{ Name = 'nodeId'; Path = '/workflows/schemas/{{schemaId}}/nodes' }
    @{ Name = 'termId'; Path = '/curriculum/projects/{{projectId}}/terms' }
    @{ Name = 'stepId'; Path = '/workflows/schemas/{{schemaId}}/steps' }
    @{ Name = 'taskBankId'; Path = '/workflows/task-bank' }
    @{ Name = 'subjectGroupId'; Path = '/curriculum/terms/{{termId}}/subject-groups' }
    @{ Name = 'subjectId'; Path = '/curriculum/subject-groups/{{subjectGroupId}}/subjects' }
    @{ Name = 'unitId'; Path = '/curriculum/subjects/{{subjectId}}/units' }
    @{ Name = 'lessonId'; Path = '/curriculum/units/{{unitId}}/lessons' }
    @{ Name = 'learningObjectiveId'; Path = '/curriculum/lessons/{{lessonId}}/learning-objectives' }
    @{ Name = 'ticketId'; Path = '/tickets' }
)

foreach ($setup in $nestedSetups) {
    $steps += @{
        name = "Setup: $($setup.Name)"
        type = 'customHttp'
        customHttpRequest = @{
            path = "{{baseUrl}}$($setup.Path)"
            method = 'get'
            auth = @{ type = 'bearer'; bearer = @{ token = '{{accessToken}}' } }
            postProcessors = @(@{
                enable = $true
                type = 'customScript'
                data = $setupIdScript.Replace('{ENV_NAME}', $setup.Name)
            })
        }
    }
}

if ($Mode -eq 'all') {
    foreach ($seed in $transactionData.seedCreates) {
        $body = Get-RequestBody -OperationId $seed.operationId -Spec $null
        $extra = @{}
        if ($body) {
            $extra.requestBody = @{ type = 'application/json'; data = $body }
        }

        $timing = $timingScript.Replace('{OPERATION_ID}', ($seed.operationId -replace "'", "\'"))
        $saveId = $saveCreatedIdScript.Replace('{ENV_NAME}', $seed.envVar).Replace('{TIMING_SCRIPT}', $timing)

        $seedRequest = @{
            path = "{{baseUrl}}$($seed.path)"
            method = 'post'
            auth = @{ type = 'bearer'; bearer = @{ token = '{{accessToken}}' } }
            postProcessors = @(@{ enable = $true; type = 'customScript'; data = $saveId })
        }
        if ($body) {
            $seedRequest.requestBody = @{ type = 'application/json'; data = $body }
        }

        $steps += @{
            name = "Seed: $($seed.operationId)"
            type = 'customHttp'
            customHttpRequest = $seedRequest
        }
    }
}

$skipOps = @(
    'createIdentityPermission', 'updateIdentityPermission', 'deleteIdentityPermission',
    'login', 'refreshToken', 'logout'
)

$i = 0
foreach ($op in $operations) {
    if ($op.OperationId -in $skipOps) { continue }
    if ($Mode -eq 'all' -and $op.OperationId -in $seedOperationIds -and $op.Method -eq 'POST') { continue }

    $i++
    $extra = @{}
    $body = Get-RequestBody -OperationId $op.OperationId -Spec $op.Spec
    if ($body) {
        $extra.requestBody = @{ type = 'application/json'; data = $body }
    }

    $stepName = "$i. $($op.Method) $($op.Path) - $($op.Summary)"
    $steps += New-Step -Name $stepName -Method $op.Method -Path $op.ResolvedPath -OperationId $op.OperationId -Extra $extra
}

$steps += @{
    name = 'ZZ. Print timing summary'
    type = 'customHttp'
    customHttpRequest = @{
        path = '{{baseUrl}}/auth/about-me'
        method = 'post'
        auth = @{ type = 'bearer'; bearer = @{ token = '{{accessToken}}' } }
        requestBody = @{ type = 'application/json'; data = '{}' }
        postProcessors = @(@{ enable = $true; type = 'customScript'; data = $summaryScript })
    }
}

$scenarioName = if ($Mode -eq 'get') { 'TMS GET Query Timing' } else { 'TMS All Transactions Timing' }
$description = "Runs all $(if ($Mode -eq 'get') { 'GET' } else { 'HTTP' }) endpoints from tms-openapi.json with request bodies from apidog/transaction-data.json. Seed creates disposable entities before mutations. Import environment.local-dev.json. Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') | Request count: $i"

$scenario = @{
    name = $scenarioName
    description = $description
    steps = $steps
}

if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $PSScriptRoot $OutputPath
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$outputDir = Split-Path $OutputPath -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$json = $scenario | ConvertTo-Json -Depth 30
[System.IO.File]::WriteAllText($OutputPath, $json, [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $OutputPath ($i operations, $($steps.Count) steps)"
