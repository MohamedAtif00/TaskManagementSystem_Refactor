using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidators_PassesThrough()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestRequest("valid"), next, CancellationToken.None);

        response.Should().Be("ok");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_InvokesNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new SuccessValidator()]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestRequest("valid"), next, CancellationToken.None);

        response.Should().Be("ok");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new FailureValidator()]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = () => behavior.Handle(new TestRequest(string.Empty), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name is required.*");
        await next.DidNotReceive().Invoke();
    }

    public sealed record TestRequest(string Name);

    private sealed class SuccessValidator : AbstractValidator<TestRequest>;

    private sealed class FailureValidator : AbstractValidator<TestRequest>
    {
        public FailureValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }
}
