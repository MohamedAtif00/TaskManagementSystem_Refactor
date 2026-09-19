using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ArchiveAcademicYear;

public sealed class ArchiveAcademicYearCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveAcademicYearCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var year = await unitOfWork.AcademicYears.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (year is null) return Result.Fail<NoValue>(CurriculumErrors.AcademicYearNotFound);
        var archiveResult = year.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

