using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

public sealed class HrDbContext(DbContextOptions<HrDbContext> options) : DbContext(options)
{
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Opinion> Opinions => Set<Opinion>();
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();
    internal DbSet<EmployeeBalanceRecord> EmployeeBalances => Set<EmployeeBalanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
}
