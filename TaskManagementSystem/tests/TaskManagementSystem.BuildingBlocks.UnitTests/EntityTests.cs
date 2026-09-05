using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.TestCommon.MediatR;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class EntityTests
{
    [Fact]
    public void CheckRule_WhenRuleIsBroken_ThrowsBusinessRuleValidationException()
    {
        var rule = new AlwaysBrokenRule();

        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            SampleEntity.Create(rule));

        Assert.Same(rule, exception.BrokenRule);
    }

    [Fact]
    public void CheckRule_WhenRuleIsValid_DoesNotThrow()
    {
        var entity = SampleEntity.Create(new NeverBrokenRule());

        Assert.NotNull(entity);
    }

    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        var entity = SampleEntity.Create(new NeverBrokenRule());
        var domainEvent = new SampleDomainEvent();

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var entity = SampleEntity.Create(new NeverBrokenRule());
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

    private sealed class SampleEntity : Entity
    {
        public SampleEntity(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public static SampleEntity Create(IBusinessRule rule)
        {
            CheckRule(rule);
            return new SampleEntity(1);
        }

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

    private sealed class AlwaysBrokenRule : IBusinessRule
    {
        public bool IsBroken() => true;

        public string Message => "Rule is broken.";
    }

    private sealed class NeverBrokenRule : IBusinessRule
    {
        public bool IsBroken() => false;

        public string Message => string.Empty;
    }
}
