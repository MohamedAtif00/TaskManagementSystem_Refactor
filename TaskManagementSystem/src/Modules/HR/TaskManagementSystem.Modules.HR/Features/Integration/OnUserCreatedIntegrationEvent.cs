using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Integration;

internal sealed class OnUserCreatedIntegrationEvent(
    IInboxGuard inboxGuard,
    IHrUnitOfWork unitOfWork,
    OrgLookupQueries orgLookupQueries)
    : INotificationHandler<UserCreatedIntegrationEvent>
{
    private const string ConsumerName = "HR.OnUserCreated";

    public async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (!await inboxGuard.TryBeginAsync(notification.Id, ConsumerName, notification, cancellationToken))
        {
            return;
        }

        var teamleaderId = await orgLookupQueries.GetTeamleaderIdForTeamAsync(notification.TeamId, cancellationToken);
        var balance = EmployeeBalanceRecord.CreateWithDefaultEntitlements(
            notification.UserId,
            notification.TeamId,
            teamleaderId,
            notification.RoleId);

        await unitOfWork.EmployeeBalances.AddAsync(balance, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
