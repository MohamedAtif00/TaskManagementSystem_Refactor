using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivity;

public sealed class ListTicketActivityQueryHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup)
    : IRequestHandler<ListTicketActivityQuery, Result<IReadOnlyList<TicketActivityListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketActivityListItemResult>>> Handle(
        ListTicketActivityQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<IReadOnlyList<TicketActivityListItemResult>>(TicketErrors.TicketNotFound);
        }

        var rows = await unitOfWork.TaskActivities.ListByTicketAsync(request.TicketId, cancellationToken);
        var userIds = rows
            .SelectMany(row => new int?[] { row.ActorOneId, row.ActorTwoId })
            .OfType<int>()
            .Distinct()
            .ToArray();
        var secondaryIds = rows
            .Select(row => row.TicketSecondaryId)
            .OfType<int>()
            .Distinct()
            .ToArray();
        var userNames = await identityUserLookup.ListNamesAsync(userIds, cancellationToken);
        var ticketNames = await unitOfWork.Tickets.ListNamesAsync(secondaryIds, cancellationToken);

        return Result.Ok<IReadOnlyList<TicketActivityListItemResult>>(
            rows.Select(row => new TicketActivityListItemResult(
                row.Id,
                (int)row.Type,
                TicketActivityText.Describe(
                    row.Type,
                    ticket.Name,
                    ActorName(userNames, row.ActorOneId),
                    ActorName(userNames, row.ActorTwoId),
                    Name(ticketNames, row.TicketSecondaryId),
                    row.AdditionalInfo),
                row.TimeStamp,
                row.ActorOneId)).ToList());
    }

    private static string? Name(IReadOnlyDictionary<int, string> names, int? id) =>
        id is int userId && names.TryGetValue(userId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : null;

    private static string? ActorName(IReadOnlyDictionary<int, string> names, int? id) =>
        id is null ? null : Name(names, id) ?? "User";
}
