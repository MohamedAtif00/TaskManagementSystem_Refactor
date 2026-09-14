using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ListAcademicYears;

public sealed record ListAcademicYearsQuery : IQuery<Result<IReadOnlyList<AcademicYearListItemResult>>>;

public sealed class ListAcademicYearsQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListAcademicYearsQuery, Result<IReadOnlyList<AcademicYearListItemResult>>>
{
    public async Task<Result<IReadOnlyList<AcademicYearListItemResult>>> Handle(
        ListAcademicYearsQuery request,
        CancellationToken cancellationToken)
    {
        var years = await unitOfWork.AcademicYears.ListActiveAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<AcademicYearListItemResult>>(years.Select(AcademicYearListItemResult.From).ToList());
    }
}
