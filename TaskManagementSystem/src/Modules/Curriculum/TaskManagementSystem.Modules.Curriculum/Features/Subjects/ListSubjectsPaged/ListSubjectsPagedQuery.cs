using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed record ListSubjectsPagedQuery(
    string? Search,
    string? Year,
    string? Term,
    int? Page,
    int? PageSize) : IQuery<Result<PageListResult<SubjectCatalogListItemResult>>>;
