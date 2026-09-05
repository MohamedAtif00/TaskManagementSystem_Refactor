using MediatR;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.RefreshToken;

public sealed class RefreshSessionCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshSessionCommand, RefreshSessionResult>
{
    public async Task<RefreshSessionResult> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new InvalidRefreshTokenException("No refresh token");
        }

        var existingToken = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null)
        {
            throw new InvalidRefreshTokenException("Invalid Refresh Token");
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (existingToken.Used || existingToken.IsExpired(utcNow))
        {
            throw new InvalidRefreshTokenException("Token expired");
        }

        var user = await unitOfWork.Users.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidRefreshTokenException("Invalid Refresh Token");
        }

        user.EnsureCanAuthenticate();

        existingToken.MarkUsed();
        var newRefreshToken = tokenGenerator.CreateRefreshToken(user, utcNow);
        await unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new RefreshSessionResult(
            tokenGenerator.CreateAccessToken(user),
            new RefreshSessionCookie(newRefreshToken.Token, newRefreshToken.Expires));
    }
}
