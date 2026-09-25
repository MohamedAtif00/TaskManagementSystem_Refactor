using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Analytics;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Analytics.Features;
using TaskManagementSystem.Modules.Analytics.Features.GetSubjectOverview;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.Analytics;

public static class GetSubjectOverviewEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/subjects/{subjectId:int}/overview", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Analytics.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectOverviewQuery(subjectId), cancellationToken);
        return result.ToHttpResult(overview => Results.Ok(MapResponse(overview)));
    }

    private static OverviewResponse MapResponse(OverviewResult overview) =>
        new()
        {
            ScopeId = overview.ScopeId,
            ScopeType = overview.ScopeType,
            TotalLearningObjectives = overview.TotalLearningObjectives,
            IdleLearningObjectives = overview.IdleLearningObjectives,
            RunningLearningObjectives = overview.RunningLearningObjectives,
            DoneLearningObjectives = overview.DoneLearningObjectives,
            ProgressPercent = overview.ProgressPercent,
            BacklogTickets = overview.BacklogTickets,
            ToDoTickets = overview.ToDoTickets,
            DoingTickets = overview.DoingTickets,
            DoneTickets = overview.DoneTickets,
            CalculatedAtUtc = overview.CalculatedAtUtc
        };
}
