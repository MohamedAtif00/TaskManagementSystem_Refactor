using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;
using Xunit;

namespace TaskManagementSystem.Modules.Workflows.UnitTests;

public sealed class CreateNodeCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSchemaMissing_ReturnsFailure()
    {
        var unitOfWork = Substitute.For<IWorkflowsUnitOfWork>();
        unitOfWork.Nodes.SchemaExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateNodeCommandHandler(unitOfWork);
        var result = await handler.Handle(
            new CreateNodeCommand(1, "Start", true, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("schema_not_found");
    }

    [Fact]
    public async Task Handle_WhenSchemaExists_AssignsNextOrder()
    {
        var unitOfWork = Substitute.For<IWorkflowsUnitOfWork>();
        unitOfWork.Nodes.SchemaExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        unitOfWork.Nodes.GetNextOrderAsync(1, Arg.Any<CancellationToken>()).Returns(2);

        WorkflowNode? savedNode = null;
        unitOfWork.Nodes.AddAsync(Arg.Do<WorkflowNode>(node => savedNode = node), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateNodeCommandHandler(unitOfWork);
        var result = await handler.Handle(
            new CreateNodeCommand(1, "Review", false, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedNode!.Order.Should().Be(2);
        savedNode.IsStart.Should().BeFalse();
    }
}
