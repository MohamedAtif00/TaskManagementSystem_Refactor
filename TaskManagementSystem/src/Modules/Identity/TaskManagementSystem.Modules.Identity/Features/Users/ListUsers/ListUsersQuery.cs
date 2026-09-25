using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

public sealed record ListUsersQuery : IQuery<Result<IReadOnlyList<UserListItemResult>>>;

public sealed record ListUsersPagedQuery(string? Search, int? Page, int? PageSize)
    : IQuery<Result<PageListResult<UserListItemResult>>>;
