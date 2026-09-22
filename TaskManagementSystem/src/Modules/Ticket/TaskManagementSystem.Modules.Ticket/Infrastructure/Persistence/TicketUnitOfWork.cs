using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TicketUnitOfWork(
    TicketDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<TicketDbContext>(context, domainEventDispatcher), ITicketUnitOfWork
{
    private readonly Lazy<TicketRepository> _ticketTasks =
        LazyRepositoryFactory.Create(() => new TicketRepository(context));

    private readonly Lazy<CommentRepository> _comments =
        LazyRepositoryFactory.Create(() => new CommentRepository(context));

    private readonly Lazy<TicketWorkTimeRepository> _taskWorkTimes =
        LazyRepositoryFactory.Create(() => new TicketWorkTimeRepository(context));

    private readonly Lazy<TicketActivityRepository> _taskActivities =
        LazyRepositoryFactory.Create(() => new TicketActivityRepository(context));

    public ITicketRepository Tickets => _ticketTasks.Value;
    public ICommentRepository Comments => _comments.Value;
    public ITicketWorkTimeRepository TicketWorkTimes => _taskWorkTimes.Value;
    public ITicketActivityRepository TaskActivities => _taskActivities.Value;
}
