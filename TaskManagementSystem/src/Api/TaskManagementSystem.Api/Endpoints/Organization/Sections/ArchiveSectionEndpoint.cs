using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Sections.ArchiveSection;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

/// <summary>
/// DELETE /organization/sections/{id} — Archives a section.
/// </summary>
public static class ArchiveSectionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder sections)
    {
        sections.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Delete);
        return sections;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSectionCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
