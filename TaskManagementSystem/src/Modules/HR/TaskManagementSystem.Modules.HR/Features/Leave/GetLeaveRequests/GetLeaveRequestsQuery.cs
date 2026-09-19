using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveRequests;

public sealed record GetLeaveRequestsQuery(int UserId) : IQuery<Result<IReadOnlyList<LeaveRequestResult>>>;

