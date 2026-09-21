using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketStats;

public sealed class GetTicketStatsQueryHandler(TicketStatsQueries ticketStatsQueries)
    : IRequestHandler<GetTicketStatsQuery, Result<TicketStatsResult>>
{
    public async Task<Result<TicketStatsResult>> Handle(
        GetTicketStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await ticketStatsQueries.GetAsync(cancellationToken);
        return Result.Ok(stats);
    }
}
