using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.GetProjectById;

public sealed record GetProjectByIdQuery(int Id) : IQuery<Result<CurriculumProjectDetailResult>>;

