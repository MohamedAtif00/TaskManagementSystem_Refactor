using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IEmployeeBalanceRepository
{
    Task<EmployeeBalance?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task DeductLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task RefundLeaveAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task DeductAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);

    Task RefundAnnualLeaveAsync(int userId, int workingDays, CancellationToken cancellationToken = default);
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
