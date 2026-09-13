using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.DeactivateTaskBankItem;

public sealed record DeactivateTaskBankItemCommand(int TaskBankId) : ICommand<Result<NoValue>>;

public sealed class DeactivateTaskBankItemCommandValidator : AbstractValidator<DeactivateTaskBankItemCommand>
{
    public DeactivateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.TaskBankId).GreaterThan(0);
    }
}

public sealed class DeactivateTaskBankItemCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateTaskBankItemCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeactivateTaskBankItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await unitOfWork.TaskBank.GetByIdTrackedAsync(request.TaskBankId, cancellationToken);
        if (item is null)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.TaskBankNotFound);
        }

        item.Deactivate();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
