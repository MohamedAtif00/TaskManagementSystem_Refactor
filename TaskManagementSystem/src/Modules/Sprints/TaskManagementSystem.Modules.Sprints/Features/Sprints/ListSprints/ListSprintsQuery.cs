using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

public sealed record ListSprintsQuery(bool? Archived = null, int? Page = null, int? PageSize = null)
    : IQuery<Result<IReadOnlyList<SprintListItemResult>>>;

public sealed record ListSprintsPagedQuery(bool? Archived, int? Page, int? PageSize)
    : IQuery<Result<PageListResult<SprintListItemResult>>>;

