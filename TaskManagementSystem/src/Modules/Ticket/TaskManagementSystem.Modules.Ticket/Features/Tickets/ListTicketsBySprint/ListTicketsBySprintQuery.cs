using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed record ListTicketsBySprintQuery(int SprintId)
    : IQuery<Result<IReadOnlyList<TicketListItemResult>>>;

public sealed class ListTicketsBySprintQueryValidator : AbstractValidator<ListTicketsBySprintQuery>
{
    public ListTicketsBySprintQueryValidator() => RuleFor(x => x.SprintId).GreaterThan(0);
}

public sealed class ListTicketsBySprintQueryHandler(SprintTicketsQueries sprintTicketsQueries)
    : IRequestHandler<ListTicketsBySprintQuery, Result<IReadOnlyList<TicketListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketListItemResult>>> Handle(
        ListTicketsBySprintQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await sprintTicketsQueries.ListBySprintAsync(request.SprintId, cancellationToken);
        return Result.Ok<IReadOnlyList<TicketListItemResult>>(tickets);
    }
}
