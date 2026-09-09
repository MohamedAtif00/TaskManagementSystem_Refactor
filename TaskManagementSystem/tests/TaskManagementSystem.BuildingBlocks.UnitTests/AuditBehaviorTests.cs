using FluentAssertions;
using MediatR;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class AuditBehaviorTests
{
    [Fact]
    public async Task Handle_WhenCommandSucceeds_AppendsSuccessfulAuditEntry()
    {
        var auditContext = Substitute.For<IAuditContext>();
        auditContext.UserId.Returns(42);
        auditContext.CorrelationId.Returns("trace-123");

        var auditStore = Substitute.For<IAuditStore>();
        AuditEntry? capturedEntry = null;
        auditStore
            .AppendAsync(Arg.Do<AuditEntry>(entry => capturedEntry = entry), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var timeProvider = TimeProvider.System;
        var behavior = new AuditBehavior<TestCommand, string>(auditContext, auditStore, timeProvider);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        response.Should().Be("ok");
        capturedEntry.Should().NotBeNull();
        capturedEntry!.ActionName.Should().Be(nameof(TestCommand));
        capturedEntry.UserId.Should().Be(42);
        capturedEntry.CorrelationId.Should().Be("trace-123");
        capturedEntry.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCommandFails_AppendsFailedAuditEntryAndRethrows()
    {
        var auditContext = Substitute.For<IAuditContext>();
        auditContext.CorrelationId.Returns("trace-456");

        var auditStore = Substitute.For<IAuditStore>();
        AuditEntry? capturedEntry = null;
        auditStore
            .AppendAsync(Arg.Do<AuditEntry>(entry => capturedEntry = entry), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var behavior = new AuditBehavior<TestCommand, string>(
            auditContext,
            auditStore,
            TimeProvider.System);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var act = () => behavior.Handle(new TestCommand(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        capturedEntry.ActionName.Should().Be(nameof(TestCommand));
    }

    [Fact]
    public async Task Handle_WhenQuery_DoesNotAppendAuditEntry()
    {
        var auditContext = Substitute.For<IAuditContext>();
        var auditStore = Substitute.For<IAuditStore>();
        var behavior = new AuditBehavior<TestQuery, string>(
            auditContext,
            auditStore,
            TimeProvider.System);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestQuery(), next, CancellationToken.None);

        response.Should().Be("ok");
        await auditStore.DidNotReceiveWithAnyArgs().AppendAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenCommandReturnsFailedResult_AppendsFailedAuditEntry()
    {
        var auditContext = Substitute.For<IAuditContext>();
        auditContext.CorrelationId.Returns("trace-789");

        var auditStore = Substitute.For<IAuditStore>();
        AuditEntry? capturedEntry = null;
        auditStore
            .AppendAsync(Arg.Do<AuditEntry>(entry => capturedEntry = entry), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var behavior = new AuditBehavior<TestCommand, Result<string>>(
            auditContext,
            auditStore,
            TimeProvider.System);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Result.Fail<string>(new ResultError("bad", "bad")));

        var response = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        response.IsSuccess.Should().BeFalse();
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
    }

    private sealed record TestCommand : ICommand<string>;

    private sealed record TestQuery : IQuery<string>;
}
