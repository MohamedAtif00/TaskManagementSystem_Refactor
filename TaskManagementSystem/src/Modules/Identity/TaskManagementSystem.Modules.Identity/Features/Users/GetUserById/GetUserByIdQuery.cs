using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(int UserId) : IQuery<Result<UserDetailResult>>;
