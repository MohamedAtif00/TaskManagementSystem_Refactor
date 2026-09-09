namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class HrLeaveEndpoints
{
    public static RouteGroupBuilder MapHrLeaveEndpoints(this RouteGroupBuilder hr)
    {
        var leave = hr.MapGroup("/leave").WithTags("HR Leave");

        GetBalancesEndpoint.Map(leave);
        GetLeaveSettingsEndpoint.Map(leave);
        PreviewLeaveEndpoint.Map(leave);
        RequestLeaveEndpoint.Map(leave);
        GetLeaveRequestsEndpoint.Map(leave);
        SearchLeaveRequestsEndpoint.Map(leave);
        GetPendingLeaveRequestsEndpoint.Map(leave);
        GetLeaveRequestByIdEndpoint.Map(leave);
        GetMedicalCertificateEndpoint.Map(leave);
        GiveLeaveOpinionEndpoint.Map(leave);
        GiveBulkLeaveOpinionEndpoint.Map(leave);
        CancelLeaveRequestEndpoint.Map(leave);
        ApproveLeaveRequestEndpoint.Map(leave);

        return hr;
    }
}
