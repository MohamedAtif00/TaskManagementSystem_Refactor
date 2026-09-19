using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequestById;

public sealed record GetWorkFromHomeRequestByIdQuery(int UserId, string UserRole, int WorkFromHomeRequestId)
    : IQuery<Result<WorkFromHomeRequestResult>>;

