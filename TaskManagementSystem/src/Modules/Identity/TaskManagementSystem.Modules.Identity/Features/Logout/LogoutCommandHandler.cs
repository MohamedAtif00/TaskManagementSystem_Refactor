using MediatR;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Logout;

public sealed class LogoutCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new InvalidRefreshTokenException("Invalid refesh token");
        }

        var token = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (token is null)
        {
            throw new InvalidRefreshTokenException("Invalid refesh token");
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (token.Used && token.IsExpired(utcNow))
        {
            throw new InvalidRefreshTokenException("Token expired");
        }

        token.MarkUsed();
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
