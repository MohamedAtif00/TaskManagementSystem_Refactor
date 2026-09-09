using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveSettings;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetLeaveSettingsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-settings", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLeaveSettingsQuery(), cancellationToken);

        return result.ToHttpResult(settings => Results.Ok(new LeaveSettingsResponse
        {
            FromNextBalanceMaxDays = settings.FromNextBalanceMaxDays,
            FromNextBalanceStartDate = settings.FromNextBalanceStartDate,
            FromNextBalanceEndDate = settings.FromNextBalanceEndDate,
            EmergencyBlackoutCutoffDate = settings.EmergencyBlackoutCutoffDate,
            ResetDate = settings.ResetDate,
            EmergencyAllowedToday = settings.EmergencyAllowedToday,
            FromNextWindowActiveToday = settings.FromNextWindowActiveToday
        }));
    }
}
