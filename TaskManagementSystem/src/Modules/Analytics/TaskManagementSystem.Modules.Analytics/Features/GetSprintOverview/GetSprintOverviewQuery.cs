using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Analytics.Features;

namespace TaskManagementSystem.Modules.Analytics.Features.GetSprintOverview;

public sealed record GetSprintOverviewQuery(int SprintId) : IQuery<Result<OverviewResult>>;
