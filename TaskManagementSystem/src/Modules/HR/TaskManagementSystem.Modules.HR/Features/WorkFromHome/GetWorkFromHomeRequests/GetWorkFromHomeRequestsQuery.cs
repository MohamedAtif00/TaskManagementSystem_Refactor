using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequests;

public sealed record GetWorkFromHomeRequestsQuery(int UserId) : IQuery<Result<IReadOnlyList<WorkFromHomeRequestResult>>>;

