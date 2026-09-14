using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;
using Xunit;

namespace TaskManagementSystem.Modules.Curriculum.UnitTests;

public sealed class CreateLearningObjectiveCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSchemaInvalid_ReturnsSchemaNotFound()
    {
        var unitOfWork = Substitute.For<ICurriculumUnitOfWork>();
        var schemaLookup = Substitute.For<IWorkflowSchemaLookup>();
        schemaLookup.ActiveSchemaExistsAsync(999, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateLearningObjectiveCommandHandler(unitOfWork, schemaLookup);
        var result = await handler.Handle(
            new CreateLearningObjectiveCommand(1, 999, "LO-1", "tag", "template", "env"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("schema_not_found");
        await unitOfWork.LearningObjectives.DidNotReceive().AddAsync(Arg.Any<Domain.LearningObjective>(), Arg.Any<CancellationToken>());
    }
}
