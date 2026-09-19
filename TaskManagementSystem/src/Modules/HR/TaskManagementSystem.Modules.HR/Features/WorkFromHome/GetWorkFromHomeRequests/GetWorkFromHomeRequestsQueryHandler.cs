using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequests;

public sealed class GetWorkFromHomeRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkFromHomeRequestsQuery, Result<IReadOnlyList<WorkFromHomeRequestResult>>>
{
    public async Task<Result<IReadOnlyList<WorkFromHomeRequestResult>>> Handle(
        GetWorkFromHomeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await unitOfWork.WorkFromHomeRequests.GetByUserIdAsync(request.UserId, cancellationToken);
        var results = requests.Select(r => WorkFromHomeRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<WorkFromHomeRequestResult>>(results);
    }
}

