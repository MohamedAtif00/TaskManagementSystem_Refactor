using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class HrUnitOfWork(
    HrDbContext context,
    LeaveRequestSearchQueries leaveRequestSearchQueries,
    PermissionRequestSearchQueries permissionRequestSearchQueries,
    WorkFromHomeRequestSearchQueries workFromHomeRequestSearchQueries,
    ForgotClockRequestSearchQueries forgotClockRequestSearchQueries)
    : UnitOfWork<HrDbContext>(context), IHrUnitOfWork
{
    private readonly Lazy<LeaveRequestRepository> _leaveRequests =
        LazyRepositoryFactory.Create(() => new LeaveRequestRepository(context, leaveRequestSearchQueries));

    private readonly Lazy<PermissionRequestRepository> _permissionRequests =
        LazyRepositoryFactory.Create(() => new PermissionRequestRepository(context, permissionRequestSearchQueries));

    private readonly Lazy<WorkFromHomeRequestRepository> _workFromHomeRequests =
        LazyRepositoryFactory.Create(() => new WorkFromHomeRequestRepository(context, workFromHomeRequestSearchQueries));

    private readonly Lazy<ForgotClockRequestRepository> _forgotClockRequests =
        LazyRepositoryFactory.Create(() => new ForgotClockRequestRepository(context, forgotClockRequestSearchQueries));

    private readonly Lazy<EmployeeBalanceRepository> _employeeBalances =
        LazyRepositoryFactory.Create(() => new EmployeeBalanceRepository(context));

    private readonly Lazy<OpinionRepository> _opinions =
        LazyRepositoryFactory.Create(() => new OpinionRepository(context));

    private readonly Lazy<HolidayRepository> _holidays =
        LazyRepositoryFactory.Create(() => new HolidayRepository(context));

    public ILeaveRequestRepository LeaveRequests => _leaveRequests.Value;

    public IPermissionRequestRepository PermissionRequests => _permissionRequests.Value;

    public IWorkFromHomeRequestRepository WorkFromHomeRequests => _workFromHomeRequests.Value;

    public IForgotClockRequestRepository ForgotClockRequests => _forgotClockRequests.Value;

    public IEmployeeBalanceRepository EmployeeBalances => _employeeBalances.Value;

    public IOpinionRepository Opinions => _opinions.Value;

    public IHolidayRepository Holidays => _holidays.Value;
}
