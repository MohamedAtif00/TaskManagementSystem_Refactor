# Runs all GET endpoints from tms-openapi.json against a live API and reports timing.
# Usage: ./scripts/run-get-query-timing.ps1 [-BaseUrl http://localhost:61173] [-UserCode TST001]

param(
    [string]$BaseUrl = "http://localhost:61173",
    [string]$UserCode = "TST001"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$openApiPath = Join-Path $repoRoot "src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json"
$reportPath = Join-Path $repoRoot "query-timing-report.csv"

$PathParamVariables = @{
    '/curriculum/years/{id}' = 'yearId'
    '/curriculum/projects/{id}' = 'projectId'
    '/curriculum/terms/{id}' = 'termId'
    '/curriculum/subject-groups/{id}' = 'subjectGroupId'
    '/curriculum/subjects/{id}' = 'subjectId'
    '/curriculum/units/{id}' = 'unitId'
    '/curriculum/lessons/{id}' = 'lessonId'
    '/curriculum/learning-objectives/{id}' = 'loId'
    '/workflows/schemas/{id}' = 'schemaId'
    '/workflows/nodes/{id}' = 'nodeId'
    '/workflows/steps/{id}' = 'stepId'
    '/organization/teams/{id}' = 'teamId'
    '/organization/sections/{id}' = 'sectionId'
    '/identity/users/{id}' = 'userId'
    '/identity/permissions/{id}' = 'permissionId'
    '/identity/roles/{id}' = 'roleId'
    '/sprints/{id}' = 'sprintId'
    '/tickets/{id}' = 'ticketId'
    '/notifications/{id}' = 'notificationId'
    '/hr/leave/leave-requests/{id}' = 'leaveRequestId'
    '/hr/leave/balances/{userId}' = 'userId'
    '/hr/permissions/{id}' = 'hrPermissionId'
    '/hr/work-from-home/{id}' = 'wfhRequestId'
    '/hr/forgot-clock/{id}' = 'forgotClockId'
    '/hr/holidays/{id}' = 'holidayId'
}

function Get-FirstIdFromJson([object]$Element) {
    if ($null -eq $Element) { return $null }
    if ($Element -is [System.Array]) {
        foreach ($item in $Element) {
            $id = Get-FirstIdFromJson $item
            if ($id) { return $id }
        }
        return $null
    }
    if ($Element.PSObject.Properties.Name -contains 'id') {
        return [int]$Element.id
    }
    if ($Element.PSObject.Properties.Name -contains 'items') {
        return Get-FirstIdFromJson $Element.items
    }
    return $null
}

function Invoke-TmsGet {
    param(
        [hashtable]$Headers,
        [string]$Path
    )
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl$Path" -Headers $Headers -UseBasicParsing -TimeoutSec 60
        $sw.Stop()
        return [PSCustomObject]@{ Status = [int]$response.StatusCode; Ms = $sw.ElapsedMilliseconds; Body = $response.Content }
    }
    catch {
        $sw.Stop()
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        return [PSCustomObject]@{ Status = $status; Ms = $sw.ElapsedMilliseconds; Body = $null }
    }
}

function Resolve-TmsPath {
    param([string]$Template, [hashtable]$Variables)
    $resolved = $Template
    if ($PathParamVariables.ContainsKey($Template) -and $Variables.ContainsKey($PathParamVariables[$Template])) {
        $resolved = $resolved -replace '\{id\}', $Variables[$PathParamVariables[$Template]]
    }
    foreach ($key in $Variables.Keys) {
        $resolved = $resolved -replace "\{$key\}", $Variables[$key]
    }
    return $resolved
}

Write-Host "Login at $BaseUrl with code $UserCode..."
$loginBody = @{ code = $UserCode } | ConvertTo-Json
$login = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json" -TimeoutSec 30
$token = $login.accessToken
if (-not $token) { throw "Login failed: no accessToken returned" }

$headers = @{ Authorization = "Bearer $token" }
$variables = @{}

