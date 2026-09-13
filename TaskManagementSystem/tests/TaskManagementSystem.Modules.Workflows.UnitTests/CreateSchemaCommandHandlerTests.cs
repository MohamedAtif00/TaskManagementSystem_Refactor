using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;
using Xunit;

namespace TaskManagementSystem.Modules.Workflows.UnitTests;

public sealed class CreateSchemaCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNameValid_PersistsSchema()
    {
        var unitOfWork = Substitute.For<IWorkflowsUnitOfWork>();
        WorkflowSchema? savedSchema = null;
        unitOfWork.Schemas.AddAsync(Arg.Do<WorkflowSchema>(schema => savedSchema = schema), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateSchemaCommandHandler(unitOfWork);
        var result = await handler.Handle(
            new CreateSchemaCommand("Ticket Flow", "Default ticket workflow", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedSchema.Should().NotBeNull();
        savedSchema!.Name.Should().Be("Ticket Flow");
        await unitOfWork.Schemas.Received(1).AddAsync(Arg.Any<WorkflowSchema>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
