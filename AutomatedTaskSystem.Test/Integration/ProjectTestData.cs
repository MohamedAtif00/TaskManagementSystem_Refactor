using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.YearModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutomatedTaskSystem.Test.Integration;

public sealed class ProjectTestData
{
    public int YearId { get; init; }
    public int FolderId { get; init; }
    public int OwnerUserId { get; init; }
    public int MemberUserId { get; init; }
    public int ProjectId { get; init; }

    public static async Task<ProjectTestData> SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        await db.Database.EnsureCreatedAsync();

        var year = await db.Years.FirstOrDefaultAsync(y => y.Number == "2025-2026");
        if (year is null)
        {
            year = new Year { Number = "2025-2026", Active = true };
            db.Years.Add(year);
            await db.SaveChangesAsync();
        }

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Code == IntegrationTestWebAppFactory.TestUserCode);
        if (owner is null)
        {
            owner = new User
            {
                Code = IntegrationTestWebAppFactory.TestUserCode,
                Name = "Integration Test Owner",
                Role = UserRoleEnum.Owner,
                HR_code = "999999",
                AccountType = AccountTypeEnum.Internal,
                Annual_leave_MAX = 30,
                Emergency_leave_MAX = 5,
                Permission_MAX = 10,
                WorkFromHome_MAX = 5,
            };
            db.Users.Add(owner);
            await db.SaveChangesAsync();
        }

        var member = await db.Users.FirstOrDefaultAsync(u => u.Code == "MBR001");
        if (member is null)
        {
            member = new User
            {
                Code = "MBR001",
                Name = "Integration Test Member",
                Role = UserRoleEnum.Member,
                HR_code = "888888",
                AccountType = AccountTypeEnum.Internal,
                Annual_leave_MAX = 30,
                Emergency_leave_MAX = 5,
                Permission_MAX = 10,
                WorkFromHome_MAX = 5,
            };
            db.Users.Add(member);
            await db.SaveChangesAsync();
        }

        var rootFolder = await db.Folders.FirstOrDefaultAsync(f => f.Name == "IntegrationRoot" && f.ParentFolderId == null);
        if (rootFolder is null)
        {
            rootFolder = new Folder { Name = "IntegrationRoot" };
            db.Folders.Add(rootFolder);
            await db.SaveChangesAsync();
        }

        var leafFolder = await db.Folders.FirstOrDefaultAsync(f => f.Name == "IntegrationLeaf" && f.ParentFolderId == rootFolder.Id);
        if (leafFolder is null)
        {
            leafFolder = new Folder { Name = "IntegrationLeaf", ParentFolderId = rootFolder.Id };
            db.Folders.Add(leafFolder);
            await db.SaveChangesAsync();
        }

        var subject = await db.Subjects.FirstOrDefaultAsync(p => p.Name == "Integration Test Project");
        if (subject is null)
        {
            subject = new Subject
            {
                Name = "Integration Test Project",
                Description = "Seeded for SubjectController integration tests",
                FolderId = leafFolder.Id,
                Status = ProjectStatusEnum.Active,
            };
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();
        }

        return new ProjectTestData
        {
            YearId = year.Id,
            FolderId = leafFolder.Id,
            OwnerUserId = owner.Id,
            MemberUserId = member.Id,
            ProjectId = subject.Id,
        };
    }
}
