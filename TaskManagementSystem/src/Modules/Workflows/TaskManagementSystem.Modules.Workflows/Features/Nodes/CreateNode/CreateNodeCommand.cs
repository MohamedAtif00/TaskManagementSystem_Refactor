using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;

public sealed record CreateNodeCommand(
    int SchemaId,
    string Name,
    bool IsStart,
    bool IsEnd) : ICommand<Result<NodeListItemResult>>;

public sealed class CreateNodeCommandValidator : AbstractValidator<CreateNodeCommand>
{
    public CreateNodeCommandValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<CreateNodeCommand, Result<NodeListItemResult>>
{
    public async Task<Result<NodeListItemResult>> Handle(
        CreateNodeCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Nodes.SchemaExistsActiveAsync(request.SchemaId, cancellationToken))
        {
            return Result.Fail<NodeListItemResult>(WorkflowsErrors.SchemaNotFound);
        }

        var order = await unitOfWork.Nodes.GetNextOrderAsync(request.SchemaId, cancellationToken);
        var createResult = WorkflowNode.Create(
            request.Name,
            order,
            request.IsStart,
            request.IsEnd,
            request.SchemaId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<NodeListItemResult>(createResult.Error);
        }

        await unitOfWork.Nodes.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(NodeListItemResult.From(createResult.Value));
    }
}
