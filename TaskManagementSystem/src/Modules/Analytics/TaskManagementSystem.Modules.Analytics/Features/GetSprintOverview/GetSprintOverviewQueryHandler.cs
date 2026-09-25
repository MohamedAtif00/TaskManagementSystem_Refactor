using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Analytics.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Analytics.Features.GetSprintOverview;

public sealed class GetSprintOverviewQueryHandler(SprintOverviewQueries sprintOverviewQueries)
    : IRequestHandler<GetSprintOverviewQuery, Result<OverviewResult>>
{
    public Task<Result<OverviewResult>> Handle(
        GetSprintOverviewQuery request,
        CancellationToken cancellationToken) =>
        sprintOverviewQueries.GetBySprintIdAsync(request.SprintId, cancellationToken);
}
