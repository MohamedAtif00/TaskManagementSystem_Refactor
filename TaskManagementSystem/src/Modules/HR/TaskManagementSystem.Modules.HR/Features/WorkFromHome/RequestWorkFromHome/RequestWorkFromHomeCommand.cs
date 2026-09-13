using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.RequestWorkFromHome;

public sealed record RequestWorkFromHomeCommand(
    int UserId,
    string RequesterRole,
    DateTime Date,
    string? NoteForManager) : ICommand<Result<WorkFromHomeRequestResult>>;

public sealed class RequestWorkFromHomeCommandHandler(
    IHrUnitOfWork unitOfWork,
    OrgLookupQueries orgLookupQueries,
    TimeProvider timeProvider)
    : IRequestHandler<RequestWorkFromHomeCommand, Result<WorkFromHomeRequestResult>>
{
    public async Task<Result<WorkFromHomeRequestResult>> Handle(
        RequestWorkFromHomeCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.UserNotFound);
        }

        if (await unitOfWork.WorkFromHomeRequests.ExistsActiveForDateAsync(
                request.UserId,
                request.Date,
                cancellationToken: cancellationToken))
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.WorkFromHomeDuplicateDate);
        }

        var pendingCount = await unitOfWork.WorkFromHomeRequests.CountPendingAsync(request.UserId, cancellationToken: cancellationToken);
        if (!balance.HasAvailableWorkFromHome(pendingCount))
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.WorkFromHomeInsufficientBalance);
        }

        var (teamleaderId, sectionheadId) = await HrRequestContextHelper.ResolveApproversAsync(
            unitOfWork,
            orgLookupQueries,
            request.UserId,
            request.RequesterRole,
            cancellationToken);

        var createResult = WorkFromHomeRequest.Create(
            request.UserId,
            request.Date,
            request.NoteForManager,
            teamleaderId,
            sectionheadId,
            utcNow);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrResultMapper.ToApplicationError(createResult.Error));
        }

        await unitOfWork.WorkFromHomeRequests.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(WorkFromHomeRequestResult.From(createResult.Value));
    }
}
