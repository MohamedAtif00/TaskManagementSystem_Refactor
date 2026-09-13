using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetPendingWorkFromHomeRequests;

public sealed record GetPendingWorkFromHomeRequestsQuery : IQuery<Result<IReadOnlyList<WorkFromHomeRequestResult>>>;

public sealed class GetPendingWorkFromHomeRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingWorkFromHomeRequestsQuery, Result<IReadOnlyList<WorkFromHomeRequestResult>>>
{
    public async Task<Result<IReadOnlyList<WorkFromHomeRequestResult>>> Handle(
        GetPendingWorkFromHomeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await unitOfWork.WorkFromHomeRequests.GetPendingAsync(cancellationToken);
        var results = requests.Select(r => WorkFromHomeRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<WorkFromHomeRequestResult>>(results);
    }
}
