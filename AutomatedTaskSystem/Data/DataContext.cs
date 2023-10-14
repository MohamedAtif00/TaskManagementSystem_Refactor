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

		modelBuilder.Entity<Comment>().HasOne(c => c.Child).WithOne();

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

		modelBuilder.Entity<User>().Property(u => u.Role).HasDefaultValue(UserRoleEnum.Member);

		modelBuilder
			.Entity<Project>()
			.Property(p => p.Status)
			.HasDefaultValue(ProjectStatusEnum.Active);

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

		modelBuilder.Entity<Rollback>().HasOne(p => p.FromTask).WithMany().OnDelete(DeleteBehavior.NoAction);
		modelBuilder.Entity<Rollback>().HasOne(p => p.ToTask).WithMany().OnDelete(DeleteBehavior.NoAction);

		modelBuilder.Entity<RollbackIssue>().HasOne(p => p.Step).WithMany().OnDelete(DeleteBehavior.NoAction);
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
}
