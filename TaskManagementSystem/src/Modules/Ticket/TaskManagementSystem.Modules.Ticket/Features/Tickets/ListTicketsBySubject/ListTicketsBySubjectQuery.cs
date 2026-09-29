using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed record ListTicketsBySubjectQuery(
    int SubjectId,
    TicketListFilter Filter,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
