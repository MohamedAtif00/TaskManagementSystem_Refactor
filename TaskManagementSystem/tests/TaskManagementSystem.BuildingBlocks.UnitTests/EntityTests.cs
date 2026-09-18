using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class EntityTests
{
    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        var entity = new SampleEntity(1);
        var domainEvent = new SampleDomainEvent();

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var entity = new SampleEntity(1);
        entity.Raise(new SampleDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Equals_WhenSameIdentity_ReturnsTrue()
    {
        var left = new SampleEntity(1);
        var right = new SampleEntity(1);

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WhenDifferentIdentity_ReturnsFalse()
    {
        var left = new SampleEntity(1);
        var right = new SampleEntity(2);

        left.Should().NotBe(right);
    }

    [Fact]
    public void Equals_WhenDifferentType_ReturnsFalse()
    {
        var entity = new SampleEntity(1);
        var otherEntity = new OtherSampleEntity(1);

        entity.Equals(otherEntity).Should().BeFalse();
    }

    private sealed class SampleEntity(int id) : Entity
    {
        public int Id { get; } = id;

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Id;
        }
    }

    private sealed class OtherSampleEntity(int id) : Entity
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return id;
        }
    }

    private sealed record SampleDomainEvent : DomainEventBase;
}
