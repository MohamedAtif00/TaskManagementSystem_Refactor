using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.CreateProject;

public sealed record CreateProjectCommand(int YearId, string Name, string? Description) : ICommand<Result<CurriculumProjectDetailResult>>;

