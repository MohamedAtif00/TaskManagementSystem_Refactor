using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.CancelWorkFromHomeRequest;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class CancelWorkFromHomeRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapPut("/{id:int}/cancel", HandleAsync).RequirePermissionCode(PermissionCodes.HrWorkFromHome.Update);
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new CancelWorkFromHomeRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(wfh => Results.Ok(WorkFromHomeMapping.MapWorkFromHome(wfh)));
    }
}
