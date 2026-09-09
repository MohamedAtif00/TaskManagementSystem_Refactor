using TaskManagementSystem.Api.Endpoints.HR.Holidays;
using TaskManagementSystem.Api.Endpoints.HR.Leave;

namespace TaskManagementSystem.Api.Endpoints;

public static class HrEndpoints
{
    public static RouteGroupBuilder MapHrEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/hr").WithTags("HR");
        group.MapHrLeaveEndpoints();
        group.MapHrHolidayEndpoints();
        return group;
    }
}
