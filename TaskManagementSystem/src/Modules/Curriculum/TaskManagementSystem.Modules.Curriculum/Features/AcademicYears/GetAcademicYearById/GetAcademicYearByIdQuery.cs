using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.GetAcademicYearById;

public sealed record GetAcademicYearByIdQuery(int Id) : IQuery<Result<AcademicYearDetailResult>>;

public sealed class GetAcademicYearByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetAcademicYearByIdQuery, Result<AcademicYearDetailResult>>
{
    public async Task<Result<AcademicYearDetailResult>> Handle(GetAcademicYearByIdQuery request, CancellationToken cancellationToken)
    {
        var year = await unitOfWork.AcademicYears.GetByIdAsync(request.Id, cancellationToken);
        return year is null ? Result.Fail<AcademicYearDetailResult>(CurriculumErrors.AcademicYearNotFound) : Result.Ok(AcademicYearDetailResult.From(year));
    }
}
