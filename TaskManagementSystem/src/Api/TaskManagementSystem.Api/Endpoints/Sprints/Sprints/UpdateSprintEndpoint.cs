using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.UpdateSprint;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>PUT /sprints/{id} — update a sprint. Requires Sprints.Update permission.</summary>
public static class UpdateSprintEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSprintRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSprintCommand(id, request.Name, request.Description, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(sprint => Results.Ok(SprintMapping.MapSprintDetail(sprint)));
    }
}
