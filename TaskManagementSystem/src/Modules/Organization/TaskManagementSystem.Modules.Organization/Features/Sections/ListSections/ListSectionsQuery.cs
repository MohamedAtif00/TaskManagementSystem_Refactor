using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.ListSections;

public sealed record ListSectionsQuery : IQuery<Result<IReadOnlyList<SectionListItemResult>>>;

public sealed class ListSectionsQueryHandler(IOrganizationUnitOfWork unitOfWork)
    : IRequestHandler<ListSectionsQuery, Result<IReadOnlyList<SectionListItemResult>>>
{
    public async Task<Result<IReadOnlyList<SectionListItemResult>>> Handle(
        ListSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var sections = await unitOfWork.Sections.ListActiveAsync(cancellationToken);
        var results = sections
            .Select(section => new SectionListItemResult(section.Id, section.Name))
            .ToList();

        return Result.Ok<IReadOnlyList<SectionListItemResult>>(results);
    }
}
