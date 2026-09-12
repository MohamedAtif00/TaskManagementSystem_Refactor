using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

internal sealed class AboutMeQueries(ISqlConnectionFactory connectionFactory) : IAboutMeQueries
{
    public async Task<AboutMeReadModel?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.[Id],
                u.[Name],
                u.[Role] AS RoleId,
                r.[Name] AS RoleName,
                t.[Name] AS TeamName,
                (
                    SELECT COUNT(*)
                    FROM [notifications].[Notifications] AS n
                    WHERE n.[UserId] = u.[Id] AND n.[IsRead] = 0
                ) AS Notifications,
                (
                    SELECT STRING_AGG(p.[Code], ',') WITHIN GROUP (ORDER BY p.[Code])
                    FROM [identity].[RolePermissions] AS rp
                    INNER JOIN [identity].[Permissions] AS p ON p.[Id] = rp.[PermissionId]
                    WHERE rp.[RoleId] = u.[Role]
                ) AS PermissionCodesCsv
            FROM [identity].[Users] AS u
            INNER JOIN [identity].[Roles] AS r ON r.[Id] = u.[Role]
            LEFT JOIN [organization].[Teams] AS t ON t.[Id] = u.[TeamId] AND t.[Archived] = 0
            WHERE u.[Id] = @UserId AND u.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var row = await connection.QuerySingleOrDefaultAsync<AboutMeRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        var permissions = string.IsNullOrWhiteSpace(row.PermissionCodesCsv)
            ? []
            : row.PermissionCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new AboutMeReadModel(
            row.Id,
            row.Name,
            row.RoleId,
            row.RoleName,
            permissions,
            row.TeamName,
            row.Notifications);
    }

    private sealed class AboutMeRow
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int RoleId { get; init; }

        public string RoleName { get; init; } = string.Empty;

        public string? TeamName { get; init; }

        public int Notifications { get; init; }

        public string? PermissionCodesCsv { get; init; }
    }
}
