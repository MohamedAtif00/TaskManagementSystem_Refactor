using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Units.ArchiveUnit;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

/// <summary>
/// Archives a unit.
/// </summary>
public static class ArchiveUnitEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder units)
    {
        units.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return units;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveUnitCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
