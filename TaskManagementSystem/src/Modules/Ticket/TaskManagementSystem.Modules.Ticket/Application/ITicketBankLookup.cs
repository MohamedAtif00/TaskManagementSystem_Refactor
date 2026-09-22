namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketBankLookup
{
    Task<TicketBankSummary?> GetActiveByIdAsync(int taskBankId, CancellationToken cancellationToken = default);
}
