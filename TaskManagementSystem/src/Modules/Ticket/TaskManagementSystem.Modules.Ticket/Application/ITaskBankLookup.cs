namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITaskBankLookup
{
    Task<TaskBankSummary?> GetActiveByIdAsync(int taskBankId, CancellationToken cancellationToken = default);
}
