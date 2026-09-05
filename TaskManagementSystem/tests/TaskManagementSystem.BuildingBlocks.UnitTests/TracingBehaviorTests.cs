using FluentAssertions;
using MediatR;
using NSubstitute;
using System.Diagnostics;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Observability;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class TracingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenSuccessful_StartsActivityWithRequestTags()
    {
        _ = Telemetry.Mediator;
        using var listener = CreateActivityListener();

        var behavior = new TracingBehavior<TestCommand, string>();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        response.Should().Be("ok");
        listener.StartedActivities.Should().ContainSingle();
        listener.StartedActivities[0].OperationName.Should().Be(nameof(TestCommand));
        listener.StartedActivities[0].Tags.Should().Contain(
            tag => tag.Key == "mediatr.kind" && tag.Value as string == "command");
        listener.StartedActivities[0].Tags.Should().Contain(
            tag => tag.Key == "mediatr.module" && tag.Value as string == "Unknown");
        listener.StartedActivities[0].Status.Should().Be(ActivityStatusCode.Unset);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_SetsErrorStatusAndRethrows()
    {
        _ = Telemetry.Mediator;
        using var listener = CreateActivityListener();

        var behavior = new TracingBehavior<TestQuery, string>();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var act = () => behavior.Handle(new TestQuery(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        listener.StartedActivities.Should().ContainSingle();
        listener.StartedActivities[0].Status.Should().Be(ActivityStatusCode.Error);
        listener.StartedActivities[0].Tags.Should().Contain(
            tag => tag.Key == "mediatr.kind" && tag.Value as string == "query");
    }

    private static TestActivityListener CreateActivityListener() =>
        new(
            sourceName => sourceName.StartsWith("TaskManagementSystem.Mediator", StringComparison.Ordinal),
            _ => ActivitySamplingResult.AllDataAndRecorded);

    private sealed record TestCommand : ICommand<string>;

    private sealed record TestQuery : IQuery<string>;

    private sealed class TestActivityListener : IDisposable
    {
        private readonly ActivityListener _listener;

        public TestActivityListener(
            Func<string, bool> shouldListen,
            Func<string, ActivitySamplingResult> sample)
        {
            StartedActivities = [];
            _listener = new ActivityListener
            {
                ShouldListenTo = source => shouldListen(source.Name),
                Sample = (ref ActivityCreationOptions<ActivityContext> options) =>
                    sample(options.Source.Name),
                ActivityStarted = activity => StartedActivities.Add(activity)
            };

            ActivitySource.AddActivityListener(_listener);
        }

        public List<Activity> StartedActivities { get; }

        public void Dispose() => _listener.Dispose();
    }
}
