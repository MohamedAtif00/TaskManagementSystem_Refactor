using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface ITicketBankRepository
{
    Task<TicketBankItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TicketBankItem?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketBankItem>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TicketBankItem item, CancellationToken cancellationToken = default);
}
