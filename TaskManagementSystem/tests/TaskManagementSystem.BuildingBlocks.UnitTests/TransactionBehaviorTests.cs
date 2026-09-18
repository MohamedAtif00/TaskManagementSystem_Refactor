using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Persistence.Behaviors;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestDoesNotApply_PassesThroughWithoutTransaction()
    {
        await using var context = CreateContext();
        var behavior = new TestTransactionBehavior<PlainRequest, Result<string>>(context, NullLogger.Instance);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Result.Ok("ok"));

        var response = await behavior.Handle(new PlainRequest(), next, CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().Be("ok");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WhenResultFails_RollsBackChanges()
    {
        await using var context = CreateContext();
        var behavior = new TestTransactionBehavior<MarkedCommand, Result<string>>(context, NullLogger.Instance);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(callInfo =>
        {
            context.Add(new TestEntity { Name = "pending" });
            context.SaveChanges();
            return Task.FromResult(Result.Fail<string>(new ResultError("failed", "failed")));
        });

        var response = await behavior.Handle(new MarkedCommand(), next, CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        context.ChangeTracker.Clear();
        context.Set<TestEntity>().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenResultSucceeds_CommitsChanges()
    {
        await using var context = CreateContext();
        var behavior = new TestTransactionBehavior<MarkedCommand, Result<string>>(context, NullLogger.Instance);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(callInfo =>
        {
            context.Add(new TestEntity { Name = "committed" });
            context.SaveChanges();
            return Task.FromResult(Result.Ok("ok"));
        });

        var response = await behavior.Handle(new MarkedCommand(), next, CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        context.ChangeTracker.Clear();
        context.Set<TestEntity>().Single().Name.Should().Be("committed");
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite($"Data Source=transaction-behavior-{Guid.NewGuid():N};Mode=Memory;Cache=Shared")
            .Options;

        var context = new TestDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    private sealed record PlainRequest;

    private sealed record MarkedCommand : ITestCommand;

    private interface ITestCommand;

    private sealed class TestEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<TestEntity>();
    }

    private sealed class TestTransactionBehavior<TRequest, TResponse>(
        TestDbContext context,
        ILogger logger)
        : TransactionBehaviorBase<TRequest, TResponse, TestDbContext>(context, logger)
        where TRequest : notnull
    {
        protected override bool AppliesTo(TRequest request) => request is ITestCommand;
    }
}
