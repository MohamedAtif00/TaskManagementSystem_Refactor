using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

public sealed record UserListItemResult(
    int Id,
    string Code,
    string Name,
    int RoleId,
    string RoleName,
    int? TeamId,
    string? TeamName)
{
    public static UserListItemResult From(UserListItemReadModel user) =>
        new(user.Id, user.Code, user.Name, user.RoleId, user.RoleName, user.TeamId, user.TeamName);
}

public sealed record UserDetailResult(
    int Id,
    string Code,
    string Name,
    string HrCode,
    string? Email,
    string? Phone,
    string? Title,
    int RoleId,
    string RoleName,
    int AccountType,
    bool OnBoard,
    int? TeamId,
    string? TeamName,
    int? TeamleaderId,
    string? TeamleaderName)
{
    public static UserDetailResult From(UserDetailReadModel user) =>
        new(
            user.Id,
            user.Code,
            user.Name,
            user.HrCode,
            user.Email,
            user.Phone,
            user.Title,
            user.RoleId,
            user.RoleName,
            user.AccountType,
            user.OnBoard,
            user.TeamId,
            user.TeamName,
            user.TeamleaderId,
            user.TeamleaderName);
}

public sealed record TeamLeaderResult(int Id, string Name, int RoleId, string RoleName)
{
    public static TeamLeaderResult From(TeamLeaderReadModel leader) =>
        new(leader.Id, leader.Name, leader.RoleId, leader.RoleName);
}
