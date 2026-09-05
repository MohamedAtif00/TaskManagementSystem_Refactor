using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_WhenComponentsMatch_ReturnsTrue()
    {
        var left = new SampleValue("alpha", 10);
        var right = new SampleValue("alpha", 10);

        left.Should().Be(right);
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WhenComponentsDiffer_ReturnsFalse()
    {
        var left = new SampleValue("alpha", 10);
        var right = new SampleValue("beta", 10);

        left.Should().NotBe(right);
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenComparedToNull_ReturnsFalse()
    {
        var value = new SampleValue("alpha", 10);

        value.Equals(null).Should().BeFalse();
        (value == null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenDifferentRuntimeType_ReturnsFalse()
    {
        var left = new SampleValue("alpha", 10);
        ValueObject right = new OtherSampleValue("alpha", 10);

        left.Equals(right).Should().BeFalse();
    }

    private sealed class SampleValue(string name, int amount) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return name;
            yield return amount;
        }
    }

    private sealed class OtherSampleValue(string name, int amount) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return name;
            yield return amount;
        }
    }
}
