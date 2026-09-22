using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketUnitOfWork : IUnitOfWork
{
    ITicketRepository Tickets { get; }
    ICommentRepository Comments { get; }
    ITicketWorkTimeRepository TicketWorkTimes { get; }
    ITicketActivityRepository TaskActivities { get; }
}
