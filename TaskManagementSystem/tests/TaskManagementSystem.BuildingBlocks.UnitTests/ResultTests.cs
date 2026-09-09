using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Ok_SetsSuccessAndValue()
    {
        var result = Result<int>.Ok(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Fail_SetsFailureAndError()
    {
        var error = new ResultError("test_code", "test message");
        var result = Result<int>.Fail(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_WhenFailure_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Fail("test_code", "test message");

        var act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Value when result is a failure*");
    }

    [Fact]
    public void Error_WhenSuccess_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Ok(42);

        var act = () => _ = result.Error;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Error when result is successful*");
    }

    [Fact]
    public void Default_IsFailureAndUninitialized()
    {
        var result = default(Result<int>);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.TryGetValue(out _).Should().BeFalse();
        result.TryGetError(out _).Should().BeFalse();

        var act = () => _ = result.Error;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Result is uninitialized*");
    }

    [Fact]
    public void StaticOk_FactoryCreatesSuccess()
    {
        var result = Result.Ok(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void StaticOkWithoutValue_CreatesNoValueSuccess()
    {
        var result = Result.Ok();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void StaticFail_FactoryCreatesFailure()
    {
        var error = new ResultError("test_code", "test message");
        var result = Result.Fail<int>(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromResultError_CreatesFailure()
    {
        var error = new ResultError("test_code", "test message");
        Result<int> result = error;

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void TryGetValue_WhenSuccess_ReturnsValue()
    {
        var result = Result<int>.Ok(42);

        var found = result.TryGetValue(out var value);

        found.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetError_WhenFailure_ReturnsError()
    {
        var error = new ResultError("test_code", "test message");
        var result = Result<int>.Fail(error);

        var found = result.TryGetError(out var actualError);

        found.Should().BeTrue();
        actualError.Should().Be(error);
    }

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        var result = Result<int>.Ok(2).Map(value => value * 3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public void Map_WhenFailure_PreservesError()
    {
        var error = new ResultError("test_code", "test message");
        var result = Result<int>.Fail(error).Map(value => value * 3);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_WhenSuccess_ChainsResults()
    {
        var result = Result<int>.Ok(2).Bind(value =>
            value > 0 ? Result<string>.Ok(value.ToString()) : Result<string>.Fail("bad", "bad"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("2");
    }

    [Fact]
    public void Bind_WhenFailure_ShortCircuits()
    {
        var error = new ResultError("test_code", "test message");
        var result = Result<int>.Fail(error).Bind(value => Result<string>.Ok(value.ToString()));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Match_RoutesToCorrectBranch()
    {
        var success = Result<int>.Ok(7).Match(
            value => $"ok:{value}",
            error => $"fail:{error.Code}");

        var failure = Result<int>.Fail("bad", "bad").Match(
            value => $"ok:{value}",
            error => $"fail:{error.Code}");

        success.Should().Be("ok:7");
        failure.Should().Be("fail:bad");
    }

    [Fact]
    public void Equals_WhenSameSuccess_AreEqual()
    {
        var left = Result<int>.Ok(42);
        var right = Result<int>.Ok(42);

        left.Should().Be(right);
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenSameFailure_AreEqual()
    {
        var error = new ResultError("test_code", "test message");
        var left = Result<int>.Fail(error);
        var right = Result<int>.Fail(error);

        left.Should().Be(right);
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void ImplementsIResult()
    {
        IResult success = Result<int>.Ok(42);
        IResult failure = Result<int>.Fail("bad", "bad");

        success.IsSuccess.Should().BeTrue();
        success.IsFailure.Should().BeFalse();
        failure.IsSuccess.Should().BeFalse();
        failure.IsFailure.Should().BeTrue();
    }
}
