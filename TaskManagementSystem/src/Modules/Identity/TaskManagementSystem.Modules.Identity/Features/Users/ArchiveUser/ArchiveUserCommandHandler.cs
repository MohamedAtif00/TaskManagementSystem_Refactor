using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ArchiveUser;

public sealed class ArchiveUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    OrganizationLookupQueries organizationLookupQueries)
    : IRequestHandler<ArchiveUserCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdTrackedAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.UserNotFound);
        }

        if (await organizationLookupQueries.IsActiveSectionHeadAsync(request.UserId, cancellationToken))
        {
            return Result.Fail<NoValue>(IdentityErrors.UserIsSectionHead);
        }

        user.Archive();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
