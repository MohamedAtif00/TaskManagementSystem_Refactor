using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Persistence.Behaviors;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

namespace TaskManagementSystem.Modules.HR.Infrastructure;

internal sealed class HrTransactionBehavior<TRequest, TResponse>(
    HrDbContext context,
    ILogger<HrTransactionBehavior<TRequest, TResponse>> logger)
    : TransactionBehaviorBase<TRequest, TResponse, HrDbContext>(context, logger)
    where TRequest : notnull
{
    protected override bool AppliesTo(TRequest request) => request is IHrCommand;
}
