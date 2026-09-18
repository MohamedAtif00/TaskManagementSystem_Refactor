using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;
using TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.AddSprintLearningObjectives;
using Xunit;

namespace TaskManagementSystem.Modules.Sprints.UnitTests;

public sealed class AddSprintLearningObjectivesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSprintMissing_ReturnsNotFound()
    {
        var unitOfWork = Substitute.For<ISprintsUnitOfWork>();
        unitOfWork.Sprints.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Sprint?)null);

        var handler = new AddSprintLearningObjectivesCommandHandler(
            unitOfWork,
            Substitute.For<ILearningObjectiveLookup>());

        var result = await handler.Handle(
            new AddSprintLearningObjectivesCommand(1, [10]),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("sprint_not_found");
    }

    [Fact]
    public async Task Handle_WhenLearningObjectiveMissing_ReturnsNotFound()
    {
        var unitOfWork = Substitute.For<ISprintsUnitOfWork>();
        unitOfWork.Sprints.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Sprint.Create("Sprint", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(7)).Value);

        var learningObjectiveLookup = Substitute.For<ILearningObjectiveLookup>();
        learningObjectiveLookup.ActiveLearningObjectivesExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new AddSprintLearningObjectivesCommandHandler(unitOfWork, learningObjectiveLookup);
        var result = await handler.Handle(
            new AddSprintLearningObjectivesCommand(1, [10]),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("learning_objective_not_found");
    }
}
