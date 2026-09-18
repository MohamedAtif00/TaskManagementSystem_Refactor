using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Persistence.Behaviors;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure;

internal sealed class TicketTransactionBehavior<TRequest, TResponse>(
    TicketDbContext context,
    ILogger<TicketTransactionBehavior<TRequest, TResponse>> logger)
    : TransactionBehaviorBase<TRequest, TResponse, TicketDbContext>(context, logger)
    where TRequest : notnull
{
    protected override bool AppliesTo(TRequest request) => request is ITicketCommand;
}
