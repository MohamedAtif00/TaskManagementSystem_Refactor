using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Sections.CreateSection;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

/// <summary>
/// POST /organization/sections — Creates a new section.
/// </summary>
public static class CreateSectionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder sections)
    {
        sections.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Create);
        return sections;
    }

    private static async Task<IResult> HandleAsync(
        CreateSectionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSectionCommand(request.Name, request.HeadId, request.TeamIds),
            cancellationToken);

        return result.ToHttpResult(section =>
            Results.Created($"/organization/sections/{section.Id}", SectionMapping.MapSectionDetail(section)));
    }
}
