using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Units.GetUnitById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

/// <summary>
/// Gets a unit by identifier.
/// </summary>
public static class GetUnitByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder units)
    {
        units.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return units;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
        return result.ToHttpResult(unit => Results.Ok(CurriculumMapping.MapUnitDetail(unit)));
    }
}
