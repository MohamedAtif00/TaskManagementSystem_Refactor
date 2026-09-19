using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    int UserId,
    string Name,
    string HrCode,
    string? Email,
    string? Phone,
    string? Title,
    int RoleId,
    AccountType AccountType,
    int? TeamId) : ICommand<Result<UserDetailResult>>;
