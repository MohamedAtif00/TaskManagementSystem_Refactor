using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class ListHolidaysEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder holidays)
    {
        holidays.MapGet("", HandleAsync).RequireAuthorization();
        return holidays;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListHolidaysQuery(fromDate, toDate), cancellationToken);

        return result.ToHttpResult(holidaysList =>
            Results.Ok(holidaysList.Select(HolidayMapping.MapHoliday).ToList()));
    }
}
