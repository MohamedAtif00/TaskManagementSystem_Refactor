using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ArchiveUnit;

public sealed record ArchiveUnitCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveUnitCommandValidator : AbstractValidator<ArchiveUnitCommand>
{
    public ArchiveUnitCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveUnitCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveUnitCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await unitOfWork.Units.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (unit is null) return Result.Fail<NoValue>(CurriculumErrors.UnitNotFound);
        var archiveResult = unit.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
