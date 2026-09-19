using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.CreateUnit;

public sealed class CreateUnitCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateUnitCommand, Result<UnitDetailResult>>
{
    public async Task<Result<UnitDetailResult>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Units.ArchivedSubjectExistsAsync(request.SubjectId, cancellationToken))
            return Result.Fail<UnitDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.Units.ActiveSubjectExistsAsync(request.SubjectId, cancellationToken))
            return Result.Fail<UnitDetailResult>(CurriculumErrors.SubjectNotFound);
        var createResult = Domain.Unit.Create(request.Name, request.SubjectId);
        if (!createResult.IsSuccess) return Result.Fail<UnitDetailResult>(createResult.Error);
        await unitOfWork.Units.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(UnitDetailResult.From(createResult.Value));
    }
}

