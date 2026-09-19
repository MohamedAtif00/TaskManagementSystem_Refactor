using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;

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

