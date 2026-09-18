using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.RequestWorkFromHome;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class RequestWorkFromHomeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.HrWorkFromHome.Create);
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        CreateWorkFromHomeRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();

        var result = await mediator.Send(
            new RequestWorkFromHomeCommand(userId, role, request.Date, request.NoteForManager),
            cancellationToken);

        return result.ToHttpResult(wfh =>
            Results.Created($"/hr/work-from-home/{wfh.Id}", WorkFromHomeMapping.MapWorkFromHome(wfh)));
    }
}
