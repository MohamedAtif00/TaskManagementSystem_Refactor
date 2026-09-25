using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// GET /identity/users — Lists all users.
/// </summary>
public static class ListUsersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapGet("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Read);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken,
        string? search = null,
        int? page = null,
        int? pageSize = null)
    {
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(search))
        {
            var pagedResult = await mediator.Send(new ListUsersPagedQuery(search, page, pageSize), cancellationToken);
            return pagedResult.ToHttpResult(pageResult => Results.Ok(new UserListPageResponse
            {
                Items = pageResult.Items.Select(IdentityMapping.ToUserListItemResponse).ToList(),
                Page = pageResult.Page,
                PageSize = pageResult.PageSize,
                TotalCount = pageResult.TotalCount
            }));
        }

        var result = await mediator.Send(new ListUsersQuery(), cancellationToken);
        return result.ToHttpResult(users => Results.Ok(users.Select(IdentityMapping.ToUserListItemResponse)));
    }
}
