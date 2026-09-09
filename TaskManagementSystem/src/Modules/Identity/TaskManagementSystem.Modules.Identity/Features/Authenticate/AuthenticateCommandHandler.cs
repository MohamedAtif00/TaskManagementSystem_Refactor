using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Authenticate;

public sealed class AuthenticateCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    TimeProvider timeProvider)
    : IRequestHandler<AuthenticateCommand, Result<AuthenticateResult>>
{
    public async Task<Result<AuthenticateResult>> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var loginCodeResult = LoginCode.TryCreate(request.Code);
        if (!loginCodeResult.IsSuccess)
        {
            return Result.Fail<AuthenticateResult>(IdentityErrors.InvalidLoginCode);
        }

        var user = await unitOfWork.Users.GetByCodeAsync(loginCodeResult.Value.Value, cancellationToken);
        if (user is null)
        {
            return Result.Fail<AuthenticateResult>(IdentityErrors.InvalidLoginCode);
        }

        var authResult = user.CanAuthenticate();
        if (!authResult.IsSuccess)
        {
            return Result.Fail<AuthenticateResult>(IdentityResultMapper.ToApplicationError(authResult.Error));
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var accessToken = tokenGenerator.CreateAccessToken(user);
        var refreshToken = tokenGenerator.CreateRefreshToken(user, utcNow);

        await unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(new AuthenticateResult(
            accessToken,
            new RefreshTokenCookie(refreshToken.Token, refreshToken.Expires)));
    }
}
