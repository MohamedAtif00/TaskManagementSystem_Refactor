using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.CancelWorkFromHomeRequest;

public sealed record CancelWorkFromHomeRequestCommand(int UserId, int WorkFromHomeRequestId)
    : ICommand<Result<WorkFromHomeRequestResult>>;

