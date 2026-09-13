using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveBulkWorkFromHomeOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class GiveBulkWorkFromHomeOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapPost("/opinions/bulk", HandleAsync).RequireAuthorization();
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        GiveBulkWorkFromHomeOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveBulkWorkFromHomeOpinionCommand(userId, role, request.WorkFromHomeRequestIds, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(bulk => Results.Ok(new BulkWorkFromHomeOpinionResponse
        {
            Succeeded = bulk.Succeeded,
            Failed = bulk.Failed,
            FailedWorkFromHomeRequestIds = bulk.FailedWorkFromHomeRequestIds.ToList()
        }));
    }
}
