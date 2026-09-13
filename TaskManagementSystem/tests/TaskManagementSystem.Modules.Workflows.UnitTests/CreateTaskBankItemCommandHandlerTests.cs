using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;
using Xunit;

namespace TaskManagementSystem.Modules.Workflows.UnitTests;

public sealed class CreateTaskBankItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTeamInvalid_ReturnsFailure()
    {
        var unitOfWork = Substitute.For<IWorkflowsUnitOfWork>();
        var organizationTeamLookup = Substitute.For<IOrganizationTeamLookup>();
        organizationTeamLookup.ActiveTeamExistsAsync(99, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateTaskBankItemCommandHandler(unitOfWork, organizationTeamLookup);
        var result = await handler.Handle(
            new CreateTaskBankItemCommand("Review", 30, TaskBankType.Review, false, 99),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("team_invalid");
        await unitOfWork.TaskBank.DidNotReceive().AddAsync(Arg.Any<TaskBankItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTeamValid_PersistsTaskBankItem()
    {
        var unitOfWork = Substitute.For<IWorkflowsUnitOfWork>();
        var organizationTeamLookup = Substitute.For<IOrganizationTeamLookup>();
        organizationTeamLookup.ActiveTeamExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        TaskBankItem? savedItem = null;
        unitOfWork.TaskBank.AddAsync(Arg.Do<TaskBankItem>(item => savedItem = item), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateTaskBankItemCommandHandler(unitOfWork, organizationTeamLookup);
        var result = await handler.Handle(
            new CreateTaskBankItemCommand("Review", 30, TaskBankType.Review, true, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedItem.Should().NotBeNull();
        savedItem!.Name.Should().Be("Review");
        savedItem.TeamLeaderOnly.Should().BeTrue();
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
