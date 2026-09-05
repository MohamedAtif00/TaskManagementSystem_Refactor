using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Identity.Features.RefreshToken;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<RefreshSessionResult>;

public sealed record RefreshSessionResult(string AccessToken, RefreshSessionCookie RefreshToken);

public sealed record RefreshSessionCookie(string Token, DateTime Expires);
