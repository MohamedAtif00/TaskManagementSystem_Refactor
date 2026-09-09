using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;
using TaskManagementSystem.Modules.HR.Features.Holidays.DeleteHoliday;
using TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;
using TaskManagementSystem.Modules.HR.Features.Holidays.UpdateHoliday;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class HrHolidayEndpoints
{
    public static RouteGroupBuilder MapHrHolidayEndpoints(this RouteGroupBuilder hr)
    {
        var holidays = hr.MapGroup("/holidays").WithTags("HR Holidays");

        holidays.MapGet("", ListHolidaysAsync).RequireAuthorization();
        holidays.MapPost("", CreateHolidayAsync)
            .RequireAuthorization(nameof(UserRole.ProjectManger));
        holidays.MapPut("/{id:int}", UpdateHolidayAsync)
            .RequireAuthorization(nameof(UserRole.ProjectManger));
        holidays.MapDelete("/{id:int}", DeleteHolidayAsync)
            .RequireAuthorization(nameof(UserRole.ProjectManger));

        return hr;
    }

    private static async Task<IResult> ListHolidaysAsync(
        IMediator mediator,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListHolidaysQuery(fromDate, toDate), cancellationToken);

        return result.ToHttpResult(holidaysList =>
            Results.Ok(holidaysList.Select(MapHoliday).ToList()));
    }

    private static async Task<IResult> CreateHolidayAsync(
        CreateHolidayRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(
            new CreateHolidayCommand(userId, request.Name, request.Description, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(holiday =>
            Results.Created($"/hr/holidays/{holiday.Id}", MapHoliday(holiday)));
    }

    private static async Task<IResult> UpdateHolidayAsync(
        int id,
        UpdateHolidayRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateHolidayCommand(id, request.Name, request.Description, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(holiday => Results.Ok(MapHoliday(holiday)));
    }

    private static async Task<IResult> DeleteHolidayAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteHolidayCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static HolidayResponse MapHoliday(HolidayResult holiday) =>
        new()
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Description = holiday.Description,
            StartDate = holiday.StartDate,
            EndDate = holiday.EndDate,
            CreatedAt = holiday.CreatedAt,
            CreatedByUserId = holiday.CreatedByUserId
        };
}
