using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

public sealed record ListSprintsQuery(bool? Archived = null) : IQuery<Result<IReadOnlyList<SprintListItemResult>>>;

public sealed class ListSprintsQueryHandler(ISprintsUnitOfWork unitOfWork)
    : IRequestHandler<ListSprintsQuery, Result<IReadOnlyList<SprintListItemResult>>>
{
    public async Task<Result<IReadOnlyList<SprintListItemResult>>> Handle(
        ListSprintsQuery request,
        CancellationToken cancellationToken)
    {
        var sprints = await unitOfWork.Sprints.ListAsync(request.Archived, cancellationToken);
        return Result.Ok<IReadOnlyList<SprintListItemResult>>(sprints.Select(SprintListItemResult.From).ToList());
    }
}
