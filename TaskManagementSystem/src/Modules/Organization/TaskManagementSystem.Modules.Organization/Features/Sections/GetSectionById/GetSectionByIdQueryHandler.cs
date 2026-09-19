using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.GetSectionById;

public sealed class GetSectionByIdQueryHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<GetSectionByIdQuery, Result<SectionDetailResult>>
{
    public async Task<Result<SectionDetailResult>> Handle(
        GetSectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Sections.GetByIdAsync(request.SectionId, cancellationToken);
        if (section is null)
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.SectionNotFound);
        }

        return await SectionDetailMapper.MapAsync(section, unitOfWork, identityLookupQueries, cancellationToken);
    }
}

