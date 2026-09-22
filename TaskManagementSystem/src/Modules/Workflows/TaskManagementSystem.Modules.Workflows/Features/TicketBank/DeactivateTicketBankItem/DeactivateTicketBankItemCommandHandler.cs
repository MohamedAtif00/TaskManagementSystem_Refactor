using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.DeactivateTicketBankItem;

public sealed class DeactivateTicketBankItemCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateTicketBankItemCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeactivateTicketBankItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await unitOfWork.TicketBank.GetByIdTrackedAsync(request.TicketBankId, cancellationToken);
        if (item is null)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.TicketBankNotFound);
        }

        item.Deactivate();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

