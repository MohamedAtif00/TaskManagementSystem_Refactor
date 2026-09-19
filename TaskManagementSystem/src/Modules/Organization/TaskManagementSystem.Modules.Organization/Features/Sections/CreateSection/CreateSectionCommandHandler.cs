using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.CreateSection;

public sealed class CreateSectionCommandHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<CreateSectionCommand, Result<SectionDetailResult>>
{
    public async Task<Result<SectionDetailResult>> Handle(
        CreateSectionCommand request,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Sections.ExistsActiveByNameAsync(request.Name, cancellationToken: cancellationToken))
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

        var createResult = Section.Create(request.Name, request.HeadId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<SectionDetailResult>(createResult.Error);
        }

        createResult.Value.ReplaceTeamLinks(request.TeamIds);
        await unitOfWork.Sections.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await SectionDetailMapper.MapAsync(createResult.Value, head, unitOfWork, cancellationToken);
    }
}

