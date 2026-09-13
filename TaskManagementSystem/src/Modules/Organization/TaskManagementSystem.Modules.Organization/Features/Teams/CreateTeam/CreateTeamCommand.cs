using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;

public sealed record CreateTeamCommand(string Name) : ICommand<Result<TeamListItemResult>>;

public sealed class CreateTeamCommandHandler(IOrganizationUnitOfWork unitOfWork)
    : IRequestHandler<CreateTeamCommand, Result<TeamListItemResult>>
{
    public async Task<Result<TeamListItemResult>> Handle(
        CreateTeamCommand request,
        CancellationToken cancellationToken)
    {
        var createResult = Team.Create(request.Name);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TeamListItemResult>(createResult.Error);
        }

        await unitOfWork.Teams.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(new TeamListItemResult(createResult.Value.Id, createResult.Value.Name, 0));
    }
}
