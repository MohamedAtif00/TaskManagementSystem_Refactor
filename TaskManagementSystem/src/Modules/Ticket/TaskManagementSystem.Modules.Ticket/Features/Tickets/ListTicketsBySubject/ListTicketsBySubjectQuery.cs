using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed record ListTicketsBySubjectQuery(int SubjectId)
    : IQuery<Result<IReadOnlyList<TicketListItemResult>>>;

public sealed class ListTicketsBySubjectQueryValidator : AbstractValidator<ListTicketsBySubjectQuery>
{
    public ListTicketsBySubjectQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

public sealed class ListTicketsBySubjectQueryHandler(SubjectTicketsQueries subjectTicketsQueries)
    : IRequestHandler<ListTicketsBySubjectQuery, Result<IReadOnlyList<TicketListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketListItemResult>>> Handle(
        ListTicketsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await subjectTicketsQueries.ListBySubjectAsync(request.SubjectId, cancellationToken);
        return Result.Ok<IReadOnlyList<TicketListItemResult>>(tickets);
    }
}
