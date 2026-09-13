using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ListStepsByNode;

public sealed record ListStepsByNodeQuery(int NodeId) : IQuery<Result<IReadOnlyList<StepListItemResult>>>;

public sealed class ListStepsByNodeQueryValidator : AbstractValidator<ListStepsByNodeQuery>
{
    public ListStepsByNodeQueryValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
    }
}

public sealed class ListStepsByNodeQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListStepsByNodeQuery, Result<IReadOnlyList<StepListItemResult>>>
{
    public async Task<Result<IReadOnlyList<StepListItemResult>>> Handle(
        ListStepsByNodeQuery request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Steps.NodeExistsActiveAsync(request.NodeId, cancellationToken))
        {
            return Result.Fail<IReadOnlyList<StepListItemResult>>(WorkflowsErrors.NodeNotFound);
        }

        var steps = await unitOfWork.Steps.ListActiveByNodeAsync(request.NodeId, cancellationToken);
        return Result.Ok<IReadOnlyList<StepListItemResult>>(steps.Select(StepListItemResult.From).ToList());
    }
}
