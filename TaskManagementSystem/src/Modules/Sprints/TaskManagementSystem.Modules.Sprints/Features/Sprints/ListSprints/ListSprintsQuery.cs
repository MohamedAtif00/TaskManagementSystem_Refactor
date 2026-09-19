using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

public sealed record ListSprintsQuery(bool? Archived = null) : IQuery<Result<IReadOnlyList<SprintListItemResult>>>;

