using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.RequestForgotClock;

public sealed record RequestForgotClockCommand(
    int UserId,
    string RequesterRole,
    ForgotClockPunchType PunchType,
    DateTime AttendanceDate,
    TimeOnly IntendedTime,
    string? Reason) : ICommand<Result<ForgotClockRequestResult>>;

