using FluentAssertions;
using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using DomainTaskPriority = TaskManagementSystem.Modules.Ticket.Domain.TaskPriority;
using Xunit;

namespace TaskManagementSystem.Modules.Ticket.UnitTests;

public sealed class TicketTaskTests
{
    [Fact]
    public void Create_WhenValid_ReturnsTicketInBacklog()
    {
        var result = TicketTask.Create(
            "Write lesson plan",
            60,
            TaskPriority.Medium,
            learningObjectiveId: 1,
            stepId: 10,
            teamId: 2,
            teamLeaderOnly: false,
            userId: null,
            createdAt: DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Write lesson plan");
        result.Value.Status.Should().Be(DomainTaskStatus.Backlog);
        result.Value.StepId.Should().Be(10);
        result.Value.TeamId.Should().Be(2);
    }

    [Fact]
    public void Assign_WhenValid_SetsUserAndMovesToToDo()
    {
        var ticket = TicketTask.Create(
            "Task",
            30,
            TaskPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var assignResult = ticket.Assign(5);

        assignResult.IsSuccess.Should().BeTrue();
        ticket.UserId.Should().Be(5);
        ticket.Status.Should().Be(DomainTaskStatus.ToDo);
    }

    [Fact]
    public void ProceedToStep_WhenLastStep_CompletesTicket()
    {
        var ticket = TicketTask.Create(
            "Task",
            30,
            TaskPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var proceedResult = ticket.ProceedToStep(null);

        proceedResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTaskStatus.Done);
    }

    [Fact]
    public void Complete_WhenActive_SetsDoneStatus()
    {
        var ticket = TicketTask.Create(
            "Task",
            30,
            TaskPriority.None,
            1,
            10,
            2,
            false,
            null,
            DateTime.UtcNow).Value;

        var completeResult = ticket.Complete();

        completeResult.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(DomainTaskStatus.Done);
    }
}
