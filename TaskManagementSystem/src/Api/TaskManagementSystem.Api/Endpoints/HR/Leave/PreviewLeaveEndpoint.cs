using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.PreviewLeave;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class PreviewLeaveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPost("/leave-requests/preview", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        PreviewLeaveRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(
            new PreviewLeaveQuery(userId, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(preview => Results.Ok(new PreviewLeaveResponse
        {
            RequestedDays = preview.RequestedDays,
            AvailableAnnual = preview.AvailableAnnual,
            NeededFromNext = preview.NeededFromNext,
            FromNextBalanceMaxDays = preview.FromNextBalanceMaxDays,
            AlreadyUsedFromNext = preview.AlreadyUsedFromNext,
            PendingFromNext = preview.PendingFromNext,
            RequiresConfirmation = preview.RequiresConfirmation,
            ErrorMessage = preview.ErrorMessage
        }));
    }
}
