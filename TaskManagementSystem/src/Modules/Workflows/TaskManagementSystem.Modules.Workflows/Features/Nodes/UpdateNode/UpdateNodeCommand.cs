using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;

public sealed record UpdateNodeCommand(
    int NodeId,
    string Name,
    bool IsStart,
    bool IsEnd) : ICommand<Result<NodeListItemResult>>;

public sealed class UpdateNodeCommandValidator : AbstractValidator<UpdateNodeCommand>
{
    public UpdateNodeCommandValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNodeCommand, Result<NodeListItemResult>>
{
    public async Task<Result<NodeListItemResult>> Handle(
        UpdateNodeCommand request,
        CancellationToken cancellationToken)
    {
        var node = await unitOfWork.Nodes.GetByIdTrackedAsync(request.NodeId, cancellationToken);
        if (node is null)
        {
            return Result.Fail<NodeListItemResult>(WorkflowsErrors.NodeNotFound);
        }

        var updateResult = node.Update(request.Name, request.IsStart, request.IsEnd);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<NodeListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(NodeListItemResult.From(node));
    }
}
