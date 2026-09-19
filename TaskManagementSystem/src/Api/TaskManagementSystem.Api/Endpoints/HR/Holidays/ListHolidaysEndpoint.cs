using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class ListHolidaysEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder holidays)
    {
        holidays.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.HrHolidays.Read);
        return holidays;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        string? fromDate,
        string? toDate,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListHolidaysQuery(
                OptionalQueryBinding.ParseOptionalDate(fromDate),
                OptionalQueryBinding.ParseOptionalDate(toDate)),
            cancellationToken);

        return result.ToHttpResult(holidaysList =>
            Results.Ok(holidaysList.Select(HolidayMapping.MapHoliday).ToList()));
    }
}
