using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;

public sealed record CreateSprintCommand(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<SprintDetailResult>>;

