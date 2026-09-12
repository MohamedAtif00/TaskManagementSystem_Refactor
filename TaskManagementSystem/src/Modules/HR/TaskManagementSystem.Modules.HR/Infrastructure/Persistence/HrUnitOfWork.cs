using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class HrUnitOfWork(
    HrDbContext context,
    LeaveRequestSearchQueries leaveRequestSearchQueries)
    : UnitOfWork<HrDbContext>(context), IHrUnitOfWork
{
    private readonly Lazy<LeaveRequestRepository> _leaveRequests =
        LazyRepositoryFactory.Create(() => new LeaveRequestRepository(context, leaveRequestSearchQueries));

    private readonly Lazy<EmployeeBalanceRepository> _employeeBalances =
        LazyRepositoryFactory.Create(() => new EmployeeBalanceRepository(context));

    private readonly Lazy<OpinionRepository> _opinions =
        LazyRepositoryFactory.Create(() => new OpinionRepository(context));

    private readonly Lazy<HolidayRepository> _holidays =
        LazyRepositoryFactory.Create(() => new HolidayRepository(context));

    public ILeaveRequestRepository LeaveRequests => _leaveRequests.Value;

    public IEmployeeBalanceRepository EmployeeBalances => _employeeBalances.Value;

    public IOpinionRepository Opinions => _opinions.Value;

    public IHolidayRepository Holidays => _holidays.Value;
}
