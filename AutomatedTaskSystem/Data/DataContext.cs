using System.Collections.Generic;
using System.Xml;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.SchemaTypesModel;
using AutomatedTaskSystem.Models.YearModel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutomatedTaskSystem.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Node>().Property(n => n.Archived).HasDefaultValue(false);

        modelBuilder.Entity<RefreshToken>().Property(rt => rt.Used).HasDefaultValue(false);

        modelBuilder.Entity<Sprint>().Property(s => s.IsArchived).HasDefaultValue(false);

        modelBuilder
            .Entity<Node>()
            .HasMany(n => n.Next)
            .WithMany(n => n.Previous)
            .UsingEntity(j => j.ToTable("NodeSequences"));

        // *** Start of Foreign Key Cycle Fixes ***

        // 1. SectionGroup to Section (as identified in the error message)
        modelBuilder.Entity<SectionGroup>()
            .HasOne(sg => sg.Section)
            .WithMany(s => s.SectionGroups)
            .HasForeignKey(sg => sg.SectionId)
            .OnDelete(DeleteBehavior.NoAction);

        // 2. SectionGroup to Group (as per your Group model having sectionGroups)
        modelBuilder.Entity<SectionGroup>()
            .HasOne(sg => sg.Group)
            .WithMany(g => g.sectionGroups)
            .HasForeignKey(sg => sg.GroupId)
            .OnDelete(DeleteBehavior.NoAction); 

        // 3. Section to User (Head) - Assuming HeadId is nullable, SetNull is fine.
        // If HeadId is non-nullable, it *must* be NoAction.
        modelBuilder.Entity<Section>()
            .HasOne(s => s.Head)
            .WithMany() 
            .HasForeignKey(s => s.HeadId)
            .IsRequired(false) 
            .OnDelete(DeleteBehavior.NoAction); 

        // 4. *** CRITICAL NEW ADDITION: User to Group relationship ***
        // This is the most likely missing piece, as Group.Users suggests User has a GroupId.
        // Assuming 'User.GroupId' is a non-nullable 'int'. If it's nullable ('int?'), use .IsRequired(false) and .OnDelete(DeleteBehavior.SetNull).
        modelBuilder.Entity<User>()
            .HasOne(u => u.Group)         // User has one Group
            .WithMany(g => g.Users)       // Group has many Users (from your Group model)
            .HasForeignKey(u => u.GroupId) // The foreign key property in User model
                                           // .IsRequired(true) // Implicitly true if GroupId is 'int' and not nullable. No need to specify if it's 'int'.
            .OnDelete(DeleteBehavior.NoAction); // *** Set to NoAction to break cycles for a required relationship ***

        // *** End of Foreign Key Cycle Fixes ***

        modelBuilder
            .Entity<Step>()
            .HasMany(n => n.Rollbacks)
            .WithMany(n => n.From)
            .UsingEntity(j => j.ToTable("RSteps"));

        modelBuilder.Entity<Comment>().HasOne(c => c.Child).WithOne();

        modelBuilder
            .Entity<LearningObjective>()
            .HasOne(lo => lo.Schema)
            .WithMany(s => s.LearningObjectives);

        modelBuilder.Entity<DailyReportNoteOverride>()
            .HasIndex(o => o.TaskId)
            .IsUnique();

        modelBuilder.Entity<DailyReportNoteOverride>()
            .HasOne(o => o.Task)
            .WithMany()
            .HasForeignKey(o => o.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Daily Report hot-path indexes
        modelBuilder.Entity<TaskActivity>()
            .HasIndex(a => new { a.Type, a.TimeStamp })
            .IncludeProperties(a => a.TaskId)
            .HasDatabaseName("IX_TaskActivities_Type_TimeStamp_TaskId");

        modelBuilder.Entity<Models.Task>()
            .HasIndex(t => new { t.Archived, t.GroupId, t.CreatedAt })
            .IncludeProperties(t => new
            {
                t.LearningObjectiveId,
                t.StepId,
                t.UserId,
                t.Status,
                t.IsRollback,
                t.Priority,
                t.Name
            })
            .HasDatabaseName("IX_Tasks_Archived_GroupId_CreatedAt");

        modelBuilder.Entity<Rollback>()
            .HasIndex(r => new { r.TaskId, r.Id })
            .IsDescending(false, true)
            .IncludeProperties(r => new { r.Clarification, r.ToTaskId })
            .HasDatabaseName("IX_Rollbacks_TaskId_Id");

        modelBuilder
            .Entity<Models.Task>()
            .HasOne(t => t.Group)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull); // Already SetNull, which is usually fine

        modelBuilder
            .Entity<Team>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Team)
            .OnDelete(DeleteBehavior.SetNull); // Already SetNull, which is usually fine

        modelBuilder.Entity<User>().HasIndex(u => u.Code).IsUnique(true);

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .OnDelete(DeleteBehavior.SetNull); // SetNull here is fine

        modelBuilder
            .Entity<RefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .OnDelete(DeleteBehavior.Cascade); // This is likely okay as RefreshTokens are typically owned by User

        modelBuilder.Entity<User>().Property(u => u.Role).HasDefaultValue(UserRoleEnum.Member);

        modelBuilder
            .Entity<Subject>()
            .Property(p => p.Status)
            .HasDefaultValue(ProjectStatusEnum.Active);

        modelBuilder.Entity<Models.Task>().Property(_ => _.Pause).HasDefaultValue(false);

        modelBuilder
            .Entity<Year>()
            .HasData(
                new Year { Active = true, Id = 1, Number = "2020" },
                new Year { Active = true, Id = 2, Number = "2021" },
                new Year { Active = true, Id = 3, Number = "2022" },
                new Year { Active = true, Id = 4, Number = "2023" },
                new Year { Active = true, Id = 5, Number = "2024" }
            );

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Subjects)
            .WithMany(s => s.Users)
            .UsingEntity<Dictionary<string, object>>(
                "SubjectUser",
                j =>
                    j.HasOne<Subject>()
                        .WithMany()
                        .HasForeignKey("SubjectsId")
                        .OnDelete(DeleteBehavior.Cascade),
                j =>
                    j.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.NoAction),
                j =>
                {
                    j.HasKey("SubjectsId", "UsersId");
                    j.ToTable("SubjectUser");
                });

        modelBuilder
            .Entity<Rollback>()
            .HasOne(p => p.Task)
            .WithMany(t => t.Rollbacks)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder
            .Entity<Rollback>()
            .HasOne(p => p.ToTask)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<RollbackIssue>()
            .HasOne(p => p.Step)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
        //modelBuilder
        //    .Entity<Sprint>()
        //    .HasMany(x => x.Tasks)
        //    .WithOne(x => x.Sprint)
        //    .OnDelete(DeleteBehavior.NoAction);
        ////////
        modelBuilder
            .Entity<LeaveRequest>()
            .Property(x => x.Type)
            .HasConversion<string>();
        modelBuilder
            .Entity<LeaveRequest>()
            .Property(x => x.Status)
            .HasConversion<string>();
        modelBuilder.Entity<LeaveRequest>()
            .Property(x => x.DateCreated);

        var timeOnlyConverter = new ValueConverter<TimeOnly, TimeSpan>(
            t => t.ToTimeSpan(),
            ts => TimeOnly.FromTimeSpan(ts));

        modelBuilder
            .Entity<Permission>()
            .Property(x => x.Type)
            .HasConversion<string>();
        modelBuilder
            .Entity<Permission>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Permission>()
            .Property(e => e.FromTime)
            .HasConversion(timeOnlyConverter);

        modelBuilder.Entity<Permission>()
           .Property(e => e.ToTime)
           .HasConversion(timeOnlyConverter);


        modelBuilder.Entity<UserChanges>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserChanges)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UserChanges>()
            .HasOne(x => x.ChangedByUser)
            .WithMany(x => x.ChangedByUser)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Opinion>()
            .HasOne(x => x.LeaveRequest)
            .WithMany(x => x.Opinions)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Subject>().ToTable("Subjects");

        modelBuilder.Entity<AcademicYear>().ToTable("AcademicYears");
        modelBuilder.Entity<CurriculumProject>().ToTable("CurriculumProjects");
        modelBuilder.Entity<CurriculumTerm>().ToTable("CurriculumTerms");
        modelBuilder.Entity<SubjectGroup>().ToTable("SubjectGroups");

        modelBuilder.Entity<CurriculumProject>()
            .HasOne(p => p.Year)
            .WithMany(y => y.Projects)
            .HasForeignKey(p => p.YearId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CurriculumTerm>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Terms)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubjectGroup>()
            .HasOne(g => g.Term)
            .WithMany(t => t.SubjectGroups)
            .HasForeignKey(g => g.TermId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Subject>()
            .HasOne(s => s.SubjectGroup)
            .WithMany(g => g.Subjects)
            .HasForeignKey(s => s.SubjectGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Unit>()
            .HasOne(u => u.Subject)
            .WithMany(s => s.Units)
            .HasForeignKey(u => u.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

	        modelBuilder
	            .Entity<Notification>()
	            .Property(n => n.Category)
	            .HasConversion<string>();

	        modelBuilder
	            .Entity<Notification>()
	            .Property(n => n.Type)
	            .HasConversion<string>();

	        modelBuilder
	            .Entity<Notification>()
	            .Property(n => n.Status)
	            .HasConversion<string>();

	        modelBuilder
	            .Entity<Notification>()
	            .Property(n => n.IsRead)
	            .HasDefaultValue(false);

	        modelBuilder
	            .Entity<Notification>()
	            .Property(n => n.HasActions)
	            .HasDefaultValue(false);
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Year> Years => Set<Year>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<CurriculumProject> CurriculumProjects => Set<CurriculumProject>();
    public DbSet<CurriculumTerm> CurriculumTerms => Set<CurriculumTerm>();
    public DbSet<SubjectGroup> SubjectGroups => Set<SubjectGroup>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Schema> Schemas => Set<Schema>();
    public DbSet<SchemaType> SchemaTypes => Set<SchemaType>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<TaskActivity> TaskActivities => Set<TaskActivity>();
    public DbSet<Models.Task> Tasks => Set<Models.Task>();
    public DbSet<TaskBank> TaskBank => Set<TaskBank>();
    public DbSet<TaskWorkTime> TaskWorkTimes => Set<TaskWorkTime>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();
    public DbSet<Rollback> Rollbacks => Set<Rollback>();
    public DbSet<RollbackIssue> RollbackIssues => Set<RollbackIssue>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<SectionGroup> SectionGroups => Set<SectionGroup>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveResetLog> LeaveResetLogs => Set<LeaveResetLog>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<WorkFromHomeRequest> WorkFromHomeRequests => Set<WorkFromHomeRequest>();
    public DbSet<UserChanges> UserChanges => Set<UserChanges>();
    public DbSet<Opinion> Opinions => Set<Opinion>();
    public DbSet<SprintLearningObjective> SprintLearningObjectives => Set<SprintLearningObjective>();
	    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DailyReportNoteOverride> DailyReportNoteOverrides => Set<DailyReportNoteOverride>();
}