using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>POST /sprints — create a sprint. Requires Sprints.Create permission.</summary>
public static class CreateSprintEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        CreateSprintRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSprintCommand(request.Name, request.Description, request.StartDate, request.EndDate),
            cancellationToken);

        return result.ToHttpResult(sprint =>
            Results.Created($"/sprints/{sprint.Id}", SprintMapping.MapSprintDetail(sprint)));
    }
}
