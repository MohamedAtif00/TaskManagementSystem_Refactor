namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class HrForgotClockEndpoints
{
    public static RouteGroupBuilder MapHrForgotClockEndpoints(this RouteGroupBuilder hr)
    {
        var forgotClock = hr.MapGroup("/forgot-clock").WithTags("HR Forgot Clock");

        RequestForgotClockEndpoint.Map(forgotClock);
        GetForgotClockRequestsEndpoint.Map(forgotClock);
        SearchForgotClockRequestsEndpoint.Map(forgotClock);
        GetPendingForgotClockRequestsEndpoint.Map(forgotClock);
        GetForgotClockRequestByIdEndpoint.Map(forgotClock);
        GiveForgotClockOpinionEndpoint.Map(forgotClock);
        GiveBulkForgotClockOpinionEndpoint.Map(forgotClock);
        CancelForgotClockRequestEndpoint.Map(forgotClock);
        ApproveForgotClockRequestEndpoint.Map(forgotClock);

        return hr;
    }
}
