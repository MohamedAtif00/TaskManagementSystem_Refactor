using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.UpdateSprint;

public sealed record UpdateSprintCommand(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<SprintDetailResult>>;

