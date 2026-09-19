using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveBulkWorkFromHomeOpinion;

public sealed record GiveBulkWorkFromHomeOpinionCommand(
    int ActorUserId,
    string ActorRole,
    IReadOnlyList<int> WorkFromHomeRequestIds,
    bool IsApproved,
    string? Comment) : IHrCommand<Result<BulkWorkFromHomeOpinionResult>>;

