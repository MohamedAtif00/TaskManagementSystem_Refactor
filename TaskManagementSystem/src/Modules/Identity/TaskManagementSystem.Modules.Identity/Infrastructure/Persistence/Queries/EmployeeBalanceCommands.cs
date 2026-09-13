using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

public sealed class EmployeeBalanceCommands(ISqlConnectionFactory connectionFactory)
{
    public async Task InsertForUserAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [hr].[EmployeeBalances]
            (
                [UserId],
                [TeamId],
                [TeamleaderId],
                [Role],
                [AnnualLeave],
                [AnnualLeaveMax],
                [EmergencyLeave],
                [EmergencyLeaveMax],
                [SickLeave],
                [Permission],
                [PermissionMax],
                [WorkFromHome],
                [WorkFromHomeMax],
                [FromNextBalanceDaysUsed],
                [OldAnnualBalance]
            )
            VALUES
            (
                @UserId,
                @TeamId,
                @TeamleaderId,
                @RoleId,
                @AnnualLeave,
                @AnnualLeaveMax,
                @EmergencyLeave,
                @EmergencyLeaveMax,
                @SickLeave,
                @PermissionBalance,
                @PermissionMax,
                @WorkFromHome,
                @WorkFromHomeMax,
                @FromNextBalanceDaysUsed,
                @OldAnnualBalance
            )
            """;

        using var connection = connectionFactory.GetOpenConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    user.Id,
                    user.TeamId,
                    user.TeamleaderId,
                    user.RoleId,
                    user.AnnualLeave,
                    user.AnnualLeaveMax,
                    user.EmergencyLeave,
                    user.EmergencyLeaveMax,
                    user.SickLeave,
                    user.PermissionBalance,
                    user.PermissionMax,
                    user.WorkFromHome,
                    user.WorkFromHomeMax,
                    user.FromNextBalanceDaysUsed,
                    user.OldAnnualBalance
                },
                cancellationToken: cancellationToken));
    }

    public async Task SyncMetadataAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [hr].[EmployeeBalances]
            SET
                [TeamId] = @TeamId,
                [TeamleaderId] = @TeamleaderId,
                [Role] = @RoleId
            WHERE [UserId] = @UserId
            """;

        using var connection = connectionFactory.GetOpenConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    user.Id,
                    user.TeamId,
                    user.TeamleaderId,
                    user.RoleId
                },
                cancellationToken: cancellationToken));
    }
}
