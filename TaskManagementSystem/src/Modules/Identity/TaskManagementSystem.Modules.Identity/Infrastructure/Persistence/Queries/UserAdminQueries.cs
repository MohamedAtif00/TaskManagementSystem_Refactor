using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

internal sealed class UserAdminQueries(ISqlConnectionFactory connectionFactory) : IUserAdminQueries
{
    public async Task<IReadOnlyList<UserListItemReadModel>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.[Id],
                u.[Code],
                u.[Name],
                u.[Role] AS RoleId,
                r.[Name] AS RoleName,
                u.[TeamId],
                t.[Name] AS TeamName
            FROM [identity].[Users] AS u
            INNER JOIN [identity].[Roles] AS r ON r.[Id] = u.[Role]
            LEFT JOIN [organization].[Teams] AS t ON t.[Id] = u.[TeamId] AND t.[Archived] = 0
            WHERE u.[Archived] = 0
            ORDER BY u.[Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var users = await connection.QueryAsync<UserListItemReadModel>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return users.ToList();
    }

    public async Task<UserDetailReadModel?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.[Id],
                u.[Code],
                u.[Name],
                u.[HR_code] AS HrCode,
                u.[Email],
                u.[Phone],
                u.[Title],
                u.[Role] AS RoleId,
                r.[Name] AS RoleName,
                u.[AccountType],
                u.[OnBoard],
                u.[TeamId],
                t.[Name] AS TeamName,
                u.[TeamleaderId],
                tl.[Name] AS TeamleaderName
            FROM [identity].[Users] AS u
            INNER JOIN [identity].[Roles] AS r ON r.[Id] = u.[Role]
            LEFT JOIN [organization].[Teams] AS t ON t.[Id] = u.[TeamId] AND t.[Archived] = 0
            LEFT JOIN [identity].[Users] AS tl ON tl.[Id] = u.[TeamleaderId] AND tl.[Archived] = 0
            WHERE u.[Id] = @UserId AND u.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<UserDetailReadModel>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<UserDetailReadModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.[Id],
                u.[Code],
                u.[Name],
                u.[HR_code] AS HrCode,
                u.[Email],
                u.[Phone],
                u.[Title],
                u.[Role] AS RoleId,
                r.[Name] AS RoleName,
                u.[AccountType],
                u.[OnBoard],
                u.[TeamId],
                t.[Name] AS TeamName,
                u.[TeamleaderId],
                tl.[Name] AS TeamleaderName
            FROM [identity].[Users] AS u
            INNER JOIN [identity].[Roles] AS r ON r.[Id] = u.[Role]
            LEFT JOIN [organization].[Teams] AS t ON t.[Id] = u.[TeamId] AND t.[Archived] = 0
            LEFT JOIN [identity].[Users] AS tl ON tl.[Id] = u.[TeamleaderId] AND tl.[Archived] = 0
            WHERE u.[Code] = @Code AND u.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<UserDetailReadModel>(
            new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TeamLeaderReadModel>> ListTeamLeadersAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.[Id],
                u.[Name],
                u.[Role] AS RoleId,
                r.[Name] AS RoleName
            FROM [identity].[Users] AS u
            INNER JOIN [identity].[Roles] AS r ON r.[Id] = u.[Role]
            WHERE u.[Archived] = 0
              AND u.[Role] IN (@TeamLeaderRoleId, @SectionHeadRoleId)
            ORDER BY u.[Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var leaders = await connection.QueryAsync<TeamLeaderReadModel>(
            new CommandDefinition(
                sql,
                new
                {
                    TeamLeaderRoleId = (int)UserRole.TeamLeader,
                    SectionHeadRoleId = (int)UserRole.SectionHead
                },
                cancellationToken: cancellationToken));

        return leaders.ToList();
    }
}
