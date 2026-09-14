using Microsoft.EntityFrameworkCore;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurriculumDbContext).Assembly);
}
