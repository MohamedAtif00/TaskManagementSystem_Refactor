using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketSummary;

public sealed class GetTicketSummaryQueryHandler(TicketSummaryQueries ticketSummaryQueries)
    : IRequestHandler<GetTicketSummaryQuery, Result<TicketSummaryResult>>
{
    public Task<Result<TicketSummaryResult>> Handle(
        GetTicketSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var hasSubject = request.SubjectId is > 0;
        var hasSprint = request.SprintId is > 0;

        if (hasSubject == hasSprint)
        {
            return Task.FromResult(Result.Fail<TicketSummaryResult>(
                new ResultError("invalid_scope", "Provide exactly one of subjectId or sprintId.")));
        }

        return hasSubject
            ? ticketSummaryQueries.GetBySubjectIdAsync(request.SubjectId!.Value, cancellationToken)
            : ticketSummaryQueries.GetBySprintIdAsync(request.SprintId!.Value, cancellationToken);
    }
}
