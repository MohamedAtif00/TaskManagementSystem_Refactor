using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;
using Xunit;

namespace TaskManagementSystem.Modules.Ticket.UnitTests;

public sealed class CreateTicketCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLearningObjectiveMissing_ReturnsNotFound()
    {
        var unitOfWork = Substitute.For<ITicketUnitOfWork>();
        var learningObjectiveLookup = Substitute.For<ILearningObjectiveLookup>();
        learningObjectiveLookup.GetActiveByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((LearningObjectiveSummary?)null);

        var handler = new CreateTicketCommandHandler(
            unitOfWork,
            learningObjectiveLookup,
            Substitute.For<ITicketBankLookup>(),
            Substitute.For<IWorkflowStepLookup>(),
            Substitute.For<IIdentityUserLookup>(),
            Substitute.For<IOrganizationTeamLookup>(),
            Substitute.For<ITicketActivityWriter>(),
            Substitute.For<IRealtimePublisher>());

        var result = await handler.Handle(
            new CreateTicketCommand(1, 2, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("learning_objective_not_found");
    }

    [Fact]
    public async Task Handle_WhenValid_PersistsTicket()
    {
        var unitOfWork = Substitute.For<ITicketUnitOfWork>();
        Domain.Ticket? savedTicket = null;
        unitOfWork.Tickets.AddAsync(Arg.Do<Domain.Ticket>(ticket => savedTicket = ticket), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var learningObjectiveLookup = Substitute.For<ILearningObjectiveLookup>();
        learningObjectiveLookup.GetActiveByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new LearningObjectiveSummary(1, 99));

        var taskBankLookup = Substitute.For<ITicketBankLookup>();
        taskBankLookup.GetActiveByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(new TicketBankSummary(2, "Review content", 45, 3, false));

        var workflowStepLookup = Substitute.For<IWorkflowStepLookup>();
        workflowStepLookup.GetFirstStepAsync(99, 2, Arg.Any<CancellationToken>())
            .Returns(new WorkflowStepSummary(10, 45, 2, 5, 2));

        var organizationTeamLookup = Substitute.For<IOrganizationTeamLookup>();
        organizationTeamLookup.ActiveTeamExistsAsync(3, Arg.Any<CancellationToken>()).Returns(true);

        var realtimePublisher = Substitute.For<IRealtimePublisher>();
        realtimePublisher.PublishToGroupAsync(
                Arg.Any<string>(),
                Arg.Any<RealtimeMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new CreateTicketCommandHandler(
            unitOfWork,
            learningObjectiveLookup,
            taskBankLookup,
            workflowStepLookup,
            Substitute.For<IIdentityUserLookup>(),
            organizationTeamLookup,
            Substitute.For<ITicketActivityWriter>(),
            realtimePublisher);

        var result = await handler.Handle(
            new CreateTicketCommand(1, 2, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedTicket.Should().NotBeNull();
        savedTicket!.Name.Should().Be("Review content");
        savedTicket.Status.Should().Be(DomainTicketStatus.Backlog);
        await unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
        await realtimePublisher.Received(1).PublishToGroupAsync(
            Arg.Any<string>(),
            Arg.Any<RealtimeMessage>(),
            Arg.Any<CancellationToken>());
    }
}
