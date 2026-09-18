using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

public sealed class HrDbContext(DbContextOptions<HrDbContext> options) : DbContext(options)
{
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PermissionRequest> PermissionRequests => Set<PermissionRequest>();
    public DbSet<WorkFromHomeRequest> WorkFromHomeRequests => Set<WorkFromHomeRequest>();
    public DbSet<ForgotClockRequest> ForgotClockRequests => Set<ForgotClockRequest>();
    public DbSet<Opinion> Opinions => Set<Opinion>();
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();
    internal DbSet<EmployeeBalanceRecord> EmployeeBalances => Set<EmployeeBalanceRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
        modelBuilder.ConfigureOutboxInbox("hr");
    }
}
