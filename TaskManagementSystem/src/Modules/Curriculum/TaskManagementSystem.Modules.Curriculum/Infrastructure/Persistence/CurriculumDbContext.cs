using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

public sealed class CurriculumDbContext(DbContextOptions<CurriculumDbContext> options) : DbContext(options)
{
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<CurriculumProject> CurriculumProjects => Set<CurriculumProject>();
    public DbSet<CurriculumTerm> CurriculumTerms => Set<CurriculumTerm>();
    public DbSet<SubjectGroup> SubjectGroups => Set<SubjectGroup>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();
    internal DbSet<SubjectUserAssignment> SubjectUserAssignments => Set<SubjectUserAssignment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurriculumDbContext).Assembly);
        modelBuilder.ConfigureOutboxInbox("curriculum");
    }
}
