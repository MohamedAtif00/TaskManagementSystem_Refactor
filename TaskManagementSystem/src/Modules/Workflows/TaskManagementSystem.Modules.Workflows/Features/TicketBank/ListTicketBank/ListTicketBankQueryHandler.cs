using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.ListTicketBank;

public sealed class ListTicketBankQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListTicketBankQuery, Result<IReadOnlyList<TicketBankListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketBankListItemResult>>> Handle(
        ListTicketBankQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.TicketBank.ListActiveAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<TicketBankListItemResult>>(items.Select(TicketBankListItemResult.From).ToList());
    }
}

