using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Sections.UpdateSection;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

/// <summary>
/// PUT /organization/sections/{id} — Updates a section.
/// </summary>
public static class UpdateSectionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder sections)
    {
        sections.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Update);
        return sections;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSectionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSectionCommand(id, request.Name, request.HeadId, request.TeamIds),
            cancellationToken);

        return result.ToHttpResult(section => Results.Ok(SectionMapping.MapSectionDetail(section)));
    }
}
