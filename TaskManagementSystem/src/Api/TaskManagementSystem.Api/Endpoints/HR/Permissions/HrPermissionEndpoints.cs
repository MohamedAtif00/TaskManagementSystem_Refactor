namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class HrPermissionEndpoints
{
    public static RouteGroupBuilder MapHrPermissionEndpoints(this RouteGroupBuilder hr)
    {
        var permissions = hr.MapGroup("/permissions").WithTags("HR Permissions");

        RequestPermissionEndpoint.Map(permissions);
        GetPermissionRequestsEndpoint.Map(permissions);
        SearchPermissionRequestsEndpoint.Map(permissions);
        GetPendingPermissionRequestsEndpoint.Map(permissions);
        GetPermissionRequestByIdEndpoint.Map(permissions);
        GivePermissionOpinionEndpoint.Map(permissions);
        GiveBulkPermissionOpinionEndpoint.Map(permissions);
        CancelPermissionRequestEndpoint.Map(permissions);
        ApprovePermissionRequestEndpoint.Map(permissions);

        return hr;
    }
}
