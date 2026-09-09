using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.AboutMe;

public sealed record AboutMeQuery(int UserId) : IQuery<Result<AboutMeResult>>;

public sealed record AboutMeResult(
    int Id,
    string Name,
    int Role,
    string RoleName,
    IReadOnlyList<string> Permissions,
    string? Group,
    int Notifications);
