using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Integration;

internal sealed class OnUserCreatedIntegrationEvent(
    IInboxGuard inboxGuard,
    IHrUnitOfWork unitOfWork)
    : INotificationHandler<UserCreatedIntegrationEvent>
{
    private const string ConsumerName = "HR.OnUserCreated";

    public async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (!await inboxGuard.TryBeginAsync(notification.Id, ConsumerName, notification, cancellationToken))
        {
            return;
        }

        var balance = EmployeeBalanceRecord.CreateForUser(
            notification.UserId,
            notification.TeamId,
            notification.TeamleaderId,
            notification.RoleId,
            notification.AnnualLeave,
            notification.AnnualLeaveMax,
            notification.EmergencyLeave,
            notification.EmergencyLeaveMax,
            notification.SickLeave,
            notification.PermissionBalance,
            notification.PermissionMax,
            notification.WorkFromHome,
            notification.WorkFromHomeMax,
            notification.FromNextBalanceDaysUsed,
            notification.OldAnnualBalance);

        await unitOfWork.EmployeeBalances.AddAsync(balance, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
