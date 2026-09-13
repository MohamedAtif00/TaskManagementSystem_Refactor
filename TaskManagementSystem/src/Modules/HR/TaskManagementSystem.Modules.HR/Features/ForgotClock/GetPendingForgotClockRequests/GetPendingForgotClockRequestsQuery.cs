using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetPendingForgotClockRequests;

public sealed record GetPendingForgotClockRequestsQuery : IQuery<Result<IReadOnlyList<ForgotClockRequestResult>>>;

public sealed class GetPendingForgotClockRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingForgotClockRequestsQuery, Result<IReadOnlyList<ForgotClockRequestResult>>>
{
    public async Task<Result<IReadOnlyList<ForgotClockRequestResult>>> Handle(
        GetPendingForgotClockRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.ForgotClockRequests.GetPendingAsync(cancellationToken);
        var results = items.Select(r => ForgotClockRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<ForgotClockRequestResult>>(results);
    }
}
