using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivityPaged;

public sealed class ListTicketActivityPagedQueryHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup)
    : IRequestHandler<ListTicketActivityPagedQuery, Result<TicketActivityListPageResult>>
{
    public async Task<Result<TicketActivityListPageResult>> Handle(
        ListTicketActivityPagedQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketActivityListPageResult>(TicketErrors.TicketNotFound);
        }

        var paging = PagingValidation.Resolve(request.Page, request.PageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<TicketActivityListPageResult>(paging.Error);
        }

        var (page, pageSize, _) = paging.Value;
        var (rows, totalCount) = await unitOfWork.TaskActivities.ListByTicketPagedAsync(
            request.TicketId,
            page,
            pageSize,
            cancellationToken);

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

        var items = rows.Select(row => new TicketActivityListItemResult(
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
            row.ActorOneId)).ToList();

        return Result.Ok(new TicketActivityListPageResult(items, page, pageSize, totalCount));
    }

    private static string? Name(IReadOnlyDictionary<int, string> names, int? id) =>
        id is int userId && names.TryGetValue(userId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : null;

    private static string? ActorName(IReadOnlyDictionary<int, string> names, int? id) =>
        id is null ? null : Name(names, id) ?? "User";
}
