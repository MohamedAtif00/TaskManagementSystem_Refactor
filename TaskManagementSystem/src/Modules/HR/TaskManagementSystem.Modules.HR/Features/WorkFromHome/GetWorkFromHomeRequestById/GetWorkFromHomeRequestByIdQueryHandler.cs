using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequestById;

public sealed class GetWorkFromHomeRequestByIdQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkFromHomeRequestByIdQuery, Result<WorkFromHomeRequestResult>>
{
    public async Task<Result<WorkFromHomeRequestResult>> Handle(
        GetWorkFromHomeRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var workFromHome = await unitOfWork.WorkFromHomeRequests.GetByIdAsync(request.WorkFromHomeRequestId, cancellationToken);
        if (workFromHome is null)
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.WorkFromHomeRequestNotFound);
        }

        if (workFromHome.UserId != request.UserId && request.UserRole != "Owner")
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.WorkFromHomeRequestNotFound);
        }

        var opinions = await unitOfWork.Opinions.GetByWorkFromHomeRequestIdAsync(request.WorkFromHomeRequestId, cancellationToken);
        var opinionResults = opinions.Select(OpinionResult.From).ToList();

        return Result.Ok(WorkFromHomeRequestResult.From(workFromHome, opinionResults));
    }
}

