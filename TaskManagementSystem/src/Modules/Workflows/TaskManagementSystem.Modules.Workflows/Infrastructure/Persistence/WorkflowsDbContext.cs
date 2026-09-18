using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

public sealed class WorkflowsDbContext(DbContextOptions<WorkflowsDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowSchema> Schemas => Set<WorkflowSchema>();
    public DbSet<SchemaType> SchemaTypes => Set<SchemaType>();
    public DbSet<TaskBankItem> TaskBank => Set<TaskBankItem>();
    public DbSet<WorkflowNode> Nodes => Set<WorkflowNode>();
    public DbSet<WorkflowStep> Steps => Set<WorkflowStep>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowsDbContext).Assembly);
        modelBuilder.ConfigureOutboxInbox("workflows");
    }
}
