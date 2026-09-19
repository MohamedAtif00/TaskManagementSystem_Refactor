using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.ArchiveSection;

public sealed class ArchiveSectionCommandHandler(IOrganizationUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveSectionCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Sections.GetByIdTrackedAsync(request.SectionId, cancellationToken);
        if (section is null)
        {
            return Result.Fail<NoValue>(OrganizationErrors.SectionNotFound);
        }

        var archiveResult = section.Archive();
        if (!archiveResult.IsSuccess)
        {
            return archiveResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

