using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.CreateTicketBankItem;

public sealed record CreateTicketBankItemCommand(
    string Name,
    int Duration,
    TicketBankType Type,
    bool TeamLeaderOnly,
    int TeamId) : ICommand<Result<TicketBankListItemResult>>;

