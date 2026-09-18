using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketCommand;

public interface ITicketCommand<out TResult> : ICommand<TResult>, ITicketCommand;
