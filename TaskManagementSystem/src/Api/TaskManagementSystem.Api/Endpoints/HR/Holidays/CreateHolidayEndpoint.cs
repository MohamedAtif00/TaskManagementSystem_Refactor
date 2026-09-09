using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class CreateHolidayEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder holidays)
    {
        holidays.MapPost("", HandleAsync)
            .RequireAuthorization(nameof(UserRole.ProjectManger));
        return holidays;
    }

    private static async Task<IResult> HandleAsync(
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
            Results.Created($"/hr/holidays/{holiday.Id}", HolidayMapping.MapHoliday(holiday)));
    }
}
