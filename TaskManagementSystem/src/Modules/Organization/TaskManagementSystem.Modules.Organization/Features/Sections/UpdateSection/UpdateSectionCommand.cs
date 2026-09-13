using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.UpdateSection;

public sealed record UpdateSectionCommand(
    int SectionId,
    string Name,
    int HeadId,
    IReadOnlyList<int> TeamIds) : ICommand<Result<SectionDetailResult>>;

public sealed class UpdateSectionCommandHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<UpdateSectionCommand, Result<SectionDetailResult>>
{
    public async Task<Result<SectionDetailResult>> Handle(
        UpdateSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Sections.GetByIdTrackedAsync(request.SectionId, cancellationToken);
        if (section is null)
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.SectionNotFound);
        }

        if (await unitOfWork.Sections.ExistsActiveByNameAsync(
                request.Name,
                section.Id,
                cancellationToken))
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.SectionAlreadyExists);
        }

        var head = await identityLookupQueries.GetActiveUserByIdAsync(request.HeadId, cancellationToken);
        if (head is null)
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.UserNotFound);
        }

        if (!await unitOfWork.Teams.AllActiveExistAsync(request.TeamIds, cancellationToken))
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.TeamInvalid);
        }

        var updateResult = section.Update(request.Name, request.HeadId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<SectionDetailResult>(updateResult.Error);
        }

        section.ReplaceTeamLinks(request.TeamIds);
        await unitOfWork.CommitAsync(cancellationToken);

        return await SectionDetailMapper.MapAsync(section, head, unitOfWork, cancellationToken);
    }
}
