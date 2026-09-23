using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class TicketBankLookupQueries(ISqlConnectionFactory connectionFactory)
    : ITicketBankLookup
{
    public async Task<TicketBankSummary?> GetActiveByIdAsync(
        int taskBankId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [Name], [Duration], [TeamId], [TL] AS TeamLeaderOnly, [Type]
            FROM [workflows].[TicketBank]
            WHERE [Id] = @TicketBankId AND [Active] = 1
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<TicketBankSummary>(
            new CommandDefinition(sql, new { TicketBankId = taskBankId }, cancellationToken: cancellationToken));
    }
}
