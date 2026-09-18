using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.BuildingBlocks.Persistence.Inbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.IntegrationEvents.Ticket;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class InboxGuardTests
{
    [Fact]
    public async Task TryBeginAsync_WhenDuplicateEvent_ReturnsFalse()
    {
        await using var context = CreateContext();
        var serializer = new IntegrationEventSerializer();
        var guard = new EfInboxGuard<InboxTestDbContext>(context, serializer);
        var integrationEvent = new TicketAssignedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            1,
            2);

        var first = await guard.TryBeginAsync(integrationEvent.Id, "TestConsumer", integrationEvent, CancellationToken.None);
        await context.SaveChangesAsync();

        var second = await guard.TryBeginAsync(integrationEvent.Id, "TestConsumer", integrationEvent, CancellationToken.None);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    private static InboxTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InboxTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new InboxTestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private sealed class InboxTestDbContext(DbContextOptions<InboxTestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ConfigureOutboxInbox("test");
    }
}
