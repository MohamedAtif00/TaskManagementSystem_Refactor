using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class EmployeeBalanceRepository(HrDbContext context) : IEmployeeBalanceRepository
{
    public async Task AddAsync(EmployeeBalanceRecord balance, CancellationToken cancellationToken = default) =>
        await context.EmployeeBalances.AddAsync(balance, cancellationToken);

    public Task<EmployeeBalanceRecord?> GetTrackedByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        context.EmployeeBalances.FirstOrDefaultAsync(balance => balance.UserId == userId, cancellationToken);

    public void DetachTracked(int userId)
    {
        foreach (var entry in context.ChangeTracker.Entries<EmployeeBalanceRecord>()
                     .Where(tracked => tracked.Entity.UserId == userId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task<EmployeeBalance?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.EmployeeBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(balance => balance.UserId == userId, cancellationToken);

        return entity is null ? null : MapToModel(entity);
    }

    public async Task<Result<NoValue>> DeductLeaveAsync(
        LeaveRequest leaveRequest,
        int fromNextBalanceMaxDays,
        CancellationToken cancellationToken = default)
    {
        if (leaveRequest.Type == LeaveType.UnpaidLeave)
        {
            return Result.Ok();
        }

        string? sql = leaveRequest.Type switch
        {
            LeaveType.Annual => EmployeeBalanceSqlMutations.DeductAnnualLeave,
            LeaveType.Emergency => EmployeeBalanceSqlMutations.DeductEmergencyLeave,
            LeaveType.Sick => EmployeeBalanceSqlMutations.DeductSickLeave,
            LeaveType.FromNextBalance => EmployeeBalanceSqlMutations.DeductFromNextBalance,
            _ => null
        };

        if (sql is null)
        {
            return Result.Fail(HrErrors.LeaveTypeNotSupported);
        }

        object parameters = leaveRequest.Type switch
        {
            LeaveType.FromNextBalance => new
            {
                UserId = leaveRequest.UserId,
                Days = leaveRequest.WorkingDays,
                MaxDays = fromNextBalanceMaxDays
            },
            _ => new { UserId = leaveRequest.UserId, Days = leaveRequest.WorkingDays }
        };

        var rowsAffected = await ExecuteMutationAsync(sql, parameters, cancellationToken);
        return rowsAffected > 0 ? Result.Ok() : Result.Fail(HrErrors.InsufficientBalance);
    }

    public async Task RefundLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        if (leaveRequest.Type == LeaveType.UnpaidLeave)
        {
            return;
        }

        string? sql = leaveRequest.Type switch
        {
            LeaveType.Annual => EmployeeBalanceSqlMutations.RefundAnnualLeave,
            LeaveType.Emergency => EmployeeBalanceSqlMutations.RefundEmergencyLeave,
            LeaveType.Sick => EmployeeBalanceSqlMutations.RefundSickLeave,
            LeaveType.FromNextBalance => EmployeeBalanceSqlMutations.RefundFromNextBalance,
            _ => null
        };

        if (sql is null)
        {
            return;
        }

        await ExecuteMutationAsync(
            sql,
            new { UserId = leaveRequest.UserId, Days = leaveRequest.WorkingDays },
            cancellationToken);
    }

    public async Task<Result<NoValue>> DeductAnnualLeaveAsync(
        int userId,
        int workingDays,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.DeductAnnualLeave,
            new { UserId = userId, Days = workingDays },
            cancellationToken);

        return rowsAffected > 0 ? Result.Ok() : Result.Fail(HrErrors.InsufficientBalance);
    }

    public async Task RefundAnnualLeaveAsync(
        int userId,
        int workingDays,
        CancellationToken cancellationToken = default) =>
        await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.RefundAnnualLeave,
            new { UserId = userId, Days = workingDays },
            cancellationToken);

    public async Task<Result<NoValue>> DeductPermissionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.DeductPermission,
            new { UserId = userId },
            cancellationToken);

        return rowsAffected > 0 ? Result.Ok() : Result.Fail(HrErrors.PermissionInsufficientBalance);
    }

    public async Task RefundPermissionAsync(int userId, CancellationToken cancellationToken = default) =>
        await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.RefundPermission,
            new { UserId = userId },
            cancellationToken);

    public async Task<Result<NoValue>> DeductWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.DeductWorkFromHome,
            new { UserId = userId },
            cancellationToken);

        return rowsAffected > 0 ? Result.Ok() : Result.Fail(HrErrors.WorkFromHomeInsufficientBalance);
    }

    public async Task RefundWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default) =>
        await ExecuteMutationAsync(
            EmployeeBalanceSqlMutations.RefundWorkFromHome,
            new { UserId = userId },
            cancellationToken);

    private async Task<int> ExecuteMutationAsync(
        string sql,
        object parameters,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        IDbTransaction? transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
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
