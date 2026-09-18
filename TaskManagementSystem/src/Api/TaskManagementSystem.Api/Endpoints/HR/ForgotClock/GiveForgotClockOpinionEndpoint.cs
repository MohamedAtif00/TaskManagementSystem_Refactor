using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class GiveForgotClockOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapPost("/{id:int}/opinions", HandleAsync).RequirePermissionCode(PermissionCodes.HrForgotClock.Update);
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        GiveForgotClockOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveForgotClockOpinionCommand(userId, role, id, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(item => Results.Ok(ForgotClockMapping.MapForgotClock(item)));
    }
}
