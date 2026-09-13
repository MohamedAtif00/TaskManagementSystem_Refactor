using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

public sealed class WorkflowsDbContext(DbContextOptions<WorkflowsDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowSchema> Schemas => Set<WorkflowSchema>();
    public DbSet<SchemaType> SchemaTypes => Set<SchemaType>();
    public DbSet<TaskBankItem> TaskBank => Set<TaskBankItem>();
    public DbSet<WorkflowNode> Nodes => Set<WorkflowNode>();
    public DbSet<WorkflowStep> Steps => Set<WorkflowStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowsDbContext).Assembly);
}
