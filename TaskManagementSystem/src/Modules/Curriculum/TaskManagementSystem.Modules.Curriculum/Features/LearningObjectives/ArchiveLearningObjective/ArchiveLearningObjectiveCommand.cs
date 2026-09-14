using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ArchiveLearningObjective;

public sealed record ArchiveLearningObjectiveCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveLearningObjectiveCommandValidator : AbstractValidator<ArchiveLearningObjectiveCommand>
{
    public ArchiveLearningObjectiveCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveLearningObjectiveCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveLearningObjectiveCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveLearningObjectiveCommand request, CancellationToken cancellationToken)
    {
        var objective = await unitOfWork.LearningObjectives.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (objective is null) return Result.Fail<NoValue>(CurriculumErrors.LearningObjectiveNotFound);
        var archiveResult = objective.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
