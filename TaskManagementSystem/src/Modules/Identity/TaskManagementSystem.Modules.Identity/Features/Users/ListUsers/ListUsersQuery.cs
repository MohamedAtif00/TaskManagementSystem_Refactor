using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

public sealed record ListUsersQuery : IQuery<Result<IReadOnlyList<UserListItemResult>>>;
