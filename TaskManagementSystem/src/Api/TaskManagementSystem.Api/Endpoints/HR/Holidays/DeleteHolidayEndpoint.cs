using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Holidays.DeleteHoliday;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

public static class DeleteHolidayEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder holidays)
    {
        holidays.MapDelete("/{id:int}", HandleAsync)
            .RequireAuthorization(nameof(UserRole.ProjectManger));
        return holidays;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteHolidayCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
