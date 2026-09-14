using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketUnitOfWork : IUnitOfWork
{
    ITicketTaskRepository TicketTasks { get; }
    ICommentRepository Comments { get; }
    ITaskWorkTimeRepository TaskWorkTimes { get; }
}
