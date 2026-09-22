using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.ApproveWorkFromHomeRequest;

public sealed class ApproveWorkFromHomeRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveWorkFromHomeRequestCommand, Result<WorkFromHomeRequestResult>>
{
    public async Task<Result<WorkFromHomeRequestResult>> Handle(
        ApproveWorkFromHomeRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorRole != "Owner")
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.HrApproveNotAuthorized);
        }

        return await mediator.Send(
            new GiveWorkFromHomeOpinionCommand(
                request.ActorUserId,
                request.ActorRole,
                request.WorkFromHomeRequestId,
                true,
                null),
            cancellationToken);
    }
}

