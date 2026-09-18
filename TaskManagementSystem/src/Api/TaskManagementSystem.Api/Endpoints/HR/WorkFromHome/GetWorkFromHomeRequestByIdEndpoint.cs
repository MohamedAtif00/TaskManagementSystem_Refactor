using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequestById;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class GetWorkFromHomeRequestByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.HrWorkFromHome.Read);
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
        var result = await mediator.Send(new GetWorkFromHomeRequestByIdQuery(userId, role, id), cancellationToken);

        return result.ToHttpResult(wfh => Results.Ok(WorkFromHomeMapping.MapWorkFromHome(wfh)));
    }
}
