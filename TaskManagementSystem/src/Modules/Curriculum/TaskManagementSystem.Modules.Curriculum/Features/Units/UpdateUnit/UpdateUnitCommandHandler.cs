using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.UpdateUnit;

public sealed class UpdateUnitCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUnitCommand, Result<UnitDetailResult>>
{
    public async Task<Result<UnitDetailResult>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await unitOfWork.Units.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (unit is null) return Result.Fail<UnitDetailResult>(CurriculumErrors.UnitNotFound);
        var updateResult = unit.Update(request.Name);
        if (!updateResult.IsSuccess) return Result.Fail<UnitDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(UnitDetailResult.From(unit));
    }
}

