using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Logout;

public sealed record LogoutCommand(string? RefreshToken) : ICommand<Result<NoValue>>;
