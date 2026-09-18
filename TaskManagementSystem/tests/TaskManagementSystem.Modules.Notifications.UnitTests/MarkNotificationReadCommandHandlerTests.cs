using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Notifications.Application;
using TaskManagementSystem.Modules.Notifications.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkNotificationRead;
using Xunit;

namespace TaskManagementSystem.Modules.Notifications.UnitTests;

public sealed class MarkNotificationReadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotificationMissing_ReturnsNotFound()
    {
        var unitOfWork = Substitute.For<INotificationsUnitOfWork>();
        unitOfWork.Notifications.GetByIdTrackedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var handler = new MarkNotificationReadCommandHandler(unitOfWork);
        var result = await handler.Handle(new MarkNotificationReadCommand(1, 99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("notification_not_found");
    }

    [Fact]
    public async Task Handle_WhenValid_MarksNotificationRead()
    {
        var notification = Notification.Create(
            99,
            "Task assigned",
            "You have a new task.",
            "Task",
            "Assignment").Value;

        var unitOfWork = Substitute.For<INotificationsUnitOfWork>();
        unitOfWork.Notifications.GetByIdTrackedAsync(1, 99, Arg.Any<CancellationToken>())
            .Returns(notification);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new MarkNotificationReadCommandHandler(unitOfWork);
        var result = await handler.Handle(new MarkNotificationReadCommand(1, 99), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRead.Should().BeTrue();
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
