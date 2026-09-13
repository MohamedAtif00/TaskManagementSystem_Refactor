using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;

public sealed record ArchiveStepCommand(int StepId) : ICommand<Result<NoValue>>;

public sealed class ArchiveStepCommandValidator : AbstractValidator<ArchiveStepCommand>
{
    public ArchiveStepCommandValidator()
    {
        RuleFor(x => x.StepId).GreaterThan(0);
    }
}

public sealed class ArchiveStepCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveStepCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveStepCommand request,
        CancellationToken cancellationToken)
    {
        var step = await unitOfWork.Steps.GetByIdTrackedAsync(request.StepId, cancellationToken);
        if (step is null)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.StepNotFound);
        }

        step.Archive();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
