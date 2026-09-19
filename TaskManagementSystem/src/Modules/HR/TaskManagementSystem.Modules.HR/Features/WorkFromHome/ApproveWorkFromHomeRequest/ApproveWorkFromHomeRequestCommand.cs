using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.ApproveWorkFromHomeRequest;

public sealed record ApproveWorkFromHomeRequestCommand(int ActorUserId, int WorkFromHomeRequestId)
    : ICommand<Result<WorkFromHomeRequestResult>>;

