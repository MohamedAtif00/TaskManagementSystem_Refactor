using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options)
{
    public DbSet<TicketTask> TicketTasks => Set<TicketTask>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TaskWorkTime> TaskWorkTimes => Set<TaskWorkTime>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketDbContext).Assembly);
}
