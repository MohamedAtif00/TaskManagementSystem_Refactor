using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.ApproveWorkFromHomeRequest;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class ApproveWorkFromHomeRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapPost("/{id:int}/approve", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrWorkFromHome.Manage);
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new ApproveWorkFromHomeRequestCommand(userId, role, id), cancellationToken);

        return result.ToHttpResult(wfh => Results.Ok(WorkFromHomeMapping.MapWorkFromHome(wfh)));
    }
}
