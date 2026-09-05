using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.ReadModels;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Testing;

public static class IdentityTestDataSeeder
{
    public const string TestUserCode = "TST001";
    public const string TestTeamName = "Integration Test Team";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Users.AnyAsync(user => user.Code == TestUserCode, cancellationToken))
        {
            return;
        }

        var team = await db.Teams.FirstOrDefaultAsync(t => t.Name == TestTeamName, cancellationToken);
        if (team is null)
        {
            team = new TeamReadModel
            {
                Name = TestTeamName,
                Archived = false
            };
            db.Teams.Add(team);
            await db.SaveChangesAsync(cancellationToken);
        }

        var user = User.CreateForPersistence();
        user.Code = TestUserCode;
        user.Name = "Integration Test User";
        user.HrCode = "999999";
        user.Role = UserRole.Owner;
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

        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);
    }
}
