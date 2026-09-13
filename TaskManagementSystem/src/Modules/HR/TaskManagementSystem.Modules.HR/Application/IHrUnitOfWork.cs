using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IHrUnitOfWork : IUnitOfWork
{
    ILeaveRequestRepository LeaveRequests { get; }

    IPermissionRequestRepository PermissionRequests { get; }

    IWorkFromHomeRequestRepository WorkFromHomeRequests { get; }

    IForgotClockRequestRepository ForgotClockRequests { get; }

    IEmployeeBalanceRepository EmployeeBalances { get; }

    IOpinionRepository Opinions { get; }

    IHolidayRepository Holidays { get; }
}
