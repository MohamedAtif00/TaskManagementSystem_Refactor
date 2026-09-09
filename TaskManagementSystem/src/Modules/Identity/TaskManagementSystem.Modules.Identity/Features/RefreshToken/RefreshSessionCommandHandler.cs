using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.RefreshToken;

public sealed class RefreshSessionCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshSessionCommand, Result<RefreshSessionResult>>
{
    public async Task<Result<RefreshSessionResult>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Fail<RefreshSessionResult>(IdentityErrors.InvalidRefreshToken("No refresh token"));
        }

        var existingToken = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null)
        {
            return Result.Fail<RefreshSessionResult>(IdentityErrors.InvalidRefreshToken("Invalid Refresh Token"));
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var rotateResult = existingToken.CanRotate(utcNow);
        if (!rotateResult.IsSuccess)
        {
            return Result.Fail<RefreshSessionResult>(
                IdentityResultMapper.ToApplicationError(rotateResult.Error));
        }

        var user = await unitOfWork.Users.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail<RefreshSessionResult>(IdentityErrors.InvalidRefreshToken("Invalid Refresh Token"));
        }

        var authResult = user.CanAuthenticate();
        if (!authResult.IsSuccess)
        {
            return Result.Fail<RefreshSessionResult>(IdentityResultMapper.ToApplicationError(authResult.Error));
        }

        existingToken.MarkUsed();
        var newRefreshToken = tokenGenerator.CreateRefreshToken(user, utcNow);
        await unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(new RefreshSessionResult(
            tokenGenerator.CreateAccessToken(user),
            new RefreshSessionCookie(newRefreshToken.Token, newRefreshToken.Expires)));
    }
}