function Try-ExtractId {
    param([string]$Path, [string]$VarName)
    $result = Invoke-TmsGet -Headers $headers -Path $Path
    if ($result.Status -ge 200 -and $result.Status -lt 300 -and $result.Body) {
        $json = $result.Body | ConvertFrom-Json
        $id = Get-FirstIdFromJson $json
        if ($id) { $variables[$VarName] = "$id"; Write-Host "  $VarName = $id" }
    }
}

Write-Host "Bootstrap IDs..."
Try-ExtractId '/organization/teams' 'teamId'
Try-ExtractId '/organization/sections' 'sectionId'
Try-ExtractId '/workflows/schemas' 'schemaId'
Try-ExtractId '/curriculum/years' 'yearId'
Try-ExtractId '/sprints?archived=false' 'sprintId'
Try-ExtractId '/identity/users' 'userId'
Try-ExtractId '/notifications?page=1&pageSize=20' 'notificationId'
Try-ExtractId '/hr/leave/leave-requests' 'leaveRequestId'
Try-ExtractId '/hr/permissions' 'hrPermissionId'
Try-ExtractId '/hr/work-from-home' 'wfhRequestId'
Try-ExtractId '/hr/forgot-clock' 'forgotClockId'
if ($variables.yearId) { Try-ExtractId "/curriculum/years/$($variables.yearId)/projects" 'projectId' }
if ($variables.schemaId) { Try-ExtractId "/workflows/schemas/$($variables.schemaId)/nodes" 'nodeId' }
if ($variables.projectId) { Try-ExtractId "/curriculum/projects/$($variables.projectId)/terms" 'termId' }
if ($variables.nodeId) { Try-ExtractId "/workflows/nodes/$($variables.nodeId)/steps" 'stepId' }
if ($variables.termId) { Try-ExtractId "/curriculum/terms/$($variables.termId)/subject-groups" 'subjectGroupId' }
if ($variables.subjectGroupId) { Try-ExtractId "/curriculum/subject-groups/$($variables.subjectGroupId)/subjects" 'subjectId' }
if ($variables.subjectId) { Try-ExtractId "/curriculum/subjects/$($variables.subjectId)/units" 'unitId' }
if ($variables.unitId) { Try-ExtractId "/curriculum/units/$($variables.unitId)/lessons" 'lessonId' }
if ($variables.lessonId) { Try-ExtractId "/curriculum/lessons/$($variables.lessonId)/learning-objectives" 'loId' }
if ($variables.loId) { Try-ExtractId "/learning-objectives/$($variables.loId)/tickets" 'ticketId' }

$openApi = Get-Content $openApiPath -Raw | ConvertFrom-Json
$results = @()

foreach ($pathProp in $openApi.paths.PSObject.Properties) {
    $template = $pathProp.Name
    if ($template -like '/openapi/*') { continue }
    if (-not $pathProp.Value.get) { continue }

    $resolved = Resolve-TmsPath -Template $template -Variables $variables
    $query = switch ($template) {
        '/sprints' { '?archived=false' }
        '/notifications' { '?page=1&pageSize=20' }
        default { '' }
    }

    $url = "$resolved$query"
    $opId = $pathProp.Value.get.operationId
    $result = Invoke-TmsGet -Headers $headers -Path $url
    $row = [PSCustomObject]@{
        OperationId = $opId
        Template    = $template
        Url         = $url
        Status      = $result.Status
        Ms          = $result.Ms
    }
    $results += $row
    Write-Host ("{0,4} ms  {1,3}  GET {2}" -f $result.Ms, $result.Status, $url)
}

$results = $results | Sort-Object Ms -Descending
$results | Export-Csv -Path $reportPath -NoTypeInformation -Encoding UTF8

$success = ($results | Where-Object { $_.Status -ge 200 -and $_.Status -lt 300 }).Count
$totalMs = ($results | Measure-Object -Property Ms -Sum).Sum
$avgMs = [math]::Round($totalMs / $results.Count)

Write-Host ""
Write-Host "=== TMS GET Query Timing Summary ($($results.Count) requests) ==="
Write-Host "2xx: $success/$($results.Count) | Total: ${totalMs}ms | Avg: ${avgMs}ms"
Write-Host "CSV: $reportPath"
Write-Host ""
$results | Select-Object Ms, Status, Template, OperationId | Format-Table -AutoSize
