using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ArchiveUser;

public sealed record ArchiveUserCommand(int UserId) : ICommand<Result<NoValue>>;
