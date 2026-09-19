using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.RequestWorkFromHome;

public sealed record RequestWorkFromHomeCommand(
    int UserId,
    string RequesterRole,
    DateTime Date,
    string? NoteForManager) : ICommand<Result<WorkFromHomeRequestResult>>;

