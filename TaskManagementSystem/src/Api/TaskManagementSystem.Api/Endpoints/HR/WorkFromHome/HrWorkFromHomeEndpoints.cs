namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class HrWorkFromHomeEndpoints
{
    public static RouteGroupBuilder MapHrWorkFromHomeEndpoints(this RouteGroupBuilder hr)
    {
        var workFromHome = hr.MapGroup("/work-from-home").WithTags("HR Work From Home");

        RequestWorkFromHomeEndpoint.Map(workFromHome);
        GetWorkFromHomeRequestsEndpoint.Map(workFromHome);
        SearchWorkFromHomeRequestsEndpoint.Map(workFromHome);
        GetPendingWorkFromHomeRequestsEndpoint.Map(workFromHome);
        GetWorkFromHomeRequestByIdEndpoint.Map(workFromHome);
        GiveWorkFromHomeOpinionEndpoint.Map(workFromHome);
        GiveBulkWorkFromHomeOpinionEndpoint.Map(workFromHome);
        CancelWorkFromHomeRequestEndpoint.Map(workFromHome);
        ApproveWorkFromHomeRequestEndpoint.Map(workFromHome);

        return hr;
    }
}
