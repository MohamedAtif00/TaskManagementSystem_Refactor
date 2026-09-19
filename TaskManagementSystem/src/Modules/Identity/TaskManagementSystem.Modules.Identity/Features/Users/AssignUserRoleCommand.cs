using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

public sealed record AssignUserRoleCommand(int UserId, int RoleId) : ICommand<Result<NoValue>>;
