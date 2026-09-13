using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class GiveWorkFromHomeOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapPost("/{id:int}/opinions", HandleAsync).RequireAuthorization();
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        GiveWorkFromHomeOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveWorkFromHomeOpinionCommand(userId, role, id, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(wfh => Results.Ok(WorkFromHomeMapping.MapWorkFromHome(wfh)));
    }
}
