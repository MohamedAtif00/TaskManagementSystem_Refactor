using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Logout;

public sealed class LogoutCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Fail(IdentityErrors.InvalidRefreshToken("Invalid refesh token"));
        }

        var existingToken = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null)
        {
            return Result.Fail(IdentityErrors.InvalidRefreshToken("Invalid refesh token"));
        }

        if (existingToken.IsExpired(timeProvider.GetUtcNow().UtcDateTime))
        {
            return Result.Fail(IdentityErrors.InvalidRefreshToken("Token expired"));
        }

        existingToken.MarkUsed();
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}
