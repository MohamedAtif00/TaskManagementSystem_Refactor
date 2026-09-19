using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;

public sealed class CreateAcademicYearCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateAcademicYearCommand, Result<AcademicYearDetailResult>>
{
    public async Task<Result<AcademicYearDetailResult>> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var createResult = AcademicYear.Create(request.Name, request.Description);
        if (!createResult.IsSuccess) return Result.Fail<AcademicYearDetailResult>(createResult.Error);
        await unitOfWork.AcademicYears.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(AcademicYearDetailResult.From(createResult.Value));
    }
}

