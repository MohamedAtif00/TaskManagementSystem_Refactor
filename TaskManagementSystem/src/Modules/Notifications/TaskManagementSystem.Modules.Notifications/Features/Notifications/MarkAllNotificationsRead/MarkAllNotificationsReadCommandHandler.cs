using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(INotificationsUnitOfWork unitOfWork)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.Notifications.MarkAllReadAsync(request.UserId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

