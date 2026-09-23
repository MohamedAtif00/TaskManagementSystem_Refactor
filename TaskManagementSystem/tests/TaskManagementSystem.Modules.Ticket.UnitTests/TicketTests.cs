using FluentAssertions;
using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;
using Xunit;

namespace TaskManagementSystem.Modules.Ticket.UnitTests;

public sealed class TicketTests
{
    [Fact]
    public void Create_WhenValid_ReturnsTicketInBacklog()
    {
        var result = Domain.Ticket.Create(
            "Write lesson plan",
            60,
            TicketPriority.Medium,
            learningObjectiveId: 1,
            stepId: 10,
            teamId: 2,
            teamLeaderOnly: false,
            userId: null,
            createdAt: DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Write lesson plan");
        result.Value.Status.Should().Be(DomainTicketStatus.Backlog);
        result.Value.StepId.Should().Be(10);
        result.Value.TeamId.Should().Be(2);
    }

    [Fact]
    public void Assign_WhenValid_SetsUserAndMovesToToDo()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var assignResult = ticket.Assign(5);

        assignResult.IsSuccess.Should().BeTrue();
        ticket.UserId.Should().Be(5);
        ticket.Status.Should().Be(DomainTicketStatus.ToDo);
    }

    [Fact]
    public void ProceedToStep_WhenLastStep_CompletesTicket()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var proceedResult = ticket.ProceedToStep(null);

        proceedResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.Done);
    }

    [Fact]
    public void Complete_WhenActive_SetsDoneStatus()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var addResult = ticket.Add(5);
        var startResult = ticket.Start(5);
        var completeResult = ticket.Complete(5, allowOtherAssignee: false);

        addResult.IsSuccess.Should().BeTrue();
        startResult.IsSuccess.Should().BeTrue();
        completeResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.Done);
    }

    [Fact]
    public void PauseWork_WhenDoing_MovesToToDo()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        ticket.Add(5);
        ticket.Start(5);

        var pauseResult = ticket.PauseWork();

        pauseResult.IsSuccess.Should().BeTrue();
        ticket.Pause.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.ToDo);
    }

    [Fact]
    public void ToggleFlag_WhenFlagged_MovesToToDo()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        ticket.Add(5);
        ticket.Start(5);

        var flagResult = ticket.ToggleFlag();

        flagResult.IsSuccess.Should().BeTrue();
        ticket.Flagged.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.ToDo);

        var startResult = ticket.Start(5);
        startResult.IsSuccess.Should().BeFalse();
        startResult.Error.Code.Should().Be("ticket_flagged");
    }

    [Fact]
    public void Rollback_WhenNotReview_IsRejected()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        ticket.Add(5);
        ticket.Start(5);

        var rollbackResult = ticket.Rollback(5, allowOtherAssignee: true);

        rollbackResult.IsSuccess.Should().BeFalse();
        rollbackResult.Error.Code.Should().Be("ticket_cannot_rollback");
    }

    [Fact]
    public void Rollback_WhenReviewDoingByAssignee_SetsRollback()
    {
        var ticket = Domain.Ticket.Create(
            "Review",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;
        ticket.PrepareSuccessor(teamLeaderOnly: false, isReview: true, fromId: null);
        ticket.Add(5);
        ticket.Start(5);

        var rollbackResult = ticket.Rollback(5, allowOtherAssignee: false);

        rollbackResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.Rollback);
    }

    [Fact]
    public void Skip_WhenPaused_ClearsPauseAndSetsDone()
    {
        var ticket = Domain.Ticket.Create(
            "Task",
            30,
            TicketPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;
        ticket.Add(5);
        ticket.Start(5);
        ticket.PauseWork();

        var skipResult = ticket.Skip();

        skipResult.IsSuccess.Should().BeTrue();
        ticket.Pause.Should().BeFalse();
        ticket.Status.Should().Be(DomainTicketStatus.Done);
    }
}
