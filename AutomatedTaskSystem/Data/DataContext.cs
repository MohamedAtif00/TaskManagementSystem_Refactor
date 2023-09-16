using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.SchemaTypesModel;
using AutomatedTaskSystem.Models.YearModel;

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

        modelBuilder
            .Entity<Node>()
            .HasMany(n => n.Required)
            .WithMany(n => n.Requires)
            .UsingEntity(j => j.ToTable("NodeDependencies"));

        modelBuilder
            .Entity<Step>()
            .HasMany(n => n.Rollbacks)
            .WithMany(n => n.From)
            .UsingEntity(j => j.ToTable("RSteps"));

        modelBuilder
            .Entity<LearningObjective>()
            .HasOne(lo => lo.Schema)
            .WithMany(s => s.LearningObjectives);

        modelBuilder
            .Entity<Models.Task>()
            .HasOne(t => t.Group)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Team>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Team)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>().HasIndex(u => u.Code).IsUnique(true);

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<RefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .OnDelete(DeleteBehavior.Cascade);

        var creation = new Models.Type { Id = 1, Name = "Creation" };
        var review = new Models.Type { Id = 3, Name = "Review" };

        modelBuilder.Entity<Models.Type>().HasData(creation, review);

        modelBuilder
            .Entity<ActivityType>()
            .HasData(
                new ActivityType { Id = 1, Name = "Start" },
                new ActivityType { Id = 2, Name = "Done" }
            );

        modelBuilder
            .Entity<EndActivityType>()
            .HasData(
                new ActivityType { Id = 1, Name = "Flag" },
                new ActivityType { Id = 2, Name = "Pause" },
                new ActivityType { Id = 3, Name = "Complete" },
                new ActivityType { Id = 4, Name = "Session" },
                new ActivityType { Id = 5, Name = "Reassign" },
                new ActivityType { Id = 6, Name = "Change of Schema" }
            );

        modelBuilder.Entity<Models.TaskBank>().Property(t => t.TypeId).HasDefaultValue(1);

        modelBuilder.Entity<User>().Property(u => u.Role).HasDefaultValue(UserRoleEnum.Member);

        modelBuilder
            .Entity<Project>()
            .Property(p => p.Status)
            .HasDefaultValue(ProjectStatusEnum.Active);

        modelBuilder
            .Entity<Assignment>()
            .Property(_ => _.TimeStamp)
            .HasDefaultValue(new DateTime(2023, 01, 01));

        modelBuilder.Entity<Models.Task>().Property(_ => _.Pause).HasDefaultValue(false);

        modelBuilder
            .Entity<Year>()
            .HasData(
                new Year
                {
                    Active = true,
                    Id = 1,
                    Number = "2020"
                },
                new Year
                {
                    Active = true,
                    Id = 2,
                    Number = "2021"
                },
                new Year
                {
                    Active = true,
                    Id = 3,
                    Number = "2022"
                },
                new Year
                {
                    Active = true,
                    Id = 4,
                    Number = "2023"
                },
                new Year
                {
                    Active = true,
                    Id = 5,
                    Number = "2024"
                }
            );

        modelBuilder.Entity<Project>().Property(p => p.YearId).HasDefaultValue(1);
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Year> Years => Set<Year>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<EndActivityType> EndActivityTypes => Set<EndActivityType>();
    public DbSet<EndActivity> EndActivities => Set<EndActivity>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Models.Type> Types => Set<Models.Type>();
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
    public DbSet<Models.Task> Tasks => Set<Models.Task>();
    public DbSet<TaskBank> TaskBank => Set<TaskBank>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();
}
