using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.GetSprintById;

public sealed record GetSprintByIdQuery(int Id) : IQuery<Result<SprintDetailResult>>;

