using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence;

public sealed class SprintsDbContext(DbContextOptions<SprintsDbContext> options) : DbContext(options)
{
    public DbSet<Sprint> Sprints => Set<Sprint>();
    internal DbSet<SprintLearningObjective> SprintLearningObjectives => Set<SprintLearningObjective>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SprintsDbContext).Assembly);
        modelBuilder.ConfigureOutboxInbox("sprints");
    }
}
