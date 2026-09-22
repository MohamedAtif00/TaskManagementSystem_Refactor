using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TicketRepository(TicketDbContext context)
    : GenericRepository<Domain.Ticket, TicketDbContext>(context), ITicketRepository
{
    public Task<Domain.Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(task => task.Id == id && !task.Archived, cancellationToken);

    public Task<Domain.Ticket?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(task => task.Id == id && !task.Archived, cancellationToken);

    public async Task<IReadOnlyList<Domain.Ticket>> ListByLearningObjectiveAsync(
        int learningObjectiveId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(task => task.LearningObjectiveId == learningObjectiveId && !task.Archived)
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Domain.Ticket ticketTask, CancellationToken cancellationToken = default) =>
        AddEntityAsync(ticketTask, cancellationToken);
}
