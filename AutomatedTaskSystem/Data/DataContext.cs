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
            .OnDelete(DeleteBehavior.NoAction); // Explicitly NoAction

        // 2. SectionGroup to Group (as per your Group model having sectionGroups)
        modelBuilder.Entity<SectionGroup>()
            .HasOne(sg => sg.Group)
            .WithMany(g => g.sectionGroups)
            .HasForeignKey(sg => sg.GroupId)
            .OnDelete(DeleteBehavior.NoAction); // Explicitly NoAction

        // 3. Section to User (Head) - Assuming HeadId is nullable, SetNull is fine.
        // If HeadId is non-nullable, it *must* be NoAction.
        modelBuilder.Entity<Section>()
            .HasOne(s => s.Head)
            .WithMany() // Assuming User doesn't have a direct collection for sections it heads
            .HasForeignKey(s => s.HeadId)
            .IsRequired(false) // Assuming HeadId is nullable (int?)
            .OnDelete(DeleteBehavior.NoAction); // SetNull is also an option if nullable, but NoAction is safer for cycles.

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
            .Entity<Project>()
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

        modelBuilder.Entity<Project>().Property(p => p.YearId).HasDefaultValue(1);

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

    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Year> Years => Set<Year>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
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
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<WorkFromHomeRequest> WorkFromHomeRequests => Set<WorkFromHomeRequest>();
    public DbSet<UserChanges> UserChanges => Set<UserChanges>();
    public DbSet<Opinion> Opinions => Set<Opinion>();
}