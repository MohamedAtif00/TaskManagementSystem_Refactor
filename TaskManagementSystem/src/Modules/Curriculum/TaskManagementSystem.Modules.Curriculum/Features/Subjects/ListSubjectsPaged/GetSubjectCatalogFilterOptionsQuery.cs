using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed record GetSubjectCatalogFilterOptionsQuery
    : IQuery<Result<SubjectCatalogFilterOptionsResult>>;
