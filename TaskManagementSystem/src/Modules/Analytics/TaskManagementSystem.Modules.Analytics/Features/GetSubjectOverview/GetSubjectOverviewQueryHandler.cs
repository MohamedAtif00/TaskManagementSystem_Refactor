using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Analytics.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Analytics.Features.GetSubjectOverview;

public sealed class GetSubjectOverviewQueryHandler(SubjectOverviewQueries subjectOverviewQueries)
    : IRequestHandler<GetSubjectOverviewQuery, Result<OverviewResult>>
{
    public Task<Result<OverviewResult>> Handle(
        GetSubjectOverviewQuery request,
        CancellationToken cancellationToken) =>
        subjectOverviewQueries.GetBySubjectIdAsync(request.SubjectId, cancellationToken);
}
