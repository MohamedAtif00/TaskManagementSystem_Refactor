namespace TaskManagementSystem.Modules.Identity.Application;

public sealed record UserListItemReadModel(
    int Id,
    string Code,
    string Name,
    int RoleId,
    string RoleName,
    int? TeamId,
    string? TeamName);

public sealed record UserDetailReadModel(
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
    string? TeamleaderName);

public sealed record TeamLeaderReadModel(
    int Id,
    string Name,
    int RoleId,
    string RoleName);
