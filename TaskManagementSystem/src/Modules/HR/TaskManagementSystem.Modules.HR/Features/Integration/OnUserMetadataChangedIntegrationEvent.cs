using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Integration;

internal sealed class OnUserMetadataChangedIntegrationEvent(
    IInboxGuard inboxGuard,
    IHrUnitOfWork unitOfWork,
    OrgLookupQueries orgLookupQueries,
    ILogger<OnUserMetadataChangedIntegrationEvent> logger)
    : INotificationHandler<UserMetadataChangedIntegrationEvent>
{
    private const string ConsumerName = "HR.OnUserMetadataChanged";

    public async Task Handle(UserMetadataChangedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (!await inboxGuard.TryBeginAsync(notification.Id, ConsumerName, notification, cancellationToken))
        {
            return;
        }

        var teamleaderId = await orgLookupQueries.GetTeamleaderIdForTeamAsync(notification.TeamId, cancellationToken);
        await ApplyMetadataAsync(notification, teamleaderId, cancellationToken);

        try
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(
                exception,
                "Concurrency conflict syncing employee balance metadata for user {UserId}. Retrying once.",
                notification.UserId);

            unitOfWork.EmployeeBalances.DetachTracked(notification.UserId);
            await ApplyMetadataAsync(notification, teamleaderId, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
    }

    private async Task ApplyMetadataAsync(
        UserMetadataChangedIntegrationEvent notification,
        int? teamleaderId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.EmployeeBalances.GetTrackedByUserIdAsync(notification.UserId, cancellationToken);
        if (balance is null)
        {
            balance = EmployeeBalanceRecord.CreateWithDefaultEntitlements(
                notification.UserId,
                notification.TeamId,
                teamleaderId,
                notification.RoleId);
            await unitOfWork.EmployeeBalances.AddAsync(balance, cancellationToken);
        }
        else
        {
            balance.SyncMetadata(notification.TeamId, teamleaderId, notification.RoleId);
        }
    }
}
