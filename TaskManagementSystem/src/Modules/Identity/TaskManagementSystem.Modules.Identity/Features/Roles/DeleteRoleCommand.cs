using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record DeleteRoleCommand(int RoleId) : ICommand<Result<NoValue>>;
