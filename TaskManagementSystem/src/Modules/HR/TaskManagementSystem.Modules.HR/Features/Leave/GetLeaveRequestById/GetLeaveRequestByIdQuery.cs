using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveRequestById;

public sealed record GetLeaveRequestByIdQuery(int UserId, string UserRole, int LeaveRequestId)
    : IQuery<Result<LeaveRequestResult>>;

