using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequests;

public sealed record GetForgotClockRequestsQuery(int UserId)
    : IQuery<Result<IReadOnlyList<ForgotClockRequestResult>>>;

public sealed class GetForgotClockRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetForgotClockRequestsQuery, Result<IReadOnlyList<ForgotClockRequestResult>>>
{
    public async Task<Result<IReadOnlyList<ForgotClockRequestResult>>> Handle(
        GetForgotClockRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.ForgotClockRequests.GetByUserIdAsync(request.UserId, cancellationToken);
        var results = items.Select(r => ForgotClockRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<ForgotClockRequestResult>>(results);
    }
}
