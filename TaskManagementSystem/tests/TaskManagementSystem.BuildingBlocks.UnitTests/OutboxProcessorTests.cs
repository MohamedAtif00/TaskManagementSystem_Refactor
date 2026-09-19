using System.Data;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Events;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.IntegrationEvents.Ticket;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class OutboxProcessorTests : IAsyncLifetime
{
    private const string Schema = "ticket";
    private readonly string _databaseName = $"OutboxProcessorTests_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        _connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        var masterConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

        await using (var connection = new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync($"CREATE DATABASE [{_databaseName}]");
        }

        await using var database = new SqlConnection(_connectionString);
        await database.OpenAsync();
        await database.ExecuteAsync($"CREATE SCHEMA [{Schema}]");
        await database.ExecuteAsync(
            """
            CREATE TABLE [ticket].[OutboxMessages] (
                [Id]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [OccurredOnUtc]  DATETIME2(7)     NOT NULL,
                [Type]           NVARCHAR(512)    NOT NULL,
                [Payload]        NVARCHAR(MAX)    NOT NULL,
                [ProcessedOnUtc] DATETIME2(7)     NULL,
                [Error]          NVARCHAR(4000)   NULL,
                [Attempts]       INT              NOT NULL DEFAULT (0)
            );
            """);
    }

    public async Task DisposeAsync()
    {
        if (!LocalDbFact.IsLocalDbAvailable || string.IsNullOrEmpty(_connectionString))
        {
            return;
        }

        var masterConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync($"""
            IF DB_ID(N'{_databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_databaseName}];
            END
            """);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishSucceeds_StampProcessedOnUtc()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }
        var integrationEvent = new TicketAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, 1, 2);
        var serializer = new IntegrationEventSerializer();
        var messageId = Guid.NewGuid();

        await InsertOutboxMessageAsync(messageId, integrationEvent, serializer);

        var bus = Substitute.For<IIntegrationEventBus>();
        var pump = CreatePump(bus);

        var processed = await pump.ProcessBatchAsync(Schema);

        processed.Should().BeTrue();
        await bus.Received(1).PublishAsync(integrationEvent, Arg.Any<CancellationToken>());

        await using var connection = new SqlConnection(_connectionString);
        var row = await connection.QuerySingleAsync<(DateTime? ProcessedOnUtc, int Attempts)>(
            "SELECT [ProcessedOnUtc], [Attempts] FROM [ticket].[OutboxMessages] WHERE [Id] = @Id",
            new { Id = messageId });

        row.ProcessedOnUtc.Should().NotBeNull();
        row.Attempts.Should().Be(0);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishFails_RecordsErrorAndIncrementsAttempts()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }
        var integrationEvent = new TicketAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, 3, 4);
        var serializer = new IntegrationEventSerializer();
        var messageId = Guid.NewGuid();

        await InsertOutboxMessageAsync(messageId, integrationEvent, serializer);

        var bus = Substitute.For<IIntegrationEventBus>();
        bus.PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("publish failed"));

        var pump = CreatePump(bus);

        var processed = await pump.ProcessBatchAsync(Schema);

        processed.Should().BeTrue();

        await using var connection = new SqlConnection(_connectionString);
        var row = await connection.QuerySingleAsync<(DateTime? ProcessedOnUtc, string? Error, int Attempts)>(
            "SELECT [ProcessedOnUtc], [Error], [Attempts] FROM [ticket].[OutboxMessages] WHERE [Id] = @Id",
            new { Id = messageId });

        row.ProcessedOnUtc.Should().BeNull();
        row.Error.Should().Be("publish failed");
        row.Attempts.Should().Be(1);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenAttemptsReachedMax_SkipsMessage()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        var integrationEvent = new TicketAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, 5, 6);
        var serializer = new IntegrationEventSerializer();
        var messageId = Guid.NewGuid();
        const int maxAttempts = 5;

        await InsertOutboxMessageAsync(messageId, integrationEvent, serializer, attempts: maxAttempts);

        var bus = Substitute.For<IIntegrationEventBus>();
        var pump = CreatePump(bus, new OutboxOptions { MaxAttempts = maxAttempts });

        var processed = await pump.ProcessBatchAsync(Schema);

        processed.Should().BeFalse();
        await bus.DidNotReceive().PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());

        await using var connection = new SqlConnection(_connectionString);
        var row = await connection.QuerySingleAsync<(DateTime? ProcessedOnUtc, int Attempts)>(
            "SELECT [ProcessedOnUtc], [Attempts] FROM [ticket].[OutboxMessages] WHERE [Id] = @Id",
            new { Id = messageId });

        row.ProcessedOnUtc.Should().BeNull();
        row.Attempts.Should().Be(maxAttempts);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishRepeatedlyFails_StopsAtMaxAttempts()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        var integrationEvent = new TicketAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, 7, 8);
        var serializer = new IntegrationEventSerializer();
        var messageId = Guid.NewGuid();
        const int maxAttempts = 3;

        await InsertOutboxMessageAsync(messageId, integrationEvent, serializer);

        var bus = Substitute.For<IIntegrationEventBus>();
        bus.PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("publish failed"));

        var pump = CreatePump(bus, new OutboxOptions { MaxAttempts = maxAttempts });

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var processed = await pump.ProcessBatchAsync(Schema);
            processed.Should().BeTrue();
        }

        var skipped = await pump.ProcessBatchAsync(Schema);
        skipped.Should().BeFalse();

        await bus.Received(maxAttempts).PublishAsync(integrationEvent, Arg.Any<CancellationToken>());

        await using var connection = new SqlConnection(_connectionString);
        var row = await connection.QuerySingleAsync<(DateTime? ProcessedOnUtc, string? Error, int Attempts)>(
            "SELECT [ProcessedOnUtc], [Error], [Attempts] FROM [ticket].[OutboxMessages] WHERE [Id] = @Id",
            new { Id = messageId });

        row.ProcessedOnUtc.Should().BeNull();
        row.Error.Should().Be("publish failed");
        row.Attempts.Should().Be(maxAttempts);
    }

    private OutboxPump CreatePump(IIntegrationEventBus bus, OutboxOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(bus);
        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        return new OutboxPump(
            new TestSqlConnectionFactory(_connectionString),
            scopeFactory,
            new IntegrationEventSerializer(),
            options ?? new OutboxOptions(),
            NullLogger<OutboxPump>.Instance);
    }

    private async Task InsertOutboxMessageAsync(
        Guid messageId,
        TicketAssignedIntegrationEvent integrationEvent,
        IntegrationEventSerializer serializer,
        int attempts = 0)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO [ticket].[OutboxMessages]
                ([Id], [OccurredOnUtc], [Type], [Payload], [ProcessedOnUtc], [Error], [Attempts])
            VALUES
                (@Id, SYSUTCDATETIME(), @Type, @Payload, NULL, NULL, @Attempts)
            """,
            new
            {
                Id = messageId,
                Type = integrationEvent.GetType().AssemblyQualifiedName,
                Payload = serializer.Serialize(integrationEvent),
                Attempts = attempts
            });
    }

    private sealed class TestSqlConnectionFactory(string connectionString) : ISqlConnectionFactory
    {
        public IDbConnection GetOpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public string GetConnectionString() => connectionString;
    }
}
