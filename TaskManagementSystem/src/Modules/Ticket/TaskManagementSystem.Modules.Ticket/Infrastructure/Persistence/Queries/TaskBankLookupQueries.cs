using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class TaskBankLookupQueries(ISqlConnectionFactory connectionFactory)
    : ITaskBankLookup
{
    public async Task<TaskBankSummary?> GetActiveByIdAsync(
        int taskBankId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [Name], [Duration], [TeamId], [TL] AS TeamLeaderOnly
            FROM [workflows].[TaskBank]
            WHERE [Id] = @TaskBankId AND [Active] = 1
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<TaskBankSummary>(
            new CommandDefinition(sql, new { TaskBankId = taskBankId }, cancellationToken: cancellationToken));
    }
}
