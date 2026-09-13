using TaskManagementSystem.Api.Endpoints.HR.Holidays;
using TaskManagementSystem.Api.Endpoints.HR.Leave;
using TaskManagementSystem.Api.Endpoints.HR.Permissions;
using TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;
using TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

namespace TaskManagementSystem.Api.Endpoints;

public static class HrEndpoints
{
    public static RouteGroupBuilder MapHrEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/hr").WithTags("HR");
        group.MapHrLeaveEndpoints();
        group.MapHrHolidayEndpoints();
        group.MapHrPermissionEndpoints();
        group.MapHrWorkFromHomeEndpoints();
        group.MapHrForgotClockEndpoints();
        return group;
    }
}
