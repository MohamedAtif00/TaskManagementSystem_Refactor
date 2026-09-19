using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

public sealed record GiveWorkFromHomeOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int WorkFromHomeRequestId,
    bool IsApproved,
    string? Comment) : ICommand<Result<WorkFromHomeRequestResult>>;

