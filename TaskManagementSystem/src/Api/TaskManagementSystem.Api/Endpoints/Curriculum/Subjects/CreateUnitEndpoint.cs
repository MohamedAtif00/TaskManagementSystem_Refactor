using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Units.CreateUnit;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Creates a unit under a subject.
/// </summary>
public static class CreateUnitEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapPost("/{subjectId:int}/units", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        CreateUnitRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateUnitCommand(subjectId, request.Name), cancellationToken);
        return result.ToHttpResult(unit => Results.Created($"/curriculum/units/{unit.Id}", CurriculumMapping.MapUnitDetail(unit)));
    }
}
