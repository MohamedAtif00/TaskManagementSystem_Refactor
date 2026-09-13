using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.RequestForgotClock;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class RequestForgotClockEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapPost("", HandleAsync).RequireAuthorization();
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        CreateForgotClockRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var punchType = ForgotClockMapping.ParsePunchType(request.PunchType);
        if (punchType is null || !TimeOnly.TryParse(request.IntendedTime, out var intendedTime))
        {
            return Results.BadRequest();
        }

        var result = await mediator.Send(
            new RequestForgotClockCommand(
                userId,
                role,
                punchType.Value,
                request.AttendanceDate,
                intendedTime,
                request.Reason),
            cancellationToken);

        return result.ToHttpResult(item =>
            Results.Created($"/hr/forgot-clock/{item.Id}", ForgotClockMapping.MapForgotClock(item)));
    }
}
