using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Integration;

internal sealed class OnUserMetadataChangedIntegrationEvent(
    IInboxGuard inboxGuard,
    IHrUnitOfWork unitOfWork)
    : INotificationHandler<UserMetadataChangedIntegrationEvent>
{
    private const string ConsumerName = "HR.OnUserMetadataChanged";

    public async Task Handle(UserMetadataChangedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (!await inboxGuard.TryBeginAsync(notification.Id, ConsumerName, notification, cancellationToken))
        {
            return;
        }

        var balance = await unitOfWork.EmployeeBalances.GetTrackedByUserIdAsync(notification.UserId, cancellationToken);
        if (balance is null)
        {
            return;
        }

        balance.SyncMetadata(notification.TeamId, notification.TeamleaderId, notification.RoleId);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
