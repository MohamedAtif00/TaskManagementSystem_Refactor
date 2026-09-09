using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.AboutMe;

public sealed class AboutMeQueryHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<AboutMeQuery, Result<AboutMeResult>>
{
    public async Task<Result<AboutMeResult>> Handle(AboutMeQuery request, CancellationToken cancellationToken)
    {
        var profile = await unitOfWork.AboutMe.GetAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Fail<AboutMeResult>(IdentityErrors.UserNotFound);
        }

        return Result.Ok(new AboutMeResult(
            profile.Id,
            profile.Name,
            profile.RoleId,
            profile.RoleName,
            profile.Permissions,
            profile.TeamName,
            profile.Notifications));
    }
}
