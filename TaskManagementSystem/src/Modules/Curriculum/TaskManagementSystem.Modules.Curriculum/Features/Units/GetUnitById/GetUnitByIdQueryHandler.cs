using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.GetUnitById;

public sealed class GetUnitByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetUnitByIdQuery, Result<UnitDetailResult>>
{
    public async Task<Result<UnitDetailResult>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await unitOfWork.Units.GetByIdAsync(request.Id, cancellationToken);
        return unit is null ? Result.Fail<UnitDetailResult>(CurriculumErrors.UnitNotFound) : Result.Ok(UnitDetailResult.From(unit));
    }
}

