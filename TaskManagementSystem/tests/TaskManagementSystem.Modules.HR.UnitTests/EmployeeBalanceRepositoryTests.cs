using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class EmployeeBalanceRepositoryTests : IAsyncLifetime
{
    private readonly string _databaseName = $"HrBalanceTests_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;
    private const int UserId = 42;

    public async Task InitializeAsync()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        _connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        const string masterConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

        await using (var connection = new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync($"CREATE DATABASE [{_databaseName}]");
        }

        await using var database = new SqlConnection(_connectionString);
        await database.OpenAsync();
        await database.ExecuteAsync("CREATE SCHEMA [hr];");
        await database.ExecuteAsync(
            """
            CREATE TABLE [hr].[EmployeeBalances]
            (
                [UserId]                  INT NOT NULL PRIMARY KEY,
                [TeamId]                  INT NULL,
                [TeamleaderId]            INT NULL,
                [Role]                    INT NOT NULL,
                [AnnualLeave]             INT NOT NULL,
                [AnnualLeaveMax]          INT NOT NULL,
                [EmergencyLeave]          INT NOT NULL,
                [EmergencyLeaveMax]       INT NOT NULL,
                [SickLeave]               INT NOT NULL,
                [Permission]              INT NOT NULL,
                [PermissionMax]           INT NOT NULL,
                [WorkFromHome]            INT NOT NULL,
                [WorkFromHomeMax]         INT NOT NULL,
                [FromNextBalanceDaysUsed] INT NOT NULL,
                [OldAnnualBalance]        INT NOT NULL,
                [RowVersion]              ROWVERSION NOT NULL
            );
            """);
    }

    public async Task DisposeAsync()
    {
        if (!LocalDbFact.IsLocalDbAvailable || string.IsNullOrEmpty(_connectionString))
        {
            return;
        }

        const string masterConnectionString =
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
    public async Task DeductPermissionAsync_WhenAtMax_ReturnsInsufficientBalance()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        await SeedBalanceAsync(permission: 10, permissionMax: 10);

        await using var context = CreateContext();
        var repository = new EmployeeBalanceRepository(context);

        var result = await repository.DeductPermissionAsync(UserId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(HrErrors.PermissionInsufficientBalance.Code);

        var permission = await ReadPermissionAsync();
        permission.Should().Be(10);
    }

    [Fact]
    public async Task DeductPermissionAsync_WhenBelowMax_Succeeds()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        await SeedBalanceAsync(permission: 9, permissionMax: 10);

        await using var context = CreateContext();
        var repository = new EmployeeBalanceRepository(context);

        var result = await repository.DeductPermissionAsync(UserId);

        result.IsSuccess.Should().BeTrue();

        var permission = await ReadPermissionAsync();
        permission.Should().Be(10);
    }

    [Fact]
    public async Task DeductPermissionAsync_SecondDeductAtLimit_Fails()
    {
        if (!LocalDbFact.IsLocalDbAvailable)
        {
            return;
        }

        await SeedBalanceAsync(permission: 9, permissionMax: 10);

        await using var context = CreateContext();
        var repository = new EmployeeBalanceRepository(context);

        var firstResult = await repository.DeductPermissionAsync(UserId);
        var secondResult = await repository.DeductPermissionAsync(UserId);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeFalse();
        secondResult.Error.Code.Should().Be(HrErrors.PermissionInsufficientBalance.Code);

        var permission = await ReadPermissionAsync();
        permission.Should().Be(10);
    }

    private HrDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HrDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new HrDbContext(options);
    }

    private async Task SeedBalanceAsync(int permission, int permissionMax)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO [hr].[EmployeeBalances]
                ([UserId], [TeamId], [TeamleaderId], [Role],
                 [AnnualLeave], [AnnualLeaveMax], [EmergencyLeave], [EmergencyLeaveMax],
                 [SickLeave], [Permission], [PermissionMax], [WorkFromHome], [WorkFromHomeMax],
                 [FromNextBalanceDaysUsed], [OldAnnualBalance])
            VALUES
                (@UserId, NULL, NULL, 3,
                 0, 30, 0, 5,
                 0, @Permission, @PermissionMax, 0, 5,
                 0, 0)
            """,
            new { UserId, Permission = permission, PermissionMax = permissionMax });
    }

    private async Task<int> ReadPermissionAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(
            "SELECT [Permission] FROM [hr].[EmployeeBalances] WHERE [UserId] = @UserId",
            new { UserId });
    }
}
