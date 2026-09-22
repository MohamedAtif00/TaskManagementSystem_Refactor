using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.ListTicketBank;

public sealed record ListTicketBankQuery : IQuery<Result<IReadOnlyList<TicketBankListItemResult>>>;

