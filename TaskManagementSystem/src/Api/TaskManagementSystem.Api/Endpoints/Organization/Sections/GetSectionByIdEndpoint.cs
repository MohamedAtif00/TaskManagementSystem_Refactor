using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Sections.GetSectionById;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

/// <summary>
/// GET /organization/sections/{id} — Gets a section by ID.
/// </summary>
public static class GetSectionByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder sections)
    {
        sections.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Read);
        return sections;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSectionByIdQuery(id), cancellationToken);
        return result.ToHttpResult(section => Results.Ok(SectionMapping.MapSectionDetail(section)));
    }
}
