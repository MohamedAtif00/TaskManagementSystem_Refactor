using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class EmployeeBalanceRepository(HrDbContext context) : IEmployeeBalanceRepository
{
    public async Task<EmployeeBalance?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(balance => balance.UserId == userId, cancellationToken);

        return entity is null ? null : MapToModel(entity);
    }

    public async Task DeductLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .FirstAsync(balance => balance.UserId == leaveRequest.UserId, cancellationToken);

        switch (leaveRequest.Type)
        {
            case LeaveType.Annual:
                entity.AnnualLeave += leaveRequest.WorkingDays;
                break;
            case LeaveType.Emergency:
                entity.EmergencyLeave += leaveRequest.WorkingDays;
                break;
            case LeaveType.Sick:
                entity.SickLeave += leaveRequest.WorkingDays;
                break;
            case LeaveType.FromNextBalance:
                entity.FromNextBalanceDaysUsed += leaveRequest.WorkingDays;
                break;
        }
    }

    public async Task RefundLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .FirstAsync(balance => balance.UserId == leaveRequest.UserId, cancellationToken);

        switch (leaveRequest.Type)
        {
            case LeaveType.Annual:
                entity.AnnualLeave = Math.Max(0, entity.AnnualLeave - leaveRequest.WorkingDays);
                break;
            case LeaveType.Emergency:
                entity.EmergencyLeave = Math.Max(0, entity.EmergencyLeave - leaveRequest.WorkingDays);
                break;
            case LeaveType.Sick:
                entity.SickLeave = Math.Max(0, entity.SickLeave - leaveRequest.WorkingDays);
                break;
            case LeaveType.FromNextBalance:
                entity.FromNextBalanceDaysUsed = Math.Max(0, entity.FromNextBalanceDaysUsed - leaveRequest.WorkingDays);
                break;
        }
    }

    public async Task DeductAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .FirstAsync(balance => balance.UserId == userId, cancellationToken);
        entity.AnnualLeave += workingDays;
    }

    public async Task RefundAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .FirstAsync(balance => balance.UserId == userId, cancellationToken);
        entity.AnnualLeave = Math.Max(0, entity.AnnualLeave - workingDays);
    }

    private static EmployeeBalance MapToModel(EmployeeBalanceRecord entity) =>
        new()
        {
            Id = entity.UserId,
            TeamId = entity.TeamId,
            TeamleaderId = entity.TeamleaderId,
            AnnualLeave = entity.AnnualLeave,
            AnnualLeaveMax = entity.AnnualLeaveMax,
            EmergencyLeave = entity.EmergencyLeave,
            EmergencyLeaveMax = entity.EmergencyLeaveMax,
            SickLeave = entity.SickLeave,
            Permission = entity.Permission,
            PermissionMax = entity.PermissionMax,
            WorkFromHome = entity.WorkFromHome,
            WorkFromHomeMax = entity.WorkFromHomeMax,
            FromNextBalanceDaysUsed = entity.FromNextBalanceDaysUsed,
            OldAnnualBalance = entity.OldAnnualBalance
        };
}
