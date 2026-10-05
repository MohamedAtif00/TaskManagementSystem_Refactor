using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectTicketSheet;

public sealed record ListSubjectTicketSheetQuery(int SubjectId)
    : IQuery<Result<IReadOnlyList<TicketSheetItemResult>>>;
