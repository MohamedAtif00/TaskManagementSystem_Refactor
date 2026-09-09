using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.RefreshToken;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<Result<RefreshSessionResult>>;

public sealed record RefreshSessionResult(string AccessToken, RefreshSessionCookie RefreshToken);

public sealed record RefreshSessionCookie(string Token, DateTime Expires);
