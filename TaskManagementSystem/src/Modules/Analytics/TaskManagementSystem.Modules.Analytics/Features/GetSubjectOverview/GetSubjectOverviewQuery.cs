using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Analytics.Features;

namespace TaskManagementSystem.Modules.Analytics.Features.GetSubjectOverview;

public sealed record GetSubjectOverviewQuery(int SubjectId) : IQuery<Result<OverviewResult>>;
