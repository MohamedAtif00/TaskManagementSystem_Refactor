using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

public sealed class ListUsersPagedQueryHandler(IUserAdminQueries userAdminQueries)
    : IRequestHandler<ListUsersPagedQuery, Result<PageListResult<UserListItemResult>>>
{
    public async Task<Result<PageListResult<UserListItemResult>>> Handle(
        ListUsersPagedQuery request,
        CancellationToken cancellationToken)
    {
        var page = await userAdminQueries.ListActivePagedAsync(
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        if (page.IsFailure)
        {
            return Result.Fail<PageListResult<UserListItemResult>>(page.Error);
        }

        return Result.Ok(new PageListResult<UserListItemResult>(
            page.Value.Items.Select(UserListItemResult.From).ToList(),
            page.Value.Page,
            page.Value.PageSize,
            page.Value.TotalCount));
    }
}
