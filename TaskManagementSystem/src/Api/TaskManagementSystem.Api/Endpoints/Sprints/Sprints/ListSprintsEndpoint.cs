using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>GET /sprints — list sprints. Requires Sprints.Read permission.</summary>
public static class ListSprintsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken,
        string? archived = null,
        int? page = null,
        int? pageSize = null)
    {
        var parsedArchived = OptionalQueryBinding.ParseOptionalBool(archived);
        if (page.HasValue || pageSize.HasValue)
        {
            var pagedResult = await mediator.Send(
                new ListSprintsPagedQuery(parsedArchived, page, pageSize),
                cancellationToken);
            return pagedResult.ToHttpResult(pageResult => Results.Ok(new SprintListPageResponse
            {
                Items = pageResult.Items.Select(SprintMapping.MapSprintListItem).ToList(),
                Page = pageResult.Page,
                PageSize = pageResult.PageSize,
                TotalCount = pageResult.TotalCount
            }));
        }

        var result = await mediator.Send(new ListSprintsQuery(parsedArchived), cancellationToken);
        return result.ToHttpResult(sprints =>
            Results.Ok(sprints.Select(SprintMapping.MapSprintListItem).ToList()));
    }
}
