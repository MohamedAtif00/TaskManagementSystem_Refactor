using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenSuccessful_LogsInformationTwice()
    {
        var logger = new CaptureLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("ok");

        var response = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        response.Should().Be("ok");
        logger.Entries.Count(entry => entry.Level == LogLevel.Information).Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_LogsErrorAndRethrows()
    {
        var logger = new CaptureLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var act = () => behavior.Handle(new TestRequest(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        logger.Entries.Count(entry => entry.Level == LogLevel.Error).Should().Be(1);
    }

    private sealed record TestRequest;

    private sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
