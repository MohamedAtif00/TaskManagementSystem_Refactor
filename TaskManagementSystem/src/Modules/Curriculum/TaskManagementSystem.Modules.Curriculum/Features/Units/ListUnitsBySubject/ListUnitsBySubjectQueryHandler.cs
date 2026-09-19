using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ListUnitsBySubject;

public sealed class ListUnitsBySubjectQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListUnitsBySubjectQuery, Result<IReadOnlyList<UnitListItemResult>>>
{
    public async Task<Result<IReadOnlyList<UnitListItemResult>>> Handle(ListUnitsBySubjectQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Units.ActiveSubjectExistsAsync(request.SubjectId, cancellationToken))
            return Result.Fail<IReadOnlyList<UnitListItemResult>>(CurriculumErrors.SubjectNotFound);
        var units = await unitOfWork.Units.ListActiveByParentIdAsync(request.SubjectId, cancellationToken);
        return Result.Ok<IReadOnlyList<UnitListItemResult>>(units.Select(UnitListItemResult.From).ToList());
    }
}

