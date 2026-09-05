using MediatR;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.AboutMe;

public sealed class AboutMeQueryHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<AboutMeQuery, AboutMeResult>
{
    public async Task<AboutMeResult> Handle(AboutMeQuery request, CancellationToken cancellationToken)
    {
        var profile = await unitOfWork.AboutMe.GetAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            throw new UserNotFoundException();
        }

        return new AboutMeResult(
            profile.Id,
            profile.Name,
            profile.Role,
            profile.TeamName ?? string.Empty,
            profile.Notifications);
    }
}
