using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Logout;

public sealed record LogoutCommand(string? RefreshToken) : ICommand;
