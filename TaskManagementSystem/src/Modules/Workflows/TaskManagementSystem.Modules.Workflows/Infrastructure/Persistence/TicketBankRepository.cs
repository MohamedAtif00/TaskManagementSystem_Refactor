using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class TicketBankRepository(WorkflowsDbContext context)
    : GenericRepository<TicketBankItem, WorkflowsDbContext>(context), ITicketBankRepository
{
    public Task<TicketBankItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(item => item.Id == id && item.Active, cancellationToken);

    public Task<TicketBankItem?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(item => item.Id == id && item.Active, cancellationToken);

    public async Task<IReadOnlyList<TicketBankItem>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(item => item.Active)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ExistsReadOnlyAsync(item => item.Id == id && item.Active, cancellationToken);

    public Task AddAsync(TicketBankItem item, CancellationToken cancellationToken = default) =>
        AddEntityAsync(item, cancellationToken);
}
