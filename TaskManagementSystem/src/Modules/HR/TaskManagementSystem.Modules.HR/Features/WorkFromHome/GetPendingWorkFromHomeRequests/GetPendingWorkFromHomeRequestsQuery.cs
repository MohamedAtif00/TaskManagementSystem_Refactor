using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetPendingWorkFromHomeRequests;

public sealed record GetPendingWorkFromHomeRequestsQuery : IQuery<Result<IReadOnlyList<WorkFromHomeRequestResult>>>;

