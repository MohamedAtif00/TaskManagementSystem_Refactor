using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

public sealed record ListUsersQuery : IQuery<Result<IReadOnlyList<UserListItemResult>>>;

public sealed class ListUsersQueryHandler(IUserAdminQueries userAdminQueries)
    : IRequestHandler<ListUsersQuery, Result<IReadOnlyList<UserListItemResult>>>
{
    public async Task<Result<IReadOnlyList<UserListItemResult>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userAdminQueries.ListActiveAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<UserListItemResult>>(
            users.Select(UserListItemResult.From).ToList());
    }
}
