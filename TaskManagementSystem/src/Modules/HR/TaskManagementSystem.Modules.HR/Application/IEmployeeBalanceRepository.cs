using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IEmployeeBalanceRepository
{
    Task AddAsync(EmployeeBalanceRecord balance, CancellationToken cancellationToken = default);

    Task<EmployeeBalanceRecord?> GetTrackedByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<EmployeeBalance?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<NoValue>> DeductLeaveAsync(
        LeaveRequest leaveRequest,
        int fromNextBalanceMaxDays,
        CancellationToken cancellationToken = default);

    Task RefundLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task<Result<NoValue>> DeductAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);

    Task RefundAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);

    Task<Result<NoValue>> DeductPermissionAsync(int userId, CancellationToken cancellationToken = default);

    Task RefundPermissionAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<NoValue>> DeductWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default);

    Task RefundWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default);
}

public static class LeaveBalanceOperations
{
    public static async Task<Result<NoValue>> DeductAsync(
        IEmployeeBalanceRepository repository,
        LeaveRequest leaveRequest,
        int fromNextBalanceMaxDays,
        CancellationToken cancellationToken) =>
        await repository.DeductLeaveAsync(leaveRequest, fromNextBalanceMaxDays, cancellationToken);

    public static async Task RefundAsync(
        IEmployeeBalanceRepository repository,
        LeaveRequest leaveRequest,
        CancellationToken cancellationToken) =>
        await repository.RefundLeaveAsync(leaveRequest, cancellationToken);
}
