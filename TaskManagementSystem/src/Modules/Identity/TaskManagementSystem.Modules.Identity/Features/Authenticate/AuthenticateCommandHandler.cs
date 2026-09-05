using MediatR;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Authenticate;

public sealed class AuthenticateCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    TimeProvider timeProvider)
    : IRequestHandler<AuthenticateCommand, AuthenticateResult>
{
    public async Task<AuthenticateResult> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var user = await unitOfWork.Users.GetByCodeAsync(normalizedCode, cancellationToken);

        if (user is null)
        {
            throw new InvalidLoginCodeException();
        }

        user.EnsureCanAuthenticate();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var accessToken = tokenGenerator.CreateAccessToken(user);
        var refreshToken = tokenGenerator.CreateRefreshToken(user, utcNow);

        await unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new AuthenticateResult(
            accessToken,
            new RefreshTokenCookie(refreshToken.Token, refreshToken.Expires));
    }
}
