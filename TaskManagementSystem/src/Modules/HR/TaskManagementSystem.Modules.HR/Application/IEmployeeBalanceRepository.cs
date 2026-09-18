using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IEmployeeBalanceRepository
{
    Task AddAsync(EmployeeBalanceRecord balance, CancellationToken cancellationToken = default);

    Task<EmployeeBalanceRecord?> GetTrackedByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<EmployeeBalance?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task DeductLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task RefundLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task DeductAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);

    Task RefundAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);

    Task DeductPermissionAsync(int userId, CancellationToken cancellationToken = default);

    Task RefundPermissionAsync(int userId, CancellationToken cancellationToken = default);

    Task DeductWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default);

    Task RefundWorkFromHomeAsync(int userId, CancellationToken cancellationToken = default);
}

public static class LeaveBalanceOperations
{
    public static async Task DeductAsync(
        IEmployeeBalanceRepository repository,
        LeaveRequest leaveRequest,
        CancellationToken cancellationToken) =>
        await repository.DeductLeaveAsync(leaveRequest, cancellationToken);

    public static async Task RefundAsync(
        IEmployeeBalanceRepository repository,
        LeaveRequest leaveRequest,
        CancellationToken cancellationToken) =>
        await repository.RefundLeaveAsync(leaveRequest, cancellationToken);
}
