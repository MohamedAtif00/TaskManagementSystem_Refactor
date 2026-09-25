using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed record ListSubjectsExportQuery(
    string? Search,
    string? Year,
    string? Term) : IQuery<Result<IReadOnlyList<SubjectCatalogListItemResult>>>;
