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

        var completeResult = ticket.Complete();

        completeResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTicketStatus.Done);
    }
}
