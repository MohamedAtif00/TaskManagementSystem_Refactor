using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.ArchiveTeam;

public sealed record ArchiveTeamCommand(int TeamId) : ICommand<Result<NoValue>>;

public sealed class ArchiveTeamCommandHandler(IOrganizationUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveTeamCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveTeamCommand request,
        CancellationToken cancellationToken)
    {
        var team = await unitOfWork.Teams.GetByIdTrackedAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Fail<NoValue>(OrganizationErrors.TeamNotFound);
        }

        var archiveResult = team.Archive();
        if (!archiveResult.IsSuccess)
        {
            return archiveResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
