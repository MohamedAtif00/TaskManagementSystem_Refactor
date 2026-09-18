using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TicketUnitOfWork(
    TicketDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<TicketDbContext>(context, domainEventDispatcher), ITicketUnitOfWork
{
    private readonly Lazy<TicketTaskRepository> _ticketTasks =
        LazyRepositoryFactory.Create(() => new TicketTaskRepository(context));

    private readonly Lazy<CommentRepository> _comments =
        LazyRepositoryFactory.Create(() => new CommentRepository(context));

    private readonly Lazy<TaskWorkTimeRepository> _taskWorkTimes =
        LazyRepositoryFactory.Create(() => new TaskWorkTimeRepository(context));

    public ITicketTaskRepository TicketTasks => _ticketTasks.Value;
    public ICommentRepository Comments => _comments.Value;
    public ITaskWorkTimeRepository TaskWorkTimes => _taskWorkTimes.Value;
}
