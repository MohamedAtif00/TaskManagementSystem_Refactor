using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.UpdateTicketBankItem;

public sealed record UpdateTicketBankItemCommand(
    int TicketBankId,
    string Name,
    int Duration,
    TicketBankType Type,
    bool TeamLeaderOnly,
    int TeamId) : ICommand<Result<TicketBankListItemResult>>;

