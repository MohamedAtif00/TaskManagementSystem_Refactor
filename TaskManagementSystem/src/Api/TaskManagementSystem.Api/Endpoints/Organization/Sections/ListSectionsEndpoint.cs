using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Sections.ListSections;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

/// <summary>
/// GET /organization/sections — Lists all sections.
/// </summary>
public static class ListSectionsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder sections)
    {
        sections.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Read);
        return sections;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSectionsQuery(), cancellationToken);
        return result.ToHttpResult(sections =>
            Results.Ok(sections.Select(SectionMapping.MapSectionListItem).ToList()));
    }
}
