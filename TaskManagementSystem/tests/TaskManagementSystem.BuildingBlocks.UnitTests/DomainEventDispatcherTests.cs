using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_PublishesAndClearsDomainEvents()
    {
        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);
        await using var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var entity = new EventEntity();
        entity.Raise(new TestDomainEvent());
        context.Add(entity);

        await dispatcher.DispatchAsync(context, CancellationToken.None);

        await publisher.Received(1).Publish(
            Arg.Any<TestDomainEvent>(),
            Arg.Any<CancellationToken>());
        entity.DomainEvents.Should().BeEmpty();
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<EventEntity>();
    }

    private sealed class EventEntity : Entity
    {
        public int Id { get; set; }

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Id;
        }
    }

    private sealed record TestDomainEvent : DomainEventBase;
}
