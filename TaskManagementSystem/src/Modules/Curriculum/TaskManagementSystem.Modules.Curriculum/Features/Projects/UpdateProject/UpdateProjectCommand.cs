using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.UpdateProject;

public sealed record UpdateProjectCommand(int Id, string Name, string? Description) : ICommand<Result<CurriculumProjectDetailResult>>;

