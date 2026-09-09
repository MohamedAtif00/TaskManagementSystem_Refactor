using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Database;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.ReadModels;

namespace TaskManagementSystem.TestCommon.Integration;

public static class IntegrationTestDataSeeder
{
    public const string TestUserCode = "TST001";
    public const string TestTeamName = "Integration Test Team";
    public const string TestUserName = "Integration Test User";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var hrDb = scope.ServiceProvider.GetRequiredService<HrDbContext>();

        await identityDb.Database.EnsureCreatedAsync(cancellationToken);
        await hrDb.Database.EnsureCreatedAsync(cancellationToken);

        if (identityDb.Database.IsInMemory())
        {
            await SeedInMemoryAsync(scope.ServiceProvider, cancellationToken);
            return;
        }

        var connectionString = identityDb.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Connection string is required for SQL script seeding.");

        var scriptPath = SqlScriptSeeder.ResolveScriptPath(SqlScriptSeeder.IdentityIntegrationTestSeedScript);
        await SqlScriptSeeder.ExecuteFileAsync(connectionString, scriptPath, cancellationToken);
    }

    private static async Task SeedInMemoryAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        // Mirrors Scripts/Migrations/011_identity_SeedUsers.sql (EF InMemory cannot execute T-SQL).
        using var scope = services.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var hrDb = scope.ServiceProvider.GetRequiredService<HrDbContext>();

        await RbacSeedData.SeedAsync(identityDb, cancellationToken);

        if (await identityDb.Users.AnyAsync(user => user.Code == TestUserCode, cancellationToken))
        {
            await SyncHrEmployeeBalancesAsync(identityDb, hrDb, cancellationToken);
            return;
        }

        var team = await identityDb.Teams.FirstOrDefaultAsync(
            t => t.Name == TestTeamName,
            cancellationToken);

        if (team is null)
        {
            team = new TeamReadModel
            {
                Name = TestTeamName,
                Archived = false
            };
            identityDb.Teams.Add(team);
            await identityDb.SaveChangesAsync(cancellationToken);
        }

        var user = User.CreateForPersistence();
        user.Code = TestUserCode;
        user.Name = TestUserName;
        user.HrCode = "999999";
        user.RoleId = (int)UserRole.Owner;
        user.AccountType = AccountType.Internal;
        user.OnBoard = false;
        user.Archived = false;
        user.TeamId = team.Id;
        user.AnnualLeave = 0;
        user.AnnualLeaveMax = 30;
        user.EmergencyLeave = 0;
        user.EmergencyLeaveMax = 5;
        user.SickLeave = 0;
        user.Permission = 0;
        user.PermissionMax = 10;
        user.WorkFromHome = 0;
        user.WorkFromHomeMax = 5;
        user.FromNextBalanceDaysUsed = 0;
        user.OldAnnualBalance = 0;

        identityDb.Users.Add(user);
        await identityDb.SaveChangesAsync(cancellationToken);

        await SyncHrEmployeeBalancesAsync(identityDb, hrDb, cancellationToken);
    }

    private static async Task SyncHrEmployeeBalancesAsync(
        IdentityDbContext identityDb,
        HrDbContext hrDb,
        CancellationToken cancellationToken)
    {
        // InMemory-only: HR maps identity.Users via a separate entity type.
        var users = await identityDb.Users.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var existing = await hrDb.EmployeeBalances
                .FirstOrDefaultAsync(balance => balance.Id == user.Id, cancellationToken);

            if (existing is null)
            {
                hrDb.EmployeeBalances.Add(new EmployeeBalanceEntity
                {
                    Id = user.Id,
                    TeamId = user.TeamId,
                    Role = user.RoleId,
                    TeamleaderId = user.TeamleaderId,
                    AnnualLeave = user.AnnualLeave,
                    AnnualLeaveMax = user.AnnualLeaveMax,
                    EmergencyLeave = user.EmergencyLeave,
                    EmergencyLeaveMax = user.EmergencyLeaveMax,
                    SickLeave = user.SickLeave,
                    Permission = user.Permission,
                    PermissionMax = user.PermissionMax,
                    WorkFromHome = user.WorkFromHome,
                    WorkFromHomeMax = user.WorkFromHomeMax,
                    FromNextBalanceDaysUsed = user.FromNextBalanceDaysUsed,
                    OldAnnualBalance = user.OldAnnualBalance
                });
            }
            else
            {
                existing.TeamleaderId = user.TeamleaderId;
                existing.TeamId = user.TeamId;
                existing.Role = user.RoleId;
                existing.AnnualLeave = user.AnnualLeave;
                existing.AnnualLeaveMax = user.AnnualLeaveMax;
                existing.EmergencyLeave = user.EmergencyLeave;
                existing.EmergencyLeaveMax = user.EmergencyLeaveMax;
                existing.SickLeave = user.SickLeave;
                existing.Permission = user.Permission;
                existing.PermissionMax = user.PermissionMax;
                existing.WorkFromHome = user.WorkFromHome;
                existing.WorkFromHomeMax = user.WorkFromHomeMax;
                existing.FromNextBalanceDaysUsed = user.FromNextBalanceDaysUsed;
                existing.OldAnnualBalance = user.OldAnnualBalance;
            }
        }

        await hrDb.SaveChangesAsync(cancellationToken);
    }
}
