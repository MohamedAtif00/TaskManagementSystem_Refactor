using FluentAssertions;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Ticket.UnitTests;

public sealed class TicketActivityTextTests
{
    [Fact]
    public void Describe_UsesOldTypeNumbersAndSentences()
    {
        ((int)TicketActivityType.Created).Should().Be(1);
        ((int)TicketActivityType.StatusToDo).Should().Be(2);
        ((int)TicketActivityType.Assign).Should().Be(10);
        ((int)TicketActivityType.PriorityChange).Should().Be(13);
        ((int)TicketActivityType.Skip).Should().Be(14);
        ((int)TicketActivityType.Reactivated).Should().Be(18);

        TicketActivityText.Describe(TicketActivityType.Created, "Lesson", null, null, null, null)
            .Should().Be("Lesson was created.");
        TicketActivityText.Describe(TicketActivityType.Created, "Lesson", "Ada", null, null, null)
            .Should().Be("Lesson was created by Ada.");
        TicketActivityText.Describe(TicketActivityType.StatusToDo, "Lesson", "Ada", null, null, null)
            .Should().Be("Ada added task to their To Do list.");
        TicketActivityText.Describe(TicketActivityType.Assign, "Lesson", "Ada", "Ben", null, null)
            .Should().Be("Ada assigned the task to Ben.");
        TicketActivityText.Describe(TicketActivityType.PriorityChange, "Lesson", "Ada", null, null, "High")
            .Should().Be("Ada updated task priority to High.");
        TicketActivityText.Describe(TicketActivityType.Flag, "Lesson", "Ada", "Ben", null, "Check this")
            .Should().Be("Ada flagged the task for Ben. \"Check this\"");
        TicketActivityText.Describe(TicketActivityType.Reactivated, "Lesson", null, null, null, null)
            .Should().Be("Task was reactivated due to a previous task completion.");
    }
}
