using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays.UpdateHoliday;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class UpdateHolidayEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder holidays)
    {
        holidays.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrHolidays.Manage);
        return holidays;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateHolidayRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateHolidayCommand(id, request.Name, request.Description, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(holiday => Results.Ok(HolidayMapping.MapHoliday(holiday)));
    }
}
