$openapi = Get-Content "$PSScriptRoot/../src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json" -Raw | ConvertFrom-Json
foreach ($pathProp in $openapi.paths.PSObject.Properties) {
    $path = $pathProp.Name
    foreach ($methodProp in $pathProp.Value.PSObject.Properties) {
        if ($methodProp.Name -notin @('get','post','put','patch','delete')) { continue }
        $spec = $methodProp.Value
        Write-Output "$($methodProp.Name.ToUpper()) $path :: $($spec.operationId)"
    }
}
