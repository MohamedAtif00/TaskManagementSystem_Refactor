using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class DomainEventBaseTests
{
    [Fact]
    public void Constructor_SetsIdAndOccurredOn()
    {
        var domainEvent = new SampleDomainEvent();

        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private sealed record SampleDomainEvent : DomainEventBase;
}
