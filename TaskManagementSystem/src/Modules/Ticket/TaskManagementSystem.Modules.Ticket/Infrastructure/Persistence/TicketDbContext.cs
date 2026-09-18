using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options)
{
    public DbSet<TicketTask> TicketTasks => Set<TicketTask>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TaskWorkTime> TaskWorkTimes => Set<TaskWorkTime>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketDbContext).Assembly);
        modelBuilder.ConfigureOutboxInbox("ticket");
    }
}
