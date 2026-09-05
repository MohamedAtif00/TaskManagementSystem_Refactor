using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Authenticate;

public sealed record AuthenticateCommand(string Code) : ICommand<AuthenticateResult>;

public sealed record AuthenticateResult(string AccessToken, RefreshTokenCookie RefreshToken);

public sealed record RefreshTokenCookie(string Token, DateTime Expires);
