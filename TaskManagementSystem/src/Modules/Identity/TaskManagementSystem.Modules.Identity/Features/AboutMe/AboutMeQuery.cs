using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.AboutMe;

public sealed record AboutMeQuery(int UserId) : IQuery<AboutMeResult>;

public sealed record AboutMeResult(
    int Id,
    string Name,
    UserRole Role,
    string? Group,
    int Notifications);
