using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;
using Xunit;

namespace TaskManagementSystem.Modules.Sprints.UnitTests;

public sealed class CreateSprintCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_PersistsSprint()
    {
        var unitOfWork = Substitute.For<ISprintsUnitOfWork>();
        Sprint? savedSprint = null;
        unitOfWork.Sprints.AddAsync(Arg.Do<Sprint>(sprint => savedSprint = sprint), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateSprintCommandHandler(unitOfWork);
        var startDate = new DateTime(2026, 9, 1);
        var endDate = new DateTime(2026, 9, 14);

        var result = await handler.Handle(
            new CreateSprintCommand("Sprint 1", "First sprint", startDate, endDate),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedSprint.Should().NotBeNull();
        savedSprint!.Name.Should().Be("Sprint 1");
        savedSprint.IsArchived.Should().BeFalse();
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEndDateBeforeStartDate_ReturnsValidationError()
    {
        var handler = new CreateSprintCommandHandler(Substitute.For<ISprintsUnitOfWork>());
        var result = await handler.Handle(
            new CreateSprintCommand("Sprint 1", "Invalid", new DateTime(2026, 9, 14), new DateTime(2026, 9, 1)),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("sprint_invalid_dates");
    }
}
