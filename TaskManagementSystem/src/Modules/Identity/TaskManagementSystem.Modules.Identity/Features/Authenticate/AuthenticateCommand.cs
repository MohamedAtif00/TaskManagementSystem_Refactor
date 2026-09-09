using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Authenticate;

public sealed record AuthenticateCommand(string Code) : ICommand<Result<AuthenticateResult>>;

public sealed record AuthenticateResult(string AccessToken, RefreshTokenCookie RefreshToken);

public sealed record RefreshTokenCookie(string Token, DateTime Expires);
