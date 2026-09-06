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

        var academicYear = await db.AcademicYears.FirstOrDefaultAsync(y => y.Name == "2025-2026");
        if (academicYear is null)
        {
            academicYear = new AcademicYear { Name = "2025-2026", Description = "Integration test year" };
            db.AcademicYears.Add(academicYear);
            await db.SaveChangesAsync();
        }

        var curriculumProject = await db.CurriculumProjects.FirstOrDefaultAsync(p => p.Name == "Integration Curriculum Project");
        if (curriculumProject is null)
        {
            curriculumProject = new CurriculumProject
            {
                Name = "Integration Curriculum Project",
                Year = academicYear,
            };
            db.CurriculumProjects.Add(curriculumProject);
            await db.SaveChangesAsync();
        }

        var term = await db.CurriculumTerms.FirstOrDefaultAsync(t => t.Name == "Integration Term");
        if (term is null)
        {
            term = new CurriculumTerm
            {
                Name = "Integration Term",
                Project = curriculumProject,
            };
            db.CurriculumTerms.Add(term);
            await db.SaveChangesAsync();
        }

        var subjectGroup = await db.SubjectGroups.FirstOrDefaultAsync(g => g.Name == "Integration Subject Group");
        if (subjectGroup is null)
        {
            subjectGroup = new SubjectGroup
            {
                Name = "Integration Subject Group",
                Term = term,
            };
            db.SubjectGroups.Add(subjectGroup);
            await db.SaveChangesAsync();
        }

        var subject = await db.Subjects.FirstOrDefaultAsync(p => p.Name == "Integration Test Project");
        if (subject is null)
        {
            subject = new Subject
            {
                Name = "Integration Test Project",
                Description = "Seeded for SubjectController integration tests",
                SubjectGroup = subjectGroup,
                Status = ProjectStatusEnum.Active,
            };
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();
        }

        return new ProjectTestData
        {
            YearId = year.Id,
            FolderId = subjectGroup.Id,
            OwnerUserId = owner.Id,
            MemberUserId = member.Id,
            ProjectId = subject.Id,
        };
    }
}
