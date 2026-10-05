using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure;

namespace TaskManagementSystem.Modules.HR.Features.Leave.UpdateEmployeeBalance;

public sealed class UpdateEmployeeBalanceCommandHandler(
    IHrUnitOfWork unitOfWork,
    LeaveRequestPlanner planner)
    : IRequestHandler<UpdateEmployeeBalanceCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(UpdateEmployeeBalanceCommand request, CancellationToken cancellationToken)
    {
        if (request.ViewerRole is not ("Owner" or "ProjectManger"))
        {
            return Result.Fail<NoValue>(HrErrors.BalanceUpdateNotAuthorized);
        }

        var balance = await unitOfWork.EmployeeBalances.GetTrackedByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<NoValue>(HrErrors.UserNotFound);
        }

        var updated = balance.SetAmounts(
            request.AnnualLeave,
            request.AnnualLeaveMax,
            request.EmergencyLeave,
            request.EmergencyLeaveMax,
            request.SickLeave,
            request.Permission,
            request.PermissionMax,
            request.WorkFromHome,
            request.WorkFromHomeMax,
            request.FromNextBalanceDaysUsed,
            planner.Settings.FromNextBalanceMaxDays);
        if (!updated.IsSuccess)
        {
            return Result.Fail<NoValue>(updated.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
