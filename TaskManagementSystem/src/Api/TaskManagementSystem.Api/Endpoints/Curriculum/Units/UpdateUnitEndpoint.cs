using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Units.UpdateUnit;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

/// <summary>
/// Updates an existing unit.
/// </summary>
public static class UpdateUnitEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder units)
    {
        units.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return units;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateUnitRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateUnitCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(unit => Results.Ok(CurriculumMapping.MapUnitDetail(unit)));
    }
}
